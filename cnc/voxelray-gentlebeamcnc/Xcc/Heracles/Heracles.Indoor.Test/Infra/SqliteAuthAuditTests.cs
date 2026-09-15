extern alias SqliteServer;

using System.Globalization;
using System.Text;
using System.Text.Json;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Moq;
using Moq.Protected;
using Infra = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using ServerProto = SqliteServer::Com.Empyreanmed.Heracles;
using UsersService = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.UsersServiceImpl;
using AuditSessionRegistry = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.AuditSessionRegistry;
using AuthService = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services.AuthServiceImpl;
using LoginRequest = SqliteServer::Com.Empyreanmed.Heracles.Auth.V1.LoginRequest;
using LogRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log>;
using SqlCipherDatabase = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherDatabase;
using User = SqliteServer::Com.Empyreanmed.Heracles.Users.V1.User;
using UserRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Users.V1.User>;
using Log = SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log;
using LogType = SqliteServer::Com.Empyreanmed.Heracles.Enums.V1.LOGTYPE;
using Severity = SqliteServer::Com.Empyreanmed.Heracles.Enums.V1.SEVERITY;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class SqliteAuthAuditTests
{
    private const string Password = "credential-must-not-appear-in-audit";
    private string _directory = null!;
    private SqlCipherDatabase _database = null!;
    private UserRepository _users = null!;
    private LogRepository _logs = null!;
    private AuditSessionRegistry _sessions = null!;
    private AuthService _auth = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"heracles-auth-audit-{Guid.NewGuid():N}");
        _database = new SqlCipherDatabase(_directory);
        _database.Initialize(_ => true, _ => null);
        _users = new UserRepository(_database.Connections, "users");
        _logs = new LogRepository(_database.Connections, "logs");
        _sessions = new AuditSessionRegistry();
        _auth = new AuthService(_users, _sessions, _logs);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task Login_UpdatesLastAccessedAndReturnsServerKnownTokenWithSecurityOutcome()
    {
        var user = await CreateUserAsync();
        var before = DateTime.UtcNow;

        var response = await _auth.Login(new LoginRequest { Username = "OPERATOR", Password = Password }, null!);

        AssertOutcome((await _logs.ReadAllAsync()).Single(), "Login", "success", user.Username, user.Id, Severity.Info, response.JwtToken);
        Assert.That((await _users.ReadAsync(user.Id))!.LastAccessed.ToDateTime(), Is.InRange(before, DateTime.UtcNow));
        Assert.That(_sessions.TryResolve(response.JwtToken, out var actor, out var userId), Is.True);
        Assert.That(actor, Is.EqualTo(user.Username));
        Assert.That(userId, Is.EqualTo(user.Id.ToString(CultureInfo.InvariantCulture)));

        var forged = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Username}:{Guid.NewGuid()}"));
        Assert.That(_sessions.TryResolve(forged, out _, out _), Is.False);
        Assert.That(new AuditSessionRegistry().TryResolve(response.JwtToken, out _, out _), Is.False);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Authentication_UnknownUsernameLogsAttemptWithoutCreatingUserOrDisclosingCredentials(bool login)
    {
        const string username = "missing\"\nactor=administrator";
        var exception = Assert.ThrowsAsync<RpcException>(async () =>
        {
            if (login)
                await _auth.Login(new LoginRequest { Username = username, Password = Password }, null!);
            else
                await _auth.AuthenticateAsync(username, Password);
        });

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That(exception.ToString(), Does.Not.Contain(Password));
        Assert.That(await _users.ReadAllAsync(), Is.Empty);
        AssertOutcome((await _logs.ReadAllAsync()).Single(), login ? "Login" : "Authenticate",
            "unknown_user", username, null, Severity.Warn);
    }

    [Test]
    public async Task Login_WrongPasswordCountsFailureWithoutChangingLastAccessed()
    {
        var user = await CreateUserAsync();
        const string wrongPassword = "incorrect-credential-must-not-appear";

        var exception = Assert.ThrowsAsync<RpcException>(async () =>
            await _auth.Login(new LoginRequest { Username = "OPERATOR", Password = wrongPassword }, null!));

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That(exception.ToString(), Does.Not.Contain(Password).And.Not.Contain(wrongPassword));
        Assert.That((await _users.ReadAsync(user.Id))!.LastAccessed, Is.EqualTo(user.LastAccessed));
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(1));
        AssertOutcome((await _logs.ReadAllAsync()).Single(), "Login", "invalid_password", user.Username, user.Id, Severity.Warn, wrongPassword);
    }

    [TestCase(Password, true)]
    [TestCase("incorrect-approval-credential", false)]
    public async Task AuthenticateAsync_ApprovalChecksCredentialsAndLogsWithoutChangingLoginCounter(string password, bool valid)
    {
        var user = await CreateUserAsync();
        await FailLoginsAsync(user.Username, 3);
        var previousLogs = (await _logs.ReadAllAsync()).Select(log => log.Id).ToHashSet();
        var before = DateTime.UtcNow;
        if (valid)
        {
            var authenticated = await _auth.AuthenticateAsync(user.Username, password);
            Assert.That(authenticated.Id, Is.EqualTo(user.Id));
            Assert.That((await _users.ReadAsync(user.Id))!.LastAccessed.ToDateTime(), Is.InRange(before, DateTime.UtcNow));
        }
        else
        {
            var exception = Assert.ThrowsAsync<RpcException>(async () =>
                await _auth.AuthenticateAsync(user.Username, password));
            Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
            Assert.That(exception.ToString(), Does.Not.Contain(password));
            Assert.That((await _users.ReadAsync(user.Id))!.LastAccessed, Is.EqualTo(user.LastAccessed));
        }

        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(3));
        AssertOutcome((await _logs.ReadAllAsync()).Single(log => !previousLogs.Contains(log.Id)),
            "Authenticate", valid ? "success" : "invalid_password", user.Username, user.Id,
            valid ? Severity.Info : Severity.Warn, password);
    }

    [Test]
    public async Task LoginAndApprovalAuthentication_PreserveExistingSessions()
    {
        var user = await CreateUserAsync();
        var request = new LoginRequest { Username = user.Username, Password = Password };
        var first = await _auth.Login(request, null!);
        var second = await _auth.Login(request, null!);
        await _auth.AuthenticateAsync(user.Username, Password);

        Assert.That(second.JwtToken, Is.Not.EqualTo(first.JwtToken));
        foreach (var token in new[] { first.JwtToken, second.JwtToken })
        {
            Assert.That(_sessions.TryResolve(token, out var actor, out var userId), Is.True);
            Assert.That(actor, Is.EqualTo(user.Username));
            Assert.That(userId, Is.EqualTo(user.Id.ToString(CultureInfo.InvariantCulture)));
        }
        var logs = await _logs.ReadAllAsync();
        Assert.That(logs, Has.Count.EqualTo(3));
        AssertOutcome(logs[0], "Login", "success", user.Username, user.Id, Severity.Info, first.JwtToken, second.JwtToken);
        AssertOutcome(logs[1], "Login", "success", user.Username, user.Id, Severity.Info, first.JwtToken, second.JwtToken);
        AssertOutcome(logs[2], "Authenticate", "success", user.Username, user.Id, Severity.Info, first.JwtToken, second.JwtToken);
    }

    [TestCase(true, Password)]
    [TestCase(false, Password)]
    [TestCase(true, "incorrect-credential-must-not-appear")]
    public async Task Authentication_UserUpdateFailureRollsBackAndLogsWithoutLeakingPersistenceDetails(bool login, string password)
    {
        var user = await CreateUserAsync();
        user.FailedLoginAttempts = 3;
        await _users.UpdateAsync(user.Id, user, preserveOutputOnly: false);
        Execute("CREATE TRIGGER reject_user_update BEFORE UPDATE ON users BEGIN SELECT RAISE(ABORT, 'credential-must-not-appear-in-audit'); END;");

        var exception = Assert.ThrowsAsync<RpcException>(async () =>
        {
            if (login)
                await _auth.Login(new LoginRequest { Username = user.Username, Password = password }, null!);
            else
                await _auth.AuthenticateAsync(user.Username, password);
        });

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Internal));
        Assert.That(exception.ToString(), Does.Not.Contain(Password));
        Assert.That((await _users.ReadAsync(user.Id))!.LastAccessed, Is.EqualTo(user.LastAccessed));
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(3));
        AssertOutcome((await _logs.ReadAllAsync()).Single(), login ? "Login" : "Authenticate",
            "persistence_error", user.Username, user.Id, Severity.Error, password);
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Authentication_UserReadFailureReturnsSanitizedErrorAndLogsOutcome(bool login)
    {
        Execute("DROP TABLE users;");

        var exception = Assert.ThrowsAsync<RpcException>(async () =>
        {
            if (login)
                await _auth.Login(new LoginRequest { Username = "operator", Password = Password }, null!);
            else
                await _auth.AuthenticateAsync("operator", Password);
        });

        Assert.That(exception!.StatusCode, Is.EqualTo(StatusCode.Internal));
        Assert.That(exception.ToString(), Does.Not.Contain(Password).And.Not.Contain("no such table"));
        AssertOutcome((await _logs.ReadAllAsync()).Single(), login ? "Login" : "Authenticate",
            "persistence_error", "operator", null, Severity.Error);
    }

    [Test]
    public async Task Login_NinthFailureStillAllowsSuccessfulLoginAndResetsPersistedCounter()
    {
        var user = await CreateUserAsync();
        await FailLoginsAsync(user.Username, 9);
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(9));

        var response = await _auth.Login(new LoginRequest { Username = user.Username, Password = Password }, null!);

        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.Zero);
        Assert.That(_sessions.TryResolve(response.JwtToken, out _, out _), Is.True);
        await FailLoginsAsync(user.Username, 1);
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(1));
    }

    [Test]
    public async Task Login_TenthFailureLocksAccountAndNeitherPasswordCanClearOrIncreaseCounter()
    {
        var user = await CreateUserAsync();
        await FailLoginsAsync(user.Username, 10);
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(10));
        var failureLogs = await _logs.ReadAllAsync();
        Assert.That(failureLogs, Has.Count.EqualTo(10));
        foreach (var log in failureLogs)
            AssertOutcome(log, "Login", "invalid_password", user.Username, user.Id, Severity.Warn, "wrong-password");

        foreach (var password in new[] { Password, "wrong-password" })
        {
            var error = Assert.ThrowsAsync<RpcException>(async () =>
                await _auth.Login(new LoginRequest { Username = "OPERATOR", Password = password }, null!));
            Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        }

        var persisted = (await _users.ReadAsync(user.Id))!;
        Assert.That(persisted.FailedLoginAttempts, Is.EqualTo(10));
        Assert.That(persisted.LastAccessed, Is.EqualTo(user.LastAccessed));
        var lockedLogs = (await _logs.ReadAllAsync()).Where(log => log.Id > failureLogs.Max(previous => previous.Id)).ToArray();
        Assert.That(lockedLogs, Has.Length.EqualTo(2));
        foreach (var log in lockedLogs)
            AssertOutcome(log, "Login", "locked", user.Username, user.Id, Severity.Warn, "wrong-password");
    }

    [Test]
    public async Task Authentication_CountersAboveThresholdRemainLocked()
    {
        var user = await CreateUserAsync();
        user.FailedLoginAttempts = 11;
        await _users.UpdateAsync(user.Id, user, preserveOutputOnly: false);

        var loginError = Assert.ThrowsAsync<RpcException>(async () =>
            await _auth.Login(new LoginRequest { Username = user.Username, Password = Password }, null!));
        var approvalError = Assert.ThrowsAsync<RpcException>(async () =>
            await _auth.AuthenticateAsync(user.Username, Password));

        Assert.That(loginError!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That(approvalError!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(11));
        var logs = await _logs.ReadAllAsync();
        Assert.That(logs, Has.Count.EqualTo(2));
        AssertOutcome(logs[0], "Login", "locked", user.Username, user.Id, Severity.Warn);
        AssertOutcome(logs[1], "Authenticate", "locked", user.Username, user.Id, Severity.Warn);
    }

    [Test]
    public async Task AuthenticateAsync_LockedAccountCannotApproveAndDoesNotResetCounter()
    {
        var user = await CreateUserAsync();
        await FailLoginsAsync(user.Username, 10);
        var previousLogs = (await _logs.ReadAllAsync()).Select(log => log.Id).ToHashSet();

        var error = Assert.ThrowsAsync<RpcException>(async () =>
            await _auth.AuthenticateAsync(user.Username, Password));

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        var persisted = (await _users.ReadAsync(user.Id))!;
        Assert.That(persisted.FailedLoginAttempts, Is.EqualTo(10));
        Assert.That(persisted.LastAccessed, Is.EqualTo(user.LastAccessed));
        AssertOutcome((await _logs.ReadAllAsync()).Single(log => !previousLogs.Contains(log.Id)),
            "Authenticate", "locked", user.Username, user.Id, Severity.Warn);
    }

    [Test]
    public async Task Login_LockoutSurvivesDatabaseAndServiceRestart()
    {
        var user = await CreateUserAsync();
        await FailLoginsAsync(user.Username, 10);
        _database.Dispose();
        _database = new SqlCipherDatabase(_directory);
        _database.Initialize(_ => true, _ => null);
        _users = new UserRepository(_database.Connections, "users");
        _logs = new LogRepository(_database.Connections, "logs");
        _sessions = new AuditSessionRegistry();
        _auth = new AuthService(_users, _sessions, _logs);

        var error = Assert.ThrowsAsync<RpcException>(async () =>
            await _auth.Login(new LoginRequest { Username = user.Username, Password = Password }, null!));

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(10));
        AssertOutcome((await _logs.ReadAllOrderedAsync())[0], "Login", "locked", user.Username, user.Id, Severity.Warn);
    }

    [Test]
    public async Task ResetUserLockout_PersistsResetAndAuditAcrossRestartButNotAdminSession()
    {
        var target = await CreateUserAsync();
        await FailLoginsAsync(target.Username, 10);
        var roles = new Infra::SqliteProtoRepository<ServerProto.Roles.V1.Role>(_database.Connections, "roles");
        var mappings = new Infra::SqliteProtoRepository<ServerProto.UserRoles.V1.UserRole>(_database.Connections, "user_roles");
        var role = await roles.CreateAsync(new ServerProto.Roles.V1.Role { RoleName = "Administrator" });
        var administrator = await _users.CreateAsync(new User
        {
            Username = "administrator", Password = "admin-password", EmailAddress = "admin@example.test"
        });
        await mappings.CreateAsync(new ServerProto.UserRoles.V1.UserRole { UserId = administrator.EmailAddress, RoleId = role.Id });
        var login = await _auth.Login(new LoginRequest { Username = administrator.Username, Password = administrator.Password }, null!);
        var context = new Mock<ServerCallContext>();
        context.Protected().SetupGet<Metadata>("RequestHeadersCore")
            .Returns(new Metadata { { "authorization", $"Bearer {login.JwtToken}" } });
        var service = new UsersService(_users, _database.Connections, _sessions, _logs, mappings, roles);

        var reset = await service.ResetUserLockout(new ServerProto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, context.Object);
        var expected = target.Clone();
        expected.FailedLoginAttempts = 0;
        Assert.That(reset.User, Is.EqualTo(expected));
        var committedLogs = await _logs.ReadAllAsync();

        _database.Dispose();
        _database = new SqlCipherDatabase(_directory);
        _database.Initialize(_ => true, _ => null);
        _users = new UserRepository(_database.Connections, "users");
        _logs = new LogRepository(_database.Connections, "logs");
        _sessions = new AuditSessionRegistry();
        _auth = new AuthService(_users, _sessions, _logs);
        roles = new Infra::SqliteProtoRepository<ServerProto.Roles.V1.Role>(_database.Connections, "roles");
        mappings = new Infra::SqliteProtoRepository<ServerProto.UserRoles.V1.UserRole>(_database.Connections, "user_roles");
        service = new UsersService(_users, _database.Connections, _sessions, _logs, mappings, roles);

        var staleSession = Assert.ThrowsAsync<RpcException>(async () => await service.ResetUserLockout(
            new ServerProto.Users.V1.ResetUserLockoutRequest { UserId = target.Id }, context.Object));
        Assert.That(staleSession!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That(await _users.ReadAsync(target.Id), Is.EqualTo(expected));
        Assert.That(await _logs.ReadAllAsync(), Is.EqualTo(committedLogs));
        var resetAudit = committedLogs.Where(log => log.Message.Contains("\"ResetUserLockout\"")).Single();
        using var outcome = JsonDocument.Parse(resetAudit.Message);
        Assert.That(outcome.RootElement.GetProperty("userId").GetString(),
            Is.EqualTo(administrator.Id.ToString(CultureInfo.InvariantCulture)));
        Assert.That(outcome.RootElement.GetProperty("targetUserId").GetString(),
            Is.EqualTo(target.Id.ToString(CultureInfo.InvariantCulture)));

        var restoredLogin = await _auth.Login(new LoginRequest { Username = target.Username, Password = Password }, null!);
        Assert.That(_sessions.TryResolve(restoredLogin.JwtToken, out _, out var authenticatedId), Is.True);
        Assert.That(authenticatedId, Is.EqualTo(target.Id.ToString(CultureInfo.InvariantCulture)));
        Assert.That((await _users.ReadAsync(target.Id))!.FailedLoginAttempts, Is.Zero);
    }

    [Test]
    public async Task Login_ConcurrentFailuresAcrossServiceInstancesAreAtomicAndCapped()
    {
        var user = await CreateUserAsync();
        async Task FailOnce(AuthService service)
        {
            try
            {
                await service.Login(new LoginRequest { Username = user.Username, Password = "wrong-password" }, null!);
            }
            catch (RpcException error)
            {
                Assert.That(error.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
                return;
            }
            Assert.Fail("An invalid password must be rejected.");
        }
        var services = Enumerable.Range(0, 9).Select(_ =>
            new AuthService(new UserRepository(_database.Connections, "users"), new AuditSessionRegistry(), _logs)).ToArray();

        await RunConcurrentlyAsync(services.Select<AuthService, Func<Task>>(service => () => FailOnce(service)).ToArray());
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(9));
        await RunConcurrentlyAsync(services.Take(4).Select<AuthService, Func<Task>>(service => () => FailOnce(service)).ToArray());

        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(10));
        var logs = await _logs.ReadAllAsync();
        Assert.That(logs, Has.Count.EqualTo(13));
        var outcomes = logs.Select(log => JsonDocument.Parse(log.Message)).ToArray();
        try
        {
            Assert.That(outcomes.Count(log => log.RootElement.GetProperty("outcome").GetString() == "invalid_password"), Is.EqualTo(10));
            Assert.That(outcomes.Count(log => log.RootElement.GetProperty("outcome").GetString() == "locked"), Is.EqualTo(3));
        }
        finally
        {
            foreach (var outcome in outcomes)
                outcome.Dispose();
        }
    }

    [Test]
    public async Task UserUpdates_RacingFailedLoginsCannotOverwriteOutputOnlyCounter()
    {
        var user = await CreateUserAsync();
        var stale = user.Clone();
        stale.FirstName = "Updated name";
        stale.FailedLoginAttempts = 0;
        var otherRepository = new UserRepository(_database.Connections, "users");
        var updates = Enumerable.Range(0, 9).Select<int, Func<Task>>(_ =>
            async () => { await otherRepository.UpdateAsync(stale.Id, stale); });
        var failures = Enumerable.Range(0, 9).Select<int, Func<Task>>(_ =>
            () => FailLoginsAsync(user.Username, 1));

        await RunConcurrentlyAsync(updates.Concat(failures).ToArray());

        var persisted = (await _users.ReadAsync(user.Id))!;
        Assert.That(persisted.FailedLoginAttempts, Is.EqualTo(9));
        Assert.That(persisted.FirstName, Is.EqualTo(stale.FirstName));
        Assert.That(persisted.Password, Is.EqualTo(Password));
    }

    private async Task FailLoginsAsync(string username, int count)
    {
        for (var attempt = 0; attempt < count; attempt++)
        {
            try
            {
                await _auth.Login(new LoginRequest { Username = username, Password = "wrong-password" }, null!);
            }
            catch (RpcException error)
            {
                Assert.That(error.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
                continue;
            }
            Assert.Fail("An invalid password must be rejected.");
        }
    }

    private static async Task RunConcurrentlyAsync(params Func<Task>[] actions)
    {
        using var barrier = new Barrier(actions.Length);
        await Task.WhenAll(actions.Select(action => Task.Factory.StartNew(async () =>
        {
            if (!barrier.SignalAndWait(TimeSpan.FromSeconds(30)))
                throw new TimeoutException("Concurrent authentication workers did not start.");
            await action();
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap()));
    }

    private static void AssertOutcome(Log log, string action, string outcome, string actor, long? userId,
        Severity severity, params string[] secrets)
    {
        Assert.That(log.Type, Is.EqualTo(LogType.Security));
        Assert.That(log.Severity, Is.EqualTo(severity));
        Assert.That(log.Timestamp, Is.Not.Null);
        Assert.That(log.Timestamp.ToDateTime(), Is.InRange(DateTime.UtcNow.AddMinutes(-5), DateTime.UtcNow));
        Assert.That(log.Message, Does.Not.Contain(Password).And.Not.Contain("\n").And.Not.Contain("\r"));
        foreach (var secret in secrets)
            Assert.That(log.Message, Does.Not.Contain(secret));
        using var json = JsonDocument.Parse(log.Message);
        Assert.That(json.RootElement.GetProperty("action").GetString(), Is.EqualTo(action));
        Assert.That(json.RootElement.GetProperty("outcome").GetString(), Is.EqualTo(outcome));
        Assert.That(json.RootElement.GetProperty("actor").GetString(), Is.EqualTo(actor));
        Assert.That(json.RootElement.GetProperty("userId").GetString(), Is.EqualTo(userId?.ToString(CultureInfo.InvariantCulture)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Authentication_AuditWriteFailureDeniesValidCredentialsWithoutLeakingStorageDetails(bool login)
    {
        var user = await CreateUserAsync();
        Execute("CREATE TRIGGER reject_audit BEFORE INSERT ON logs BEGIN SELECT RAISE(ABORT, 'credential-must-not-appear-in-audit'); END;");

        var error = Assert.ThrowsAsync<RpcException>(async () =>
        {
            if (login)
                await _auth.Login(new LoginRequest { Username = user.Username, Password = Password }, null!);
            else
                await _auth.AuthenticateAsync(user.Username, Password);
        });

        Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Internal));
        Assert.That(error.ToString(), Does.Not.Contain(Password).And.Not.Contain("reject_audit"));
        Assert.That(await _logs.ReadAllAsync(), Is.Empty);
    }

    [Test]
    public async Task Login_AuditWriteFailureCannotRollBackBruteForceCounter()
    {
        var user = await CreateUserAsync();
        Execute("CREATE TRIGGER reject_audit BEFORE INSERT ON logs BEGIN SELECT RAISE(ABORT, 'credential-must-not-appear-in-audit'); END;");
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var error = Assert.ThrowsAsync<RpcException>(async () =>
                await _auth.Login(new LoginRequest { Username = user.Username, Password = "wrong-password" }, null!));
            Assert.That(error!.StatusCode, Is.EqualTo(StatusCode.Internal));
        }
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(10));
        Assert.That(await _logs.ReadAllAsync(), Is.Empty);

        Execute("DROP TRIGGER reject_audit;");
        var locked = Assert.ThrowsAsync<RpcException>(async () =>
            await _auth.Login(new LoginRequest { Username = user.Username, Password = Password }, null!));

        Assert.That(locked!.StatusCode, Is.EqualTo(StatusCode.Unauthenticated));
        Assert.That((await _users.ReadAsync(user.Id))!.FailedLoginAttempts, Is.EqualTo(10));
        AssertOutcome((await _logs.ReadAllAsync()).Single(), "Login", "locked", user.Username, user.Id, Severity.Warn);
    }

    private async Task<User> CreateUserAsync()
    {
        var user = await _users.CreateAsync(new User { Username = "operator", Password = Password });
        user.LastAccessed = Timestamp.FromDateTime(DateTime.UnixEpoch);
        return await _users.UpdateAsync(user.Id, user, preserveOutputOnly: false);
    }

    private void Execute(string sql)
    {
        using var connection = _database.Connections.Open();
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
