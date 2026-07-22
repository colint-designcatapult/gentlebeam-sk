using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.Data.Sqlite;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

/// <summary>
/// Generic SQLite-backed repository for any Protobuf message type.
/// Data is persisted as JSON in a TEXT column alongside an integer primary key.
/// For child-entity tables a non-nullable <c>parent_id</c> column is added.
/// </summary>
public sealed class SqliteProtoRepository<T> where T : class, IMessage<T>, new()
{
    private static readonly JsonFormatter Formatter =
        JsonFormatter.Default;

    private static readonly JsonParser Parser =
        new(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly bool _hasParentId;

    public SqliteProtoRepository(string dbPath, string tableName, bool hasParentId = false)
    {
        _connectionString = $"Data Source={dbPath}";
        _tableName = tableName;
        _hasParentId = hasParentId;
        EnsureTable();
    }

    // ── table bootstrap ──────────────────────────────────────────────────────

    private void EnsureTable()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();

        if (_hasParentId)
        {
            cmd.CommandText =
                $"""
                CREATE TABLE IF NOT EXISTS {_tableName} (
                    id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    parent_id INTEGER NOT NULL DEFAULT 0,
                    data      TEXT    NOT NULL
                )
                """;
            cmd.ExecuteNonQuery();

            // Migration: add parent_id to tables that were created before this column existed.
            using var check = conn.CreateCommand();
            check.CommandText = $"PRAGMA table_info({_tableName})";
            bool hasColumn = false;
            using var r = check.ExecuteReader();
            while (r.Read())
            {
                if (r.GetString(1) == "parent_id") { hasColumn = true; break; }
            }
            if (!hasColumn)
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE {_tableName} ADD COLUMN parent_id INTEGER NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }
        }
        else
        {
            cmd.CommandText =
                $"""
                CREATE TABLE IF NOT EXISTS {_tableName} (
                    id   INTEGER PRIMARY KEY AUTOINCREMENT,
                    data TEXT    NOT NULL
                )
                """;
            cmd.ExecuteNonQuery();
        }
    }

    // ── CRUD ─────────────────────────────────────────────────────────────────

    public async Task<T> CreateAsync(T message, long parentId = 0)
    {
        await using var conn = await OpenAsync();

        using var insert = conn.CreateCommand();
        if (_hasParentId)
        {
            insert.CommandText =
                $"INSERT INTO {_tableName} (parent_id, data) VALUES (@p, @d); " +
                "SELECT last_insert_rowid();";
            insert.Parameters.AddWithValue("@p", parentId);
        }
        else
        {
            insert.CommandText =
                $"INSERT INTO {_tableName} (data) VALUES (@d); " +
                "SELECT last_insert_rowid();";
        }
        insert.Parameters.AddWithValue("@d", Formatter.Format(message));

        var newId = (long)(await insert.ExecuteScalarAsync())!;

        // Persist the canonical JSON that includes the correct id
        var updated = SetId(message.Clone(), newId);
        var json = Formatter.Format(updated);

        using var upd = conn.CreateCommand();
        upd.CommandText = $"UPDATE {_tableName} SET data = @d WHERE id = @id";
        upd.Parameters.AddWithValue("@d", json);
        upd.Parameters.AddWithValue("@id", newId);
        await upd.ExecuteNonQueryAsync();

        return updated;
    }

    public async Task<T?> ReadAsync(long id)
    {
        await using var conn = await OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        var json = (string?)await cmd.ExecuteScalarAsync();
        return json is null ? null : Parser.Parse<T>(json);
    }

    public async Task<IList<T>> ReadAllAsync()
    {
        await using var conn = await OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName}";

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(Parser.Parse<T>(reader.GetString(0)));
        return result;
    }

    public async Task<IList<T>> ReadByParentIdAsync(long parentId)
    {
        await using var conn = await OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} WHERE parent_id = @p";
        cmd.Parameters.AddWithValue("@p", parentId);

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(Parser.Parse<T>(reader.GetString(0)));
        return result;
    }

    public async Task<T> UpdateAsync(long id, T message)
    {
        await using var conn = await OpenAsync();
        var updated = SetId(message, id);
        var json = Formatter.Format(updated);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"UPDATE {_tableName} SET data = @d WHERE id = @id";
        cmd.Parameters.AddWithValue("@d", json);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
        return updated;
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = await OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM {_tableName} WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    // ── single-row helpers (Settings, System …) ───────────────────────────────

    public async Task<T?> ReadSingleAsync()
    {
        await using var conn = await OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} LIMIT 1";
        var json = (string?)await cmd.ExecuteScalarAsync();
        return json is null ? null : Parser.Parse<T>(json);
    }

    public async Task<T> UpsertSingleAsync(T message)
    {
        await using var conn = await OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            $"DELETE FROM {_tableName}; " +
            $"INSERT INTO {_tableName} (data) VALUES (@d); " +
            "SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@d", Formatter.Format(message));
        var rowId = (long)(await cmd.ExecuteScalarAsync())!;
        return SetId(message, rowId);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private async Task<SqliteConnection> OpenAsync()
    {
        var conn = new SqliteConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }

    /// <summary>
    /// Reflectively sets the <c>Id</c> property on proto messages that expose it.
    /// Returns the modified clone (or the original if there is no Id property).
    /// </summary>
    private static T SetId(T message, long id)
    {
        var prop = typeof(T).GetProperty("Id");
        if (prop is not null && prop.CanWrite)
            prop.SetValue(message, id);
        return message;
    }
}
