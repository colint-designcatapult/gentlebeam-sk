extern alias SqliteServer;

using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using PresetConfiguration = SqliteServer::Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration;
using ApprovePresetConfigurationRequest = SqliteServer::Com.Empyreanmed.Heracles.PresetConfigurations.V1.ApprovePresetConfigurationRequest;
using AuthService = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.AuthServiceImpl;
using User = SqliteServer::Com.Empyreanmed.Heracles.Users.V1.User;
using PresetService = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.PresetConfigurationServiceImpl;
using PresetRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.PresetConfigurations.V1.PresetConfiguration>;
using UserRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Users.V1.User>;
using AuditSessionRegistry = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.AuditSessionRegistry;
using LogRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log>;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class SqlitePresetPersistenceTests
{
    private string _directory = null!;
    private TestSqliteDatabase _database = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"preset-history-{Guid.NewGuid():N}");
        _database = new TestSqliteDatabase(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
        Directory.Delete(_directory, recursive: true);
    }
    [Test]
    public async Task ApprovePresetConfiguration_ValidCredentials_PersistsApproverEmail()
    {
        var repository = new PresetRepository(_database.Connections, "preset_configurations");
        var users = new UserRepository(_database.Connections, "users");
        var callerTimestamp = Timestamp.FromDateTime(DateTime.UnixEpoch);
        var preset = await repository.CreateAsync(new PresetConfiguration
        {
            CollimatorConfigurationId = 42,
            CreateDate = callerTimestamp,
            PresetName = "Reference",
            IsDefault = true,
            IsActive = true
        }, 42);
        Assert.That(preset.CreateDate, Is.Not.EqualTo(callerTimestamp));
        await users.CreateAsync(new User
        {
            Username = "physicist",
            Password = "correct-password",
            EmailAddress = "physicist@example.test"
        });
        var service = new PresetService(repository, CreateAuthService(users));

        var response = await service.ApprovePresetConfiguration(
            new ApprovePresetConfigurationRequest
            {
                PresetConfigurationId = preset.Id,
                Username = "PHYSICIST",
                Password = "correct-password"
            }, null!);
        var persisted = await repository.ReadAsync(preset.Id);
        var persistedUser = (await users.ReadAllAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(response.ApprovedPresetConfiguration.ApprovedBy, Is.EqualTo("physicist@example.test"));
            Assert.That(persisted!.ApprovedBy, Is.EqualTo("physicist@example.test"));
            Assert.That(persistedUser.LastAccessed, Is.Not.Null);
        });
    }

    [TestCase("physicist", "wrong-password")]
    [TestCase("missing-user", "correct-password")]
    public async Task ApprovePresetConfiguration_InvalidCredentials_RejectsWithoutPersisting(
        string username, string password)
    {
        var repository = new PresetRepository(_database.Connections, "preset_configurations");
        var users = new UserRepository(_database.Connections, "users");
        var preset = await repository.CreateAsync(new PresetConfiguration
        {
            CollimatorConfigurationId = 42,
            PresetName = "Reference",
            IsDefault = true,
            IsActive = true
        }, 42);
        await users.CreateAsync(new User
        {
            Username = "physicist",
            Password = "correct-password",
            EmailAddress = "physicist@example.test"
        });
        var service = new PresetService(repository, CreateAuthService(users));

        var exception = Assert.ThrowsAsync<RpcException>(() => service.ApprovePresetConfiguration(
            new ApprovePresetConfigurationRequest
            {
                PresetConfigurationId = preset.Id,
                Username = username,
                Password = password
            }, null!));
        var persisted = await repository.ReadAsync(preset.Id);

        Assert.Multiple(() =>
        {
            Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
            Assert.That(persisted!.ApprovedBy, Is.Empty);
        });
    }

    [Test]
    public async Task ApprovePresetConfiguration_MissingPreset_ReturnsNotFound()
    {
        var repository = new PresetRepository(_database.Connections, "preset_configurations");
        var users = new UserRepository(_database.Connections, "users");
        await users.CreateAsync(new User
        {
            Username = "physicist",
            Password = "correct-password",
            EmailAddress = "physicist@example.test"
        });
        var service = new PresetService(repository, CreateAuthService(users));

        var exception = Assert.ThrowsAsync<RpcException>(() => service.ApprovePresetConfiguration(
            new ApprovePresetConfigurationRequest
            {
                PresetConfigurationId = 404,
                Username = "physicist",
                Password = "correct-password"
            }, null!));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.NotFound));
    }

    private AuthService CreateAuthService(UserRepository users)
        => new(users, new AuditSessionRegistry(), new LogRepository(_database.Connections, "logs"));
}
