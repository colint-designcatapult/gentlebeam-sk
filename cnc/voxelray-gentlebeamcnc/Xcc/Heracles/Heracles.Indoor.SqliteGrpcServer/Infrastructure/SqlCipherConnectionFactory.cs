using Microsoft.Data.Sqlite;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

public sealed class SqlCipherConnectionFactory
{
    private static readonly Lazy<bool> Provider = new(InitializeProvider);
    private readonly string _connectionString;
    private int _closed;

    public SqlCipherConnectionFactory(string databasePath, string password)
    {
        EnsureProvider();
        if (string.IsNullOrWhiteSpace(password) || password.Contains('\0'))
            throw new ArgumentException("A nonempty database password without NUL is required.", nameof(password));
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(databasePath),
            Password = password,
            Mode = SqliteOpenMode.ReadWrite,
            Pooling = true
        }.ToString();
    }

    public SqliteConnection Open()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this);
        var connection = new SqliteConnection(_connectionString);
        try
        {
            connection.Open();
            Configure(connection);
            return connection;
        }
        catch
        {
            connection.Dispose();
            throw new IOException("The encrypted database cannot be opened. Check its availability and storage permissions.");
        }
    }

    public async Task<SqliteConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _closed) != 0, this);
        var connection = new SqliteConnection(_connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            Configure(connection);
            cancellationToken.ThrowIfCancellationRequested();
            return connection;
        }
        catch (OperationCanceledException)
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw;
        }
        catch
        {
            await connection.DisposeAsync().ConfigureAwait(false);
            throw new IOException("The encrypted database cannot be opened. Check its availability and storage permissions.");
        }
    }

    internal static void EnsureProvider() => _ = Provider.Value;

    internal static void Configure(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA temp_store=MEMORY";
        command.ExecuteNonQuery();
    }

    internal void ClosePool()
    {
        Interlocked.Exchange(ref _closed, 1);
        using var connection = new SqliteConnection(_connectionString);
        SqliteConnection.ClearPool(connection);
    }

    private static bool InitializeProvider()
    {
        try
        {
            SQLitePCL.Batteries_V2.Init();
            using var connection = new SqliteConnection("Data Source=:memory:;Pooling=False");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA cipher_version";
            var version = command.ExecuteScalar() as string;
            if (string.IsNullOrEmpty(version) || !version.StartsWith("4.", StringComparison.Ordinal))
                throw new InvalidOperationException();
            // Do not allow native diagnostics to write database details to stderr or files.
            command.CommandText = "PRAGMA cipher_log_level=NONE";
            command.ExecuteNonQuery();
            return true;
        }
        catch
        {
            throw new InvalidOperationException("SQLCipher 4 encryption is unavailable. Repair the application installation before opening the database.");
        }
    }
}
