extern alias SqliteServer;

using System.Data;
using System.Globalization;
using Microsoft.Data.Sqlite;
using ConnectionFactory = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherConnectionFactory;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class SqliteConnectionPoolTests
{
    private static readonly TimeSpan Deadline = TimeSpan.FromSeconds(10);
    private string _directory = null!;
    private string _path = null!;
    private ConnectionFactory? _factory;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"heracles-connection-pool-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_directory);
        _path = Path.Combine(_directory, "pool.db");
    }

    [TearDown]
    public void TearDown()
    {
        _factory?.ClosePool();
        _factory = null;
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task Contention_WaitsForAnExclusiveConnectionWithoutOpeningAnother()
    {
        _factory = ConnectionFactory.CreatePlaintextForTests(_path, poolSize: 2);
        using var first = _factory.Rent();
        using var second = _factory.Rent();
        var returnedConnection = first.Connection;
        Assert.That(second.Connection, Is.Not.SameAs(returnedConnection));

        using var cancellation = new CancellationTokenSource(Deadline);
        var waiting = _factory.RentAsync(cancellation.Token).AsTask();
        try
        {
            Assert.That(waiting.IsCompleted, Is.False);
            first.Dispose();
            await using var next = await waiting;
            Assert.That(next.Connection, Is.SameAs(returnedConnection));
            Assert.That(next.Connection, Is.Not.SameAs(second.Connection));
            Assert.That(Scalar(next.Connection, "SELECT 1"), Is.EqualTo(1L));
        }
        finally
        {
            cancellation.Cancel();
            if (waiting.IsCompletedSuccessfully)
                waiting.Result.Dispose();
        }
    }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Cancellation_DoesNotConsumeTheOnlyConnection(bool synchronous)
    {
        _factory = ConnectionFactory.CreatePlaintextForTests(_path, poolSize: 1);
        using var held = _factory.Rent();
        var connection = held.Connection;
        using var cancellation = new CancellationTokenSource();
        var waiting = synchronous
            ? Task.Run(() => _factory.Rent(cancellation.Token))
            : _factory.RentAsync(cancellation.Token).AsTask();
        Assert.That(waiting.IsCompleted, Is.False);
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await waiting.WaitAsync(Deadline));
        held.Dispose();

        Assert.Throws<OperationCanceledException>(() => _factory.Rent(cancellation.Token));
        using var timeout = new CancellationTokenSource(Deadline);
        await using var next = await _factory.RentAsync(timeout.Token);
        Assert.That(next.Connection, Is.SameAs(connection));
        Assert.That(Scalar(next.Connection, "SELECT 1"), Is.EqualTo(1L));
    }

    [Test]
    public async Task CancellationRacingReturn_LeavesTheConnectionAvailable()
    {
        _factory = ConnectionFactory.CreatePlaintextForTests(_path, poolSize: 1);
        using var timeout = new CancellationTokenSource(Deadline);
        for (var attempt = 0; attempt < 32; attempt++)
        {
            using var held = await _factory.RentAsync(timeout.Token);
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
            var waiting = _factory.RentAsync(cancellation.Token).AsTask();
            await Task.WhenAll(Task.Run(held.Dispose), Task.Run(cancellation.Cancel));
            try
            {
                using var next = await waiting;
                Assert.That(Scalar(next.Connection, "SELECT 1"), Is.EqualTo(1L));
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
        }
        using var finalLease = await _factory.RentAsync(timeout.Token);
        Assert.That(Scalar(finalLease.Connection, "SELECT 1"), Is.EqualTo(1L));
    }

    [Test]
    public async Task RepeatedLeaseDisposal_CannotReturnAnAlreadyRentedConnection()
    {
        _factory = ConnectionFactory.CreatePlaintextForTests(_path, poolSize: 1);
        using var first = _factory.Rent();
        first.Dispose();
        using var second = _factory.Rent();
        await Task.WhenAll(Task.Run(first.Dispose), Task.Run(async () => await first.DisposeAsync()));
        using var cancellation = new CancellationTokenSource();
        var waiting = _factory.RentAsync(cancellation.Token).AsTask();
        try
        {
            Assert.That(waiting.IsCompleted, Is.False);
            cancellation.Cancel();
            Assert.CatchAsync<OperationCanceledException>(async () => await waiting.WaitAsync(Deadline));
            Assert.Throws<ObjectDisposedException>(() => _ = first.Connection);
        }
        finally
        {
            cancellation.Cancel();
            if (waiting.IsCompletedSuccessfully)
                waiting.Result.Dispose();
        }
    }

    [Test]
    public async Task ConcurrentShutdown_WakesWaitersAndDrainsActiveLeasesBeforeClosing()
    {
        _factory = ConnectionFactory.CreatePlaintextForTests(_path, poolSize: 1);
        using var held = _factory.Rent();
        var connection = held.Connection;
        var waiting = _factory.RentAsync().AsTask();
        var firstClose = Task.Run(_factory.ClosePool);
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await waiting.WaitAsync(Deadline));
        var secondClose = Task.Run(_factory.ClosePool);
        Assert.That(firstClose.IsCompleted, Is.False);
        Assert.That(secondClose.IsCompleted, Is.False);
        Assert.Throws<ObjectDisposedException>(() => _factory.Rent());
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await _factory.RentAsync());
        Assert.That(Scalar(connection, "SELECT 1"), Is.EqualTo(1L));

        held.Dispose();
        await Task.WhenAll(firstClose, secondClose).WaitAsync(Deadline);
        _factory.ClosePool();
        Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [Test]
    public void ReturnedLease_PreservesNativeStateAndFileOwnershipUntilShutdown()
    {
        _factory = ConnectionFactory.CreatePlaintextForTests(_path, poolSize: 1);
        SqliteConnection connection;
        using (var lease = _factory.Rent())
        {
            connection = lease.Connection;
            Execute(connection, "CREATE TABLE durable(value INTEGER); CREATE TEMP TABLE session(value INTEGER); INSERT INTO session VALUES(17)");
        }

        Assert.Throws<IOException>(() =>
        {
            using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        });
        using (var lease = _factory.Rent())
        {
            Assert.That(lease.Connection, Is.SameAs(connection));
            Assert.That(Scalar(lease.Connection, "SELECT value FROM session"), Is.EqualTo(17L));
        }

        _factory.ClosePool();
        using var released = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [TestCase(4000)]
    [TestCase(256000)]
    public void EncryptedPool_KeysEveryConnectionWithItsDatabaseIterations(int kdfIterations)
    {
        CreateEncryptedDatabase(kdfIterations);
        _factory = new ConnectionFactory(_path, "pool-secret", kdfIterations, poolSize: 2);
        using var first = _factory.Rent();
        using var second = _factory.Rent();
        Assert.That(Scalar(first.Connection, "SELECT value FROM records"), Is.EqualTo("encrypted record"));
        Assert.That(Scalar(second.Connection, "SELECT value FROM records"), Is.EqualTo("encrypted record"));
    }

    [Test]
    public void FailedConstruction_ReleasesTheInvalidKeyHandle()
    {
        CreateEncryptedDatabase(4000);
        Assert.Throws<IOException>(() => new ConnectionFactory(_path, "wrong-secret", 4000, poolSize: 2));
        using var exclusive = File.Open(_path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
    }

    [Test]
    public void MissingEncryptedDatabase_IsNotRecreated()
    {
        Assert.Throws<IOException>(() => new ConnectionFactory(_path, "pool-secret", 4000, poolSize: 1));
        Assert.That(File.Exists(_path), Is.False);
    }

    private void CreateEncryptedDatabase(int kdfIterations)
    {
        ConnectionFactory.EnsureProvider();
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = _path,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ToString());
        connection.Open();
        Execute(connection, "PRAGMA key='pool-secret'");
        Execute(connection, $"PRAGMA kdf_iter={kdfIterations.ToString(CultureInfo.InvariantCulture)}");
        Execute(connection, "CREATE TABLE records(value TEXT); INSERT INTO records VALUES('encrypted record')");
    }

    private static object? Scalar(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        return command.ExecuteScalar();
    }

    private static void Execute(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
