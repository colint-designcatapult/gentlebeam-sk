using System;
using Google.Api;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Reflection;
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

    private readonly SqlCipherConnectionFactory _connections;
    private readonly string _tableName;
    private readonly bool _hasParentId;
    private readonly string? _parentIdJsonField;

    public SqliteProtoRepository(
        SqlCipherConnectionFactory connections,
        string tableName,
        bool hasParentId = false,
        string? parentIdJsonField = null)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
        _tableName = tableName;
        _hasParentId = hasParentId;
        _parentIdJsonField = parentIdJsonField;
        EnsureTable();
    }

    // ── table bootstrap ──────────────────────────────────────────────────────

    private void EnsureTable()
    {
        using var lease = _connections.Rent();
        var conn = lease.Connection;
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
            bool hasColumn = false;
            using (var check = conn.CreateCommand())
            {
                check.CommandText = $"PRAGMA table_info({_tableName})";
                using var reader = check.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(1) == "parent_id")
                    {
                        hasColumn = true;
                        break;
                    }
                }
            }

            if (!hasColumn)
            {
                using var alter = conn.CreateCommand();
                alter.CommandText = $"ALTER TABLE {_tableName} ADD COLUMN parent_id INTEGER NOT NULL DEFAULT 0";
                alter.ExecuteNonQuery();
            }

            if (!string.IsNullOrEmpty(_parentIdJsonField))
            {
                using var backfill = conn.CreateCommand();
                backfill.CommandText =
                    $"""
                    UPDATE {_tableName}
                    SET parent_id = CAST(json_extract(data, @path) AS INTEGER)
                    WHERE parent_id = 0
                      AND json_extract(data, @path) IS NOT NULL
                    """;
                backfill.Parameters.AddWithValue("@path", $"$.{_parentIdJsonField}");
                backfill.ExecuteNonQuery();
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
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var transaction = conn.BeginTransaction(deferred: false);
        var created = await CreateAsync(message, transaction, parentId);
        transaction.Commit();
        return created;
    }

    internal async Task<T> CreateAsync(T message, SqliteTransaction transaction, long parentId = 0)
    {
        var prepared = PrepareForCreate(message);
        var conn = transaction.Connection!;

        using var insert = conn.CreateCommand();
        insert.Transaction = transaction;
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
        insert.Parameters.AddWithValue("@d", Formatter.Format(prepared));

        var newId = (long)(await insert.ExecuteScalarAsync())!;

        var updated = SetId(prepared, newId);
        var json = Formatter.Format(updated);

        using var upd = conn.CreateCommand();
        upd.Transaction = transaction;
        upd.CommandText = $"UPDATE {_tableName} SET data = @d WHERE id = @id";
        upd.Parameters.AddWithValue("@d", json);
        upd.Parameters.AddWithValue("@id", newId);
        await upd.ExecuteNonQueryAsync();

        return updated;
    }

    internal async Task<T?> CreateIfNoMatchAsync(
        T message,
        Func<T, bool> matches,
        long parentId = 0)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var transaction = conn.BeginTransaction(deferred: false);

        using (var read = conn.CreateCommand())
        {
            read.Transaction = transaction;
            read.CommandText = $"SELECT data FROM {_tableName}";
            await using var reader = await read.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var candidate = Parser.Parse<T>(reader.GetString(0));
                if (matches(candidate))
                {
                    transaction.Commit();
                    return null;
                }
            }
        }

        var created = await CreateAsync(message, transaction, parentId);
        transaction.Commit();
        return created;
    }

    public async Task<T?> ReadAsync(long id)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        return await ReadAsync(id, conn, null);
    }

    internal Task<T?> ReadAsync(long id, SqliteTransaction transaction) =>
        ReadAsync(id, transaction.Connection!, transaction);

    private async Task<T?> ReadAsync(long id, SqliteConnection conn, SqliteTransaction? transaction)
    {
        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"SELECT data FROM {_tableName} WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);

        var json = (string?)await cmd.ExecuteScalarAsync();
        return json is null ? null : Parser.Parse<T>(json);
    }

    internal async Task<T?> ReadFirstAsync(Func<T, bool> matches, SqliteTransaction transaction)
    {
        using var cmd = transaction.Connection!.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"SELECT id, data FROM {_tableName}";
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var candidate = Parser.Parse<T>(reader.GetString(1));
            if (matches(candidate))
                return SetId(candidate, reader.GetInt64(0));
        }
        return null;
    }

    public async Task<IList<T>> ReadPageAsync(int skip, int get)
    {
        skip = Math.Max(0, skip);
        get = Math.Max(1, get);

        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} ORDER BY id DESC LIMIT @get OFFSET @skip";
        cmd.Parameters.AddWithValue("@get", get);
        cmd.Parameters.AddWithValue("@skip", skip);

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(Parser.Parse<T>(reader.GetString(0)));
        return result;
    }

    public async Task<long> CountAsync()
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT COUNT(*) FROM {_tableName}";
        return (long)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task<IList<T>> ReadAllOrderedAsync()
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} ORDER BY id DESC";

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(Parser.Parse<T>(reader.GetString(0)));
        return result;
    }

    public async Task<IList<T>> ReadAllAsync()
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
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
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} WHERE parent_id = @p";
        cmd.Parameters.AddWithValue("@p", parentId);

        var result = new List<T>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            result.Add(Parser.Parse<T>(reader.GetString(0)));
        return result;
    }

    public async Task<T> UpdateAsync(long id, T message, bool preserveOutputOnly = true)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        // Reserve the write lock before reading output-only fields. A concurrent
        // authentication must not be overwritten by a stale normal user update.
        using var transaction = conn.BeginTransaction(deferred: false);
        var updated = await UpdateAsync(id, message, transaction, preserveOutputOnly);
        transaction.Commit();
        return updated;
    }

    internal async Task<T> UpdateAsync(
        long id, T message, SqliteTransaction transaction, bool preserveOutputOnly = true)
    {
        var conn = transaction.Connection!;
        var existing = preserveOutputOnly ? await ReadAsync(id, transaction) : null;
        var prepared = existing is null ? message.Clone() : PreserveOutputOnlyFields(existing, message);
        var updated = SetId(prepared, id);

        using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = $"UPDATE {_tableName} SET data = @d WHERE id = @id";
        cmd.Parameters.AddWithValue("@d", Formatter.Format(updated));
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
        return updated;
    }

    internal async Task<T?> UpdateIfNoMatchAsync(
        long id,
        T message,
        Func<T, bool> matches,
        bool preserveOutputOnly = true)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var transaction = conn.BeginTransaction(deferred: false);

        using (var read = conn.CreateCommand())
        {
            read.Transaction = transaction;
            read.CommandText = $"SELECT id, data FROM {_tableName}";
            await using var reader = await read.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var candidateId = reader.GetInt64(0);
                if (candidateId != id && matches(Parser.Parse<T>(reader.GetString(1))))
                {
                    transaction.Commit();
                    return null;
                }
            }
        }

        var updated = await UpdateAsync(id, message, transaction, preserveOutputOnly);
        transaction.Commit();
        return updated;
    }

    // Internal read/check/write boundary for authentication. SQLite's immediate
    // transaction serializes separate service instances and normal CRUD writers.
    internal async Task<T?> UpdateFirstAsync(Func<T, bool> matches, Func<T, bool> update)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var transaction = conn.BeginTransaction(deferred: false);
        T? selected = null;
        long id = 0;
        using (var read = conn.CreateCommand())
        {
            read.Transaction = transaction;
            read.CommandText = $"SELECT id, data FROM {_tableName}";
            await using var reader = await read.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var candidate = Parser.Parse<T>(reader.GetString(1));
                if (!matches(candidate))
                    continue;
                id = reader.GetInt64(0);
                selected = SetId(candidate, id);
                break;
            }
        }

        if (selected is not null && update(selected))
        {
            using var write = conn.CreateCommand();
            write.Transaction = transaction;
            write.CommandText = $"UPDATE {_tableName} SET data = @d WHERE id = @id";
            write.Parameters.AddWithValue("@d", Formatter.Format(selected));
            write.Parameters.AddWithValue("@id", id);
            await write.ExecuteNonQueryAsync();
        }
        transaction.Commit();
        return selected;
    }

    public async Task DeleteAsync(long id)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"DELETE FROM {_tableName} WHERE id = @id";
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    // ── single-row helpers (Settings, System …) ───────────────────────────────

    public async Task<T?> ReadSingleAsync()
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText = $"SELECT data FROM {_tableName} LIMIT 1";
        var json = (string?)await cmd.ExecuteScalarAsync();
        return json is null ? null : Parser.Parse<T>(json);
    }

    public async Task<T> UpsertSingleAsync(T message)
    {
        await using var lease = await _connections.RentAsync();
        var conn = lease.Connection;
        using var cmd = conn.CreateCommand();
        cmd.CommandText =
            $"DELETE FROM {_tableName}; " +
            $"INSERT INTO {_tableName} (data) VALUES (@d); " +
            "SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("@d", Formatter.Format(message));
        var rowId = (long)(await cmd.ExecuteScalarAsync())!;
        return SetId(message, rowId);
    }
    private static T PrepareForCreate(T message)
    {
        var prepared = message.Clone();
        foreach (var field in prepared.Descriptor.Fields.InDeclarationOrder().Where(IsOutputOnly))
        {
            if (field.Name is "create_date" or "creation_date" or "timestamp")
                field.Accessor.SetValue(prepared, Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow));
            else
                field.Accessor.Clear(prepared);
        }
        return prepared;
    }

    private static T PreserveOutputOnlyFields(T existing, T incoming)
    {
        var prepared = incoming.Clone();
        foreach (var field in existing.Descriptor.Fields.InDeclarationOrder().Where(IsOutputOnly))
        {
            field.Accessor.Clear(prepared);
            if (field.Accessor.HasValue(existing))
                field.Accessor.SetValue(prepared, field.Accessor.GetValue(existing));
        }
        return prepared;
    }
    private static bool IsOutputOnly(FieldDescriptor field)
    {
        var options = field.GetOptions();
        return options is not null &&
            options.GetExtension(FieldBehaviorExtensions.FieldBehavior).Contains(FieldBehavior.OutputOnly);
    }

    // ── helpers ───────────────────────────────────────────────────────────────

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
