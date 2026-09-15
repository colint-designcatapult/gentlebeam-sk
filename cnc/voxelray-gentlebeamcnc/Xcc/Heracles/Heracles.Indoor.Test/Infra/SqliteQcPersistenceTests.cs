extern alias SqliteServer;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using QCSample = SqliteServer::Com.Empyreanmed.Heracles.Qcsamples.V1.QCSample;
using ApproveQCSampleRequest = SqliteServer::Com.Empyreanmed.Heracles.Qcsamples.V1.ApproveQCSampleRequest;
using User = SqliteServer::Com.Empyreanmed.Heracles.Users.V1.User;
using ListQCSamplesRequest = SqliteServer::Com.Empyreanmed.Heracles.Qcsamples.V1.ListQCSamplesRequest;
using QCSampleServiceImpl = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.QCSampleServiceImpl;
using AuthService = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.AuthServiceImpl;
using SqliteProtoRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Qcsamples.V1.QCSample>;
using UserRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Users.V1.User>;
using AuditSessionRegistry = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.AuditSessionRegistry;
using LogRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log>;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class SqliteQcPersistenceTests
{
    private string _directory = null!;
    private TestSqliteDatabase _database = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"qc-history-{Guid.NewGuid():N}");
        _database = new TestSqliteDatabase(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task CreateAndReadByParent_PreservesTimestampAndFiltersConfiguration()
    {
        var repository = CreateParentedRepository();
        var timestamp = Timestamp.FromDateTime(new DateTime(2026, 8, 28, 18, 30, 0, DateTimeKind.Utc));

        var created = await repository.CreateAsync(CreateSample(42, timestamp), 42);
        await repository.CreateAsync(CreateSample(43, timestamp), 43);

        var service = new QCSampleServiceImpl(repository, CreateAuthService(CreateUserRepository()));
        var response = await service.ListQCSamples(
            new ListQCSamplesRequest { CollimatorConfigurationId = 42 },
            null!);
        var results = response.Qcsamples;

        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Id, Is.EqualTo(created.Id));
            Assert.That(results[0].CollimatorConfigurationId, Is.EqualTo(42));
            Assert.That(results[0].CreateDate, Is.EqualTo(timestamp));
        });
    }

    [Test]
    public async Task EnablingParentIndex_BackfillsExistingQcSampleRows()
    {
        var timestamp = Timestamp.FromDateTime(new DateTime(2026, 8, 28, 18, 30, 0, DateTimeKind.Utc));
        var legacyRepository = new SqliteProtoRepository(_database.Connections, "qcsamples");
        var created = await legacyRepository.CreateAsync(CreateSample(42, timestamp));

        var migratedRepository = CreateParentedRepository();
        var results = await migratedRepository.ReadByParentIdAsync(42);

        Assert.Multiple(() =>
        {
            Assert.That(results, Has.Count.EqualTo(1));
            Assert.That(results[0].Id, Is.EqualTo(created.Id));
            Assert.That(results[0].CreateDate, Is.EqualTo(timestamp));
        });
    }
    [Test]
    public async Task ApproveQCSample_ValidCredentials_PersistsApproverEmail()
    {
        var repository = CreateParentedRepository();
        var users = CreateUserRepository();
        var sample = await repository.CreateAsync(
            CreateSample(42, Timestamp.FromDateTime(DateTime.UtcNow)),
            42);
        await users.CreateAsync(new User
        {
            Username = "physicist",
            Password = "correct-password",
            EmailAddress = "physicist@example.test"
        });
        var service = new QCSampleServiceImpl(repository, CreateAuthService(users));

        var response = await service.ApproveQCSample(
            new ApproveQCSampleRequest
            {
                QcsampleId = sample.Id,
                Username = "PHYSICIST",
                Password = "correct-password"
            },
            null!);
        var persisted = await repository.ReadAsync(sample.Id);

        Assert.Multiple(() =>
        {
            Assert.That(response.ApprovedQcsample.ApprovedBy, Is.EqualTo("physicist@example.test"));
            Assert.That(persisted!.ApprovedBy, Is.EqualTo("physicist@example.test"));
        });
    }

    [TestCase("physicist", "wrong-password")]
    [TestCase("missing-user", "correct-password")]
    public async Task ApproveQCSample_InvalidCredentials_RejectsWithoutPersisting(
        string username,
        string password)
    {
        var repository = CreateParentedRepository();
        var users = CreateUserRepository();
        var sample = await repository.CreateAsync(
            CreateSample(42, Timestamp.FromDateTime(DateTime.UtcNow)),
            42);
        await users.CreateAsync(new User
        {
            Username = "physicist",
            Password = "correct-password",
            EmailAddress = "physicist@example.test"
        });
        var service = new QCSampleServiceImpl(repository, CreateAuthService(users));

        var exception = Assert.ThrowsAsync<RpcException>(() => service.ApproveQCSample(
            new ApproveQCSampleRequest
            {
                QcsampleId = sample.Id,
                Username = username,
                Password = password
            },
            null!));
        var persisted = await repository.ReadAsync(sample.Id);

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
            Assert.That(persisted!.ApprovedBy, Is.Empty);
        });
    }

    [Test]
    public async Task ApproveQCSample_MissingSample_ReturnsNotFound()
    {
        var repository = CreateParentedRepository();
        var users = CreateUserRepository();
        await users.CreateAsync(new User
        {
            Username = "physicist",
            Password = "correct-password",
            EmailAddress = "physicist@example.test"
        });
        var service = new QCSampleServiceImpl(repository, CreateAuthService(users));

        var exception = Assert.ThrowsAsync<RpcException>(() => service.ApproveQCSample(
            new ApproveQCSampleRequest
            {
                QcsampleId = 404,
                Username = "physicist",
                Password = "correct-password"
            },
            null!));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.NotFound));
    }

    private SqliteProtoRepository CreateParentedRepository()
    {
        return new SqliteProtoRepository(
            _database.Connections,
            "qcsamples",
            hasParentId: true,
            parentIdJsonField: "collimatorConfigurationId");
    }
    private UserRepository CreateUserRepository()
    {
        return new UserRepository(_database.Connections, "users");
    }

    private AuthService CreateAuthService(UserRepository users)
        => new(users, new AuditSessionRegistry(), new LogRepository(_database.Connections, "logs"));

    private static QCSample CreateSample(long configurationId, Timestamp timestamp)
    {
        return new QCSample
        {
            CollimatorConfigurationId = configurationId,
            CreateDate = timestamp,
            PerformedBy = "operator@example.com",
            EmissionCurrent = 1,
            HeaterCurrent = 2,
            Duration = 3,
            Referenced = false
        };
    }
}
