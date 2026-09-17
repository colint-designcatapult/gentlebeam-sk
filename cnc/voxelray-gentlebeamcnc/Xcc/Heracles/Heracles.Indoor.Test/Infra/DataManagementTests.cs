extern alias SqliteServer;

using Grpc.Core;
using Heracles.Indoor.Services;
using Moq;
using Xcc.Application.AppLayer.Model;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Infra.UserSessions.BearerToken;
using DatabaseMaintenanceGate = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.DatabaseMaintenanceGate;
using DatabaseMaintenanceInterceptor = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.DatabaseMaintenanceInterceptor;
using SqlCipherDatabase = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqlCipherDatabase;
using SqliteGrpcServerHost = SqliteServer::Heracles.Indoor.SqliteGrpcServer.SqliteGrpcServerHost;
using Plan = SqliteServer::Com.Empyreanmed.Heracles.Plans.V1.Plan;
using PlanRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Plans.V1.Plan>;
using LoadingState = SqliteServer::Com.Empyreanmed.Heracles.Enums.V1.TREATMENTLOADINGSTATE;
using LogRepository = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure.SqliteProtoRepository<SqliteServer::Com.Empyreanmed.Heracles.Logs.V1.Log>;
using System.Text.Json;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
public sealed class DataManagementTests
{
    [Test]
    public async Task Maintenance_DrainsWholeRpcAndRejectsNewCallsUntilReleased()
    {
        var gate = new DatabaseMaintenanceGate();
        var interceptor = new DatabaseMaintenanceInterceptor(gate);
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var completedWrite = false;
        var rpc = interceptor.UnaryServerHandler(new object(), null!, async (_, _) =>
        {
            await finish.Task;
            completedWrite = true;
            return new object();
        });
        var pause = gate.PauseAsync(CancellationToken.None);
        Assert.That(pause.IsCompleted, Is.False, "An admitted RPC must finish before maintenance.");
        var rejected = Assert.ThrowsAsync<RpcException>(() => interceptor.UnaryServerHandler(
            new object(), null!, (_, _) => Task.FromResult(new object())));
        Assert.That(rejected!.StatusCode, Is.EqualTo(StatusCode.Unavailable));
        finish.SetResult();
        await rpc;
        using (await pause)
            Assert.That(completedWrite, Is.True);
        using var admittedAgain = gate.EnterUnary();
    }

    [Test]
    public async Task CancelledDrain_ReopensAdmissionButStopNeverDoes()
    {
        var gate = new DatabaseMaintenanceGate();
        using var active = gate.EnterUnary();
        using var cancellation = new CancellationTokenSource();
        var pause = gate.PauseAsync(cancellation.Token);
        cancellation.Cancel();
        Assert.CatchAsync<OperationCanceledException>(async () => await pause);
        using (gate.EnterUnary()) { }
        active.Dispose();
        using (await gate.PauseAsync(CancellationToken.None))
            gate.Stop();
        Assert.That(Assert.Throws<RpcException>(() => gate.EnterUnary())!.StatusCode,
            Is.EqualTo(StatusCode.Unavailable));
    }

    [TestCase(UserRole.BuiltInNames.Physicist, false, false)]
    [TestCase(UserRole.BuiltInNames.Administrator, true, false)]
    [TestCase(UserRole.BuiltInNames.Administrator, false, true)]
    public async Task Management_RejectsNonAdministratorOrInactiveSession(string role, bool locked, bool expired)
    {
        await using var fixture = new ManagementFixture(authorizationOnly: true);
        fixture.Users.AuthorizedUser!.Role = new UserRole(role);
        fixture.Session = new BearerTokenUserSession("operator", locked ? "" : "token",
            expired ? DateTime.Now.AddMinutes(-1) : DateTime.Now.AddHours(1), null);
        Assert.Multiple(() =>
        {
            Assert.That(fixture.Service.CanManage, Is.False);
            Assert.Throws<UnauthorizedAccessException>(() => fixture.Service.RevealRecoveryKey());
            Assert.Throws<UnauthorizedAccessException>(() => fixture.Service.RevealDatabasePassword());
        });
        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.Service.ExportAsync(Path.Combine(fixture.DirectoryPath, "export.db"), "backup-password"));
        Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            fixture.Service.ImportAsync(Path.Combine(fixture.DirectoryPath, "missing.db"), "backup-password"));
        Assert.That(fixture.ExitRequested, Is.False);
    }

    [Test]
    public async Task RevealedDatabasePasswordOpensLiveDataAsSqlCipherPassphrase()
    {
        await using var fixture = new ManagementFixture();
        var plans = new PlanRepository(fixture.Database.Connections, "plans", hasParentId: true);
        var plan = await plans.CreateAsync(new Plan { TreatmentLoadingState = LoadingState.Unloaded });
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection(new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(fixture.DirectoryPath, "heracles.encrypted.db"),
            Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString());
        connection.Open();
        using var query = connection.CreateCommand();
        // Own the handle before key validation, and use the new local database's KDF.
        query.CommandText = $"PRAGMA key='{fixture.Service.RevealDatabasePassword()}'; PRAGMA kdf_iter=4000";
        query.ExecuteNonQuery();
        query.CommandText = "SELECT data FROM plans WHERE id=$id";
        query.Parameters.AddWithValue("$id", plan.Id);
        Assert.That(Plan.Parser.ParseJson((string)query.ExecuteScalar()!).Id, Is.EqualTo(plan.Id));
    }

    [TestCase(LoadingState.Pendingload)]
    [TestCase(LoadingState.Partialpendingload)]
    [TestCase(LoadingState.Loaded)]
    public async Task Import_RefusesActiveTreatmentWithoutReplacingDataOrStoppingHost(LoadingState state)
    {
        await using var fixture = new ManagementFixture();
        var exportPath = Path.Combine(fixture.DirectoryPath, "backup.db");
        await fixture.Service.ExportAsync(exportPath, "backup-password");
        var plans = new PlanRepository(fixture.Database.Connections, "plans", hasParentId: true);
        var plan = await plans.CreateAsync(new Plan { TreatmentLoadingState = state });
        Assert.That(await fixture.Host.HasActiveTreatmentAsync(), Is.True);
        Assert.CatchAsync<InvalidOperationException>(() => fixture.Service.ImportAsync(exportPath, "backup-password"));
        Assert.That((await plans.ReadAsync(plan.Id))!.TreatmentLoadingState, Is.EqualTo(state));
        Assert.That(fixture.ExitRequested, Is.False);
        var logs = await new LogRepository(fixture.Database.Connections, "logs").ReadAllAsync();
        Assert.That(logs, Has.Count.EqualTo(1));
        using var audit = JsonDocument.Parse(logs.Single().Message);
        Assert.That(audit.RootElement.GetProperty("action").GetString(), Is.EqualTo("Export database"));
        using var admission = await fixture.Host.PauseUnaryCallsAsync();
    }

    [Test]
    public async Task SuccessfulImport_RestoresSnapshotKeepsRecoveryKeyAndRequestsExit()
    {
        await using var fixture = new ManagementFixture();
        var plans = new PlanRepository(fixture.Database.Connections, "plans", hasParentId: true);
        var original = await plans.CreateAsync(new Plan { TreatmentLoadingState = LoadingState.Unloaded });
        var recoveryKey = fixture.Service.RevealRecoveryKey();
        var exportPath = Path.Combine(fixture.DirectoryPath, "backup.db");
        await fixture.Service.ExportAsync(exportPath, "backup-password");
        await plans.DeleteAsync(original.Id);
        await plans.CreateAsync(new Plan { TreatmentLoadingState = LoadingState.Unloaded });
        await fixture.Service.ImportAsync(exportPath, "backup-password");
        Assert.Multiple(() =>
        {
            Assert.That(fixture.ExitRequested, Is.True);
            Assert.That(fixture.ExitIsError, Is.False);
        });
        using var reopened = new SqlCipherDatabase(fixture.DirectoryPath);
        reopened.Initialize(_ => throw new AssertionException("Import must retain automatic unlock."),
            _ => throw new AssertionException("Import must retain the local protector."));
        Assert.That(reopened.GetRecoveryKey(), Is.EqualTo(recoveryKey));
        var restored = new PlanRepository(reopened.Connections, "plans", hasParentId: true);
        Assert.That((await restored.ReadAllAsync()).Select(plan => plan.Id), Is.EqualTo(new[] { original.Id }));
        Assert.ThrowsAsync<RpcException>(() => fixture.Host.PauseUnaryCallsAsync());
        var audits = await new LogRepository(reopened.Connections, "logs").ReadAllAsync();
        using var importAudit = JsonDocument.Parse(audits.Single().Message);
        Assert.That(importAudit.RootElement.GetProperty("action").GetString(), Is.EqualTo("Import database configuration and patient records"));
        Assert.That(importAudit.RootElement.GetProperty("userId").GetInt64(), Is.EqualTo(17));
        Assert.That(importAudit.RootElement.GetProperty("outcome").GetString(), Is.EqualTo("success"));
        Assert.That(audits.Single().Message, Does.Not.Contain("backup-password").And.Not.Contain(recoveryKey));
    }

    private sealed class ManagementFixture : IAsyncDisposable
    {
        private readonly TestSqliteDatabase? _plaintextDatabase;
        public string DirectoryPath { get; } = Path.Combine(Path.GetTempPath(), $"heracles-management-{Guid.NewGuid():N}");
        public SqlCipherDatabase Database { get; }
        public SqliteGrpcServerHost Host { get; }
        public AuthorizedUserStore Users { get; } = new()
        {
            AuthorizedUser = new User { Id = 17, Username = "operator", Role = new UserRole(UserRole.BuiltInNames.Administrator) }
        };
        public BearerTokenUserSession Session { get; set; } =
            new("operator", "token", DateTime.Now.AddHours(1), null);
        public DataManagementService Service { get; }
        public bool ExitRequested { get; private set; }
        public bool ExitIsError { get; private set; }

        public ManagementFixture(bool authorizationOnly = false)
        {
            Database = new SqlCipherDatabase(DirectoryPath);
            if (authorizationOnly)
            {
                // Authorization must reject these calls before touching the uninitialized
                // encryption lifecycle. The host only needs ordinary SQLite repositories.
                _plaintextDatabase = new TestSqliteDatabase(DirectoryPath);
                Host = new SqliteGrpcServerHost(_plaintextDatabase.Connections, port: 0);
            }
            else
            {
                Database.Initialize(_ => true, _ => null);
                Host = new SqliteGrpcServerHost(Database.Connections, port: 0);
            }
            var sessions = new Mock<IBearerTokenUserSessionManager>();
            sessions.SetupGet(manager => manager.UserSession).Returns(() => Session);
            Service = new DataManagementService(Users, sessions.Object, Database, Host,
                () => { }, () => Task.CompletedTask, action => action(), (_, isError) =>
                {
                    ExitRequested = true;
                    ExitIsError = isError;
                });
        }

        public async ValueTask DisposeAsync()
        {
            Service.Dispose();
            await Host.DisposeAsync();
            Database.Dispose();
            _plaintextDatabase?.Dispose();
            Directory.Delete(DirectoryPath, recursive: true);
        }
    }
}
