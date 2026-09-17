using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading.Channels;
using Microsoft.Data.Sqlite;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

public sealed class SqlCipherConnectionFactory
{
    private static readonly Lazy<bool> Provider = new(InitializeProvider);
    private readonly object _gate = new();
    private readonly SqliteConnection[] _connections;
    private readonly Channel<SqliteConnection> _available;
    private int _activeLeases;
    private bool _closing;
    private bool _closed;

    public SqlCipherConnectionFactory(
        string databasePath, string password, int kdfIterations = 256000, int poolSize = 4)
        : this(databasePath, password, kdfIterations, poolSize, plaintext: false)
    {
    }

    // Friend-assembly tests can exercise real SQLite transactions without provisioning keys.
    // The public constructor remains encryption-required for every production caller.
    internal static SqlCipherConnectionFactory CreatePlaintextForTests(string databasePath, int poolSize = 4) =>
        new(databasePath, "", 256000, poolSize, plaintext: true);

    private SqlCipherConnectionFactory(
        string databasePath, string password, int kdfIterations, int poolSize, bool plaintext)
    {
        if (!plaintext && (string.IsNullOrWhiteSpace(password) || password.Contains('\0')))
            throw new ArgumentException("A nonempty database password without NUL is required.", nameof(password));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(kdfIterations);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(poolSize);
        EnsureProvider();
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.GetFullPath(databasePath),
            Mode = plaintext ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadWrite,
            Pooling = false
        }.ToString();
        _connections = new SqliteConnection[poolSize];
        _available = Channel.CreateBounded<SqliteConnection>(poolSize);
        try
        {
            for (var i = 0; i < _connections.Length; i++)
            {
                var connection = new SqliteConnection(connectionString);
                _connections[i] = connection;
                connection.Open();
                using var command = connection.CreateCommand();
                if (!plaintext)
                {
                    // Own the handle before key validation can fail inside SQLCipher.
                    command.CommandText = $"PRAGMA key='{password.Replace("'", "''")}'";
                    command.ExecuteNonQuery();
                    command.CommandText = $"PRAGMA kdf_iter={kdfIterations.ToString(CultureInfo.InvariantCulture)}";
                    command.ExecuteNonQuery();
                }
                command.CommandText = "SELECT count(*) FROM sqlite_master";
                command.ExecuteScalar();
                Configure(connection);
                _available.Writer.TryWrite(connection);
            }
        }
        catch
        {
            DisposeConnections();
            throw new IOException("The encrypted database cannot be opened. Check its availability and storage permissions.");
        }
    }

    public Lease Rent(CancellationToken cancellationToken = default)
    {
        var rental = RentAsync(cancellationToken);
        return rental.IsCompletedSuccessfully ? rental.Result : rental.AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask<Lease> RentAsync(CancellationToken cancellationToken = default)
    {
        while (true)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_closing, this);
                cancellationToken.ThrowIfCancellationRequested();
                if (_available.Reader.TryRead(out var connection))
                {
                    _activeLeases++;
                    return new Lease(this, connection);
                }
            }

            if (!await _available.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                throw new ObjectDisposedException(nameof(SqlCipherConnectionFactory));
        }
    }

    private void Return(SqliteConnection connection)
    {
        lock (_gate)
        {
            if (!_closing)
                _available.Writer.TryWrite(connection);
            _activeLeases--;
            if (_activeLeases == 0)
                Monitor.PulseAll(_gate);
        }
    }

    /// <summary>
    /// Exclusive access to one live connection. Dispose commands, readers and transactions
    /// before the lease; only the pool owns and disposes the underlying connection.
    /// </summary>
    public sealed class Lease : IDisposable, IAsyncDisposable
    {
        private SqlCipherConnectionFactory? _owner;
        private readonly SqliteConnection _connection;

        internal Lease(SqlCipherConnectionFactory owner, SqliteConnection connection)
        {
            _owner = owner;
            _connection = connection;
        }

        public SqliteConnection Connection
        {
            get
            {
                ObjectDisposedException.ThrowIf(Volatile.Read(ref _owner) is null, this);
                return _connection;
            }
        }

        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Return(_connection);

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
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
        lock (_gate)
        {
            if (_closing)
            {
                while (!_closed)
                    Monitor.Wait(_gate);
                return;
            }

            _closing = true;
            _available.Writer.TryComplete();
            while (_activeLeases != 0)
                Monitor.Wait(_gate);
        }

        try
        {
            var error = DisposeConnections();
            if (error is not null)
                ExceptionDispatchInfo.Capture(error).Throw();
        }
        finally
        {
            lock (_gate)
            {
                _closed = true;
                Monitor.PulseAll(_gate);
            }
        }
    }

    private Exception? DisposeConnections()
    {
        Exception? error = null;
        foreach (var connection in _connections)
        {
            try { connection?.Dispose(); }
            catch (Exception exception) { error ??= exception; }
        }
        return error;
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
