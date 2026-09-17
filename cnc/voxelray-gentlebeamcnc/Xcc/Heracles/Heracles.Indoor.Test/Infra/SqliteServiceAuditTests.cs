extern alias SqliteServer;

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Infra = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using Proto = Com.Empyreanmed.Heracles;
using ServerProto = SqliteServer::Com.Empyreanmed.Heracles;
using SqliteGrpcServerHost = SqliteServer::Heracles.Indoor.SqliteGrpcServer.SqliteGrpcServerHost;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
[NonParallelizable]
public sealed class SqliteServiceAuditTests
{
    private string _directory = null!;
    private TestSqliteDatabase _database = null!;
    private Infra::SqliteProtoRepository<ServerProto.Logs.V1.Log> _logs = null!;
    private SqliteGrpcServerHost _host = null!;
    private GrpcChannel _channel = null!;
    private Proto.Users.V1.UsersService.UsersServiceClient _users = null!;
    private Proto.Settings.V1.SettingsService.SettingsServiceClient _settings = null!;
    private Proto.Logs.V1.LogService.LogServiceClient _logClient = null!;

    [SetUp]
    public async Task SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"heracles-service-audit-{Guid.NewGuid():N}");
        _database = new TestSqliteDatabase(_directory);
        _logs = new(_database.Connections, "logs");
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        _host = new SqliteGrpcServerHost(_database.Connections, port);
        await _host.StartAsync();
        _channel = GrpcChannel.ForAddress($"http://127.0.0.1:{port}");
        _users = new(_channel);
        _settings = new(_channel);
        _logClient = new(_channel);
    }

    [TearDown]
    public async Task TearDown()
    {
        _channel?.Dispose();
        if (_host is not null)
            await _host.DisposeAsync();
        _database.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task ReadsIncludingLazyInitializationAndRejectedReads_DoNotCreateChangeAudit()
    {
        var users = await _users.ListUsersAsync(new Proto.Users.V1.ListUsersRequest(), Options());
        var initialized = await _settings.GetSettingsAsync(new Proto.Settings.V1.GetSettingsRequest(), Options());
        var repeated = await _settings.GetSettingsAsync(new Proto.Settings.V1.GetSettingsRequest(), Options());
        var error = Assert.ThrowsAsync<RpcException>(async () =>
            await _users.GetUserAsync(new Proto.Users.V1.GetUserRequest { UserId = long.MaxValue }, Options()));

        Assert.That(users.Users, Is.Empty);
        Assert.That(repeated.Settings.Id, Is.EqualTo(initialized.Settings.Id));
        var settingsRepository = new Infra::SqliteProtoRepository<ServerProto.Settings.V1.Settings>(
            _database.Connections, "settings");
        Assert.That((await settingsRepository.ReadAllAsync()).Single().Id, Is.EqualTo(initialized.Settings.Id));
        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.NotFound));
        Assert.That((await _logClient.ListLogsAsync(new Proto.Logs.V1.ListLogsRequest(), Options())).Logs, Is.Empty);
        Assert.That(await _logs.ReadAllAsync(), Is.Empty);
    }

    [Test]
    public async Task BackgroundMutations_WithAnAmbientLoginToken_DoNotCreateChangeAudit()
    {
        var usersRepository = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        await usersRepository.CreateAsync(new ServerProto.Users.V1.User { Username = "operator", Password = "password" });
        var auth = new Proto.Auth.V1.AuthService.AuthServiceClient(_channel);
        var login = await auth.LoginAsync(new Proto.Auth.V1.LoginRequest { Username = "operator", Password = "password" }, Options());
        var authenticationLog = (await _logs.ReadAllAsync()).Single();
        Assert.That(authenticationLog.Type, Is.EqualTo(ServerProto.Enums.V1.LOGTYPE.Security));
        var headers = new Metadata { { "authorization", $"Bearer {login.JwtToken}" } };
        var warmups = new Proto.Warmups.V1.WarmupService.WarmupServiceClient(_channel);

        // Device/background persistence shares the logged-in transport; that does not make it a user action.
        var created = await warmups.CreateWarmupAsync(new Proto.Warmups.V1.CreateWarmupRequest
        {
            Warmup = new() { HeadId = 7, HeaterCurrent = 1.5f }
        }, Options(headers));
        created.Warmup.HeaterCurrent = 2.5f;
        await warmups.UpdateWarmupAsync(new Proto.Warmups.V1.UpdateWarmupRequest { Warmup = created.Warmup }, Options(headers));
        var persisted = await warmups.GetWarmupAsync(new Proto.Warmups.V1.GetWarmupRequest { Id = created.Warmup.Id }, Options(headers));

        Assert.That(persisted.Warmup.HeaterCurrent, Is.EqualTo(2.5f));
        Assert.That(persisted.Warmup.HeadId, Is.EqualTo(7));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(new[] { authenticationLog }));
    }

    [Test]
    public async Task ExplicitUserLog_PersistsWithoutAutomaticRpcEntriesOrReadFeedback()
    {
        const string message = "User operator changed device serial to device-42";
        await _settings.UpdateSettingsAsync(new Proto.Settings.V1.UpdateSettingsRequest
        {
            Settings = new() { DeviceSerial = "device-42" }
        }, Options());
        var created = await _logClient.CreateLogAsync(new Proto.Logs.V1.CreateLogRequest
        {
            Log = new()
            {
                Message = message,
                Type = Proto.Enums.V1.LOGTYPE.User,
                Severity = Proto.Enums.V1.SEVERITY.Info,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            }
        }, Options());
        var settings = await _settings.GetSettingsAsync(new Proto.Settings.V1.GetSettingsRequest(), Options());
        var firstPage = await _logClient.ListLogsAsync(new Proto.Logs.V1.ListLogsRequest { Get = 1 }, Options());
        var repeatedPage = await _logClient.ListLogsAsync(new Proto.Logs.V1.ListLogsRequest { Get = 1 }, Options());
        var record = await _logClient.GetLogAsync(new Proto.Logs.V1.GetLogRequest { Id = created.Log.Id }, Options());

        Assert.That(settings.Settings.DeviceSerial, Is.EqualTo("device-42"));
        Assert.That(firstPage.Logs.Select(log => log.Message), Is.EqualTo(new[] { message }));
        Assert.That(repeatedPage, Is.EqualTo(firstPage));
        Assert.That(firstPage.NextPageToken, Is.Empty);
        Assert.That(record.Log, Is.EqualTo(created.Log));
        var persisted = (await _logs.ReadAllAsync()).Single();
        Assert.That(persisted.Id, Is.EqualTo(created.Log.Id));
        Assert.That(persisted.Message, Is.EqualTo(message));
        Assert.That(persisted.Type, Is.EqualTo(ServerProto.Enums.V1.LOGTYPE.User));
    }

    [Test]
    public async Task UserCrud_CannotSetOrClearFailedLoginCounterEvenWhenChangingPassword()
    {
        var created = await _users.CreateUserAsync(new Proto.Users.V1.CreateUserRequest
        {
            User = new() { Username = "operator", Password = "password", FailedLoginAttempts = 10 }
        }, Options());
        Assert.That(created.User.FailedLoginAttempts, Is.Zero);
        var original = (await _users.GetUserAsync(new Proto.Users.V1.GetUserRequest { UserId = created.User.Id }, Options())).User;
        Assert.That(original.FailedLoginAttempts, Is.Zero);
        var auth = new Proto.Auth.V1.AuthService.AuthServiceClient(_channel);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var failure = Assert.ThrowsAsync<RpcException>(async () =>
                await auth.LoginAsync(new Proto.Auth.V1.LoginRequest { Username = "operator", Password = "wrong-password" }, Options()));
            Assert.That(failure!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        }

        // A client loaded before the failures has a stale zero, including when
        // updating a password or explicitly asking to clear this output-only field.
        original.FailedLoginAttempts = 0;
        original.Password = "new-password";
        original.FirstName = "Updated operator";
        var updated = await _users.UpdateUserAsync(new Proto.Users.V1.UpdateUserRequest { User = original }, Options());
        Assert.That(updated.User.FailedLoginAttempts, Is.EqualTo(10));
        var persisted = (await _users.GetUserAsync(new Proto.Users.V1.GetUserRequest { UserId = original.Id }, Options())).User;
        Assert.That(persisted.FailedLoginAttempts, Is.EqualTo(10));
        Assert.That(persisted.FirstName, Is.EqualTo(original.FirstName));
        Assert.That(persisted.Password, Is.EqualTo(original.Password));
        var locked = Assert.ThrowsAsync<RpcException>(async () =>
            await auth.LoginAsync(new Proto.Auth.V1.LoginRequest { Username = original.Username, Password = original.Password }, Options()));
        Assert.That(locked!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
    }

    [TestCase("password", 3u, true, "success")]
    [TestCase("wrong-password", 3u, false, "invalid_password")]
    [TestCase("password", 10u, false, "locked")]
    public async Task PlanApproval_AuthenticatesBeforeChangingStatusWithoutChangingLoginCounter(
        string password, uint failures, bool approved, string outcome)
    {
        var users = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        var user = await users.CreateAsync(new ServerProto.Users.V1.User { Username = "operator", Password = "password" });
        user.FailedLoginAttempts = failures;
        await users.UpdateAsync(user.Id, user, preserveOutputOnly: false);
        var plans = new Proto.Plans.V1.PlanService.PlanServiceClient(_channel);
        var created = await plans.CreatePlanAsync(new Proto.Plans.V1.CreatePlanRequest
        {
            Plan = new() { PrescriptionId = 42, Name = "Plan to approve", Status = Proto.Enums.V1.STATUS.PendingApproval }
        }, Options());
        var request = new Proto.Plans.V1.UpdatePlanPrescriptionSimulationStatusRequest
        {
            PlanId = created.Plan.Id,
            Username = "OPERATOR",
            Password = password,
            Status = Proto.Enums.V1.STATUS.Approved
        };

        if (approved)
        {
            var response = await plans.UpdatePlanPrescriptionSimulationStatusAsync(request, Options());
            Assert.That(response.UpdatedPlan.Status, Is.EqualTo(Proto.Enums.V1.STATUS.Approved));
        }
        else
        {
            var error = Assert.ThrowsAsync<RpcException>(async () =>
                await plans.UpdatePlanPrescriptionSimulationStatusAsync(request, Options()));
            Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        }

        var persisted = await plans.GetPlanAsync(new Proto.Plans.V1.GetPlanRequest { PlanId = created.Plan.Id }, Options());
        Assert.That(persisted.Plan.Status, Is.EqualTo(approved ? Proto.Enums.V1.STATUS.Approved : Proto.Enums.V1.STATUS.PendingApproval));
        Assert.That((await users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(failures));
        var log = (await _logs.ReadAllAsync()).Single();
        Assert.That(log.Type, Is.EqualTo(ServerProto.Enums.V1.LOGTYPE.Security));
        Assert.That(log.Severity, Is.EqualTo(approved ? ServerProto.Enums.V1.SEVERITY.Info : ServerProto.Enums.V1.SEVERITY.Warn));
        Assert.That(log.Message, Does.Not.Contain(password));
        using var audit = JsonDocument.Parse(log.Message);
        Assert.That(audit.RootElement.GetProperty("action").GetString(), Is.EqualTo("Authenticate"));
        Assert.That(audit.RootElement.GetProperty("outcome").GetString(), Is.EqualTo(outcome));
        Assert.That(audit.RootElement.GetProperty("actor").GetString(), Is.EqualTo(user.Username));
    }

    [TestCase(10u)]
    [TestCase(11u)]
    public async Task ResetUserLockout_AdminResetsOnlyCounterAndAuditsCommittedChange(uint failures)
    {
        var (actor, _, _, headers) = await CreateResetActorAsync();
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        var target = await CreateResetTargetAsync(failures);
        var other = await store.CreateAsync(new ServerProto.Users.V1.User { Username = "unrelated", Password = "other-password" });
        var actorBefore = await store.ReadAsync(actor.Id);
        var beforeLogs = await _logs.ReadAllAsync();
        var expected = target.Clone();
        expected.FailedLoginAttempts = 0;

        var result = await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers));

        Assert.That(result.User.ToString(), Is.EqualTo(expected.ToString()));
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(expected));
        Assert.That(await store.ReadAsync(actor.Id), Is.EqualTo(actorBefore));
        Assert.That(await store.ReadAsync(other.Id), Is.EqualTo(other));
        var log = (await _logs.ReadAllAsync()).Single(entry => !beforeLogs.Any(previous => previous.Id == entry.Id));
        Assert.That(log.Type, Is.EqualTo(ServerProto.Enums.V1.LOGTYPE.Security));
        Assert.That(log.Severity, Is.EqualTo(ServerProto.Enums.V1.SEVERITY.Info));
        Assert.That(log.Timestamp, Is.Not.Null);
        Assert.That(log.Message, Does.Not.Contain(target.Password).And.Not.Contain(actor.Password)
            .And.Not.Contain(headers.GetValue("authorization")![7..]));
        using var audit = JsonDocument.Parse(log.Message);
        Assert.That(audit.RootElement.GetProperty("action").GetString(), Is.EqualTo("ResetUserLockout"));
        Assert.That(audit.RootElement.GetProperty("outcome").GetString(), Is.EqualTo("success"));
        Assert.That(audit.RootElement.GetProperty("actor").GetString(), Is.EqualTo(actor.Username));
        Assert.That(audit.RootElement.GetProperty("userId").GetString(), Is.EqualTo(actor.Id.ToString(CultureInfo.InvariantCulture)));
        Assert.That(audit.RootElement.GetProperty("targetUserId").GetString(), Is.EqualTo(target.Id.ToString(CultureInfo.InvariantCulture)));

        var committedLogs = await _logs.ReadAllAsync();
        await _users.ResetUserLockoutAsync(new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(committedLogs));
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(expected));

        var auth = new Proto.Auth.V1.AuthService.AuthServiceClient(_channel);
        var rejected = Assert.ThrowsAsync<RpcException>(async () => await auth.LoginAsync(
            new Proto.Auth.V1.LoginRequest { Username = target.Username, Password = "wrong-password" }, Options()));
        Assert.That(rejected!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That((await store.ReadAsync(target.Id))!.FailedLoginAttempts, Is.EqualTo(1));
        await auth.LoginAsync(new Proto.Auth.V1.LoginRequest { Username = target.Username, Password = target.Password }, Options());
        Assert.That((await store.ReadAsync(target.Id))!.FailedLoginAttempts, Is.Zero);
    }

    [TestCase(0u)]
    [TestCase(9u)]
    public async Task ResetUserLockout_BelowThresholdPreservesAccountAndDoesNotAuditMutation(uint failures)
    {
        var (_, _, _, headers) = await CreateResetActorAsync();
        var target = await CreateResetTargetAsync(failures);
        var before = await _logs.ReadAllAsync();
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");

        var result = await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers));

        Assert.That(result.User.ToString(), Is.EqualTo(target.ToString()));
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(target));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(before));
    }

    [TestCase(null)]
    [TestCase("Bearer forged-administrator-token")]
    [TestCase("Basic forged-administrator-token")]
    public async Task ResetUserLockout_RejectsMissingOrForgedSession(string? authorization)
    {
        var target = await CreateResetTargetAsync(10);
        var headers = new Metadata();
        if (authorization is not null)
            headers.Add("authorization", authorization);

        var error = Assert.ThrowsAsync<RpcException>(async () => await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers)));

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(target));
        Assert.That(await _logs.ReadAllAsync(), Is.Empty);
    }

    [TestCase("Service")]
    [TestCase("administrator")]
    public async Task ResetUserLockout_RequiresExactMappedAdministratorNotUserRoleText(string roleName)
    {
        var (_, _, _, headers) = await CreateResetActorAsync(roleName);
        var target = await CreateResetTargetAsync(10);
        var before = await _logs.ReadAllAsync();

        var error = Assert.ThrowsAsync<RpcException>(async () => await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers)));

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.PermissionDenied));
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(target));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(before));
    }

    [TestCase("demoted", StatusCode.PermissionDenied)]
    [TestCase("mapping-deleted", StatusCode.PermissionDenied)]
    [TestCase("role-renamed", StatusCode.PermissionDenied)]
    [TestCase("role-deleted", StatusCode.PermissionDenied)]
    [TestCase("account-deleted", StatusCode.Unauthenticated)]
    [TestCase("account-locked", StatusCode.PermissionDenied)]
    public async Task ResetUserLockout_RechecksPersistedActorAfterLogin(string change, StatusCode status)
    {
        var (actor, role, mapping, headers) = await CreateResetActorAsync();
        var target = await CreateResetTargetAsync(10);
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        var roles = new Infra::SqliteProtoRepository<ServerProto.Roles.V1.Role>(_database.Connections, "roles");
        var mappings = new Infra::SqliteProtoRepository<ServerProto.UserRoles.V1.UserRole>(_database.Connections, "user_roles");
        switch (change)
        {
            case "demoted":
                var serviceRole = await roles.CreateAsync(new ServerProto.Roles.V1.Role { RoleName = "Service" });
                mapping.RoleId = serviceRole.Id;
                await mappings.UpdateAsync(mapping.Id, mapping);
                break;
            case "mapping-deleted":
                await mappings.DeleteAsync(mapping.Id);
                break;
            case "role-renamed":
                role.RoleName = "Service";
                await roles.UpdateAsync(role.Id, role);
                break;
            case "role-deleted":
                await roles.DeleteAsync(role.Id);
                break;
            case "account-deleted":
                await store.DeleteAsync(actor.Id);
                break;
            case "account-locked":
                actor = (await store.ReadAsync(actor.Id))!;
                actor.FailedLoginAttempts = 10;
                await store.UpdateAsync(actor.Id, actor, preserveOutputOnly: false);
                break;
        }
        var before = await _logs.ReadAllAsync();

        var error = Assert.ThrowsAsync<RpcException>(async () => await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers)));

        Assert.That(error!.StatusCode, Is.EqualTo(status));
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(target));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(before));
    }

    [Test]
    public async Task ResetUserLockout_MissingTargetReturnsNotFoundWithoutMutationAudit()
    {
        var (_, _, _, headers) = await CreateResetActorAsync();
        var before = await _logs.ReadAllAsync();

        var error = Assert.ThrowsAsync<RpcException>(async () => await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = long.MaxValue }, Options(headers)));

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.NotFound));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(before));
    }

    [TestCase("INSERT ON logs")]
    [TestCase("UPDATE ON logs")]
    [TestCase("UPDATE ON users")]
    public async Task ResetUserLockout_PersistenceFailureRollsBackResetAndSuccessAudit(string operation)
    {
        var (_, _, _, headers) = await CreateResetActorAsync();
        var target = await CreateResetTargetAsync(10);
        var before = await _logs.ReadAllAsync();
        using (var lease = _database.Connections.Rent())
        using (var command = lease.Connection.CreateCommand())
        {
            command.CommandText = $"CREATE TRIGGER reject_reset BEFORE {operation} BEGIN SELECT RAISE(ABORT, 'secret-storage-detail'); END;";
            command.ExecuteNonQuery();
        }

        var error = Assert.ThrowsAsync<RpcException>(async () => await _users.ResetUserLockoutAsync(
            new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, Options(headers)));

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Internal));
        Assert.That(error.ToString(), Does.Not.Contain("secret-storage-detail").And.Not.Contain("reject_reset"));
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        Assert.That(await store.ReadAsync(target.Id), Is.EqualTo(target));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(before));
        using (var lease = _database.Connections.Rent())
        using (var command = lease.Connection.CreateCommand())
        {
            command.CommandText = "DROP TRIGGER reject_reset;";
            command.ExecuteNonQuery();
        }
        var auth = new Proto.Auth.V1.AuthService.AuthServiceClient(_channel);
        var locked = Assert.ThrowsAsync<RpcException>(async () => await auth.LoginAsync(
            new Proto.Auth.V1.LoginRequest { Username = target.Username, Password = target.Password }, Options()));
        Assert.That(locked!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
    }

    private async Task<(ServerProto.Users.V1.User Actor, ServerProto.Roles.V1.Role Role,
        ServerProto.UserRoles.V1.UserRole Mapping, Metadata Headers)> CreateResetActorAsync(string roleName = "Administrator")
    {
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        var roles = new Infra::SqliteProtoRepository<ServerProto.Roles.V1.Role>(_database.Connections, "roles");
        var mappings = new Infra::SqliteProtoRepository<ServerProto.UserRoles.V1.UserRole>(_database.Connections, "user_roles");
        var role = await roles.CreateAsync(new ServerProto.Roles.V1.Role { RoleName = roleName });
        // This legacy text must not override the authoritative role mapping.
        var actor = await store.CreateAsync(new ServerProto.Users.V1.User
        {
            Username = "administrator", Password = "administrator-secret", EmailAddress = "admin@example.test", Role = "Administrator"
        });
        var mapping = await mappings.CreateAsync(new ServerProto.UserRoles.V1.UserRole { UserId = actor.EmailAddress, RoleId = role.Id });
        var auth = new Proto.Auth.V1.AuthService.AuthServiceClient(_channel);
        var login = await auth.LoginAsync(new Proto.Auth.V1.LoginRequest { Username = actor.Username, Password = actor.Password }, Options());
        return (actor, role, mapping, new Metadata { { "authorization", $"Bearer {login.JwtToken}" } });
    }

    private async Task<ServerProto.Users.V1.User> CreateResetTargetAsync(uint failures)
    {
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        var target = await store.CreateAsync(new ServerProto.Users.V1.User
        {
            Username = "locked-target", Password = "target-secret", EmailAddress = "target@example.test",
            FirstName = "First", MiddleName = "Middle", LastName = "Last", Picture = "picture", Role = "Service"
        });
        target.LastAccessed = Timestamp.FromDateTime(DateTime.UnixEpoch);
        target.FailedLoginAttempts = failures;
        return await store.UpdateAsync(target.Id, target, preserveOutputOnly: false);
    }

    [Test]
    public async Task ResetUserLockout_ConcurrentRequestsCommitOneResetAndOneSuccessAudit()
    {
        var (_, _, _, headers) = await CreateResetActorAsync();
        var target = await CreateResetTargetAsync(10);
        var before = await _logs.ReadAllAsync();
        var request = new Proto.Users.V1.ResetUserLockoutRequest { UserId = target.Id };

        var results = await Task.WhenAll(
            _users.ResetUserLockoutAsync(request, Options(headers)).ResponseAsync,
            _users.ResetUserLockoutAsync(request, Options(headers)).ResponseAsync);

        Assert.That(results.All(result => result.User.FailedLoginAttempts == 0), Is.True);
        var store = new Infra::SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users");
        Assert.That((await store.ReadAsync(target.Id))!.FailedLoginAttempts, Is.Zero);
        var committed = (await _logs.ReadAllAsync()).Single(log => !before.Any(previous => previous.Id == log.Id));
        using var audit = JsonDocument.Parse(committed.Message);
        Assert.That(audit.RootElement.GetProperty("action").GetString(), Is.EqualTo("ResetUserLockout"));
        Assert.That(audit.RootElement.GetProperty("targetUserId").GetString(), Is.EqualTo(target.Id.ToString(CultureInfo.InvariantCulture)));
    }

    private static CallOptions Options(Metadata? headers = null) =>
        new(headers: headers, deadline: DateTime.UtcNow.AddSeconds(15));
}
