extern alias SqliteServer;

using System.Threading.Channels;
using Grpc.Core;
using Moq;
using Moq.Protected;
using SqliteInfra = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using Plans = SqliteServer::Com.Empyreanmed.Heracles.Plans.V1;
using ServerProto = SqliteServer::Com.Empyreanmed.Heracles;
using SqliteServices = SqliteServer::Heracles.Indoor.SqliteGrpcServer.Services;
using LoadingState = SqliteServer::Com.Empyreanmed.Heracles.Enums.V1.TREATMENTLOADINGSTATE;

namespace Heracles.Indoor.Test.Infra;

[TestFixture]
[NonParallelizable]
public sealed class SqlitePlanTreatmentEventsTests
{
    private string _directory = null!;
    private TestSqliteDatabase _database = null!;

    [SetUp]
    public void SetUp()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"plan-treatment-events-{Guid.NewGuid():N}");
        _database = new TestSqliteDatabase(_directory);
    }

    [TearDown]
    public void TearDown()
    {
        _database.Dispose();
        Directory.Delete(_directory, recursive: true);
    }

    [Test]
    public async Task Unload_PersistsAndBroadcastsUnloaded_WithoutRequestingAnotherLoad()
    {
        var repository = new SqliteInfra.SqliteProtoRepository<Plans.Plan>(
            _database.Connections, "plans", hasParentId: true, parentIdJsonField: "prescriptionId");
        var auth = new SqliteServices.AuthServiceImpl(
            new SqliteInfra.SqliteProtoRepository<ServerProto.Users.V1.User>(_database.Connections, "users"),
            new SqliteInfra.AuditSessionRegistry(),
            new SqliteInfra.SqliteProtoRepository<ServerProto.Logs.V1.Log>(_database.Connections, "logs"));
        var service = new SqliteServices.PlanServiceImpl(repository, auth);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var context = new Mock<ServerCallContext>();
        context.Protected().SetupGet<CancellationToken>("CancellationTokenCore").Returns(cancellation.Token);
        var created = await repository.CreateAsync(new Plans.Plan { PrescriptionId = 42 }, 42);
        await service.TreatmentLoadAck(new Plans.TreatmentLoadAckRequest { Id = created.Id }, context.Object);
        Assert.That((await repository.ReadAsync(created.Id))!.TreatmentLoadingState, Is.EqualTo(LoadingState.Loaded));

        var planEvents = new RecordingStream<Plans.PlanEventsResponse>();
        var loadEvents = new RecordingStream<Plans.LoadForTreatmentEventsResponse>();
        // Direct calls register both subscriptions before their first asynchronous wait.
        var planStream = service.PlanEvents(new Plans.PlanEventsRequest(), planEvents, context.Object);
        var loadStream = service.LoadForTreatmentEvents(new Plans.LoadForTreatmentEventsRequest(), loadEvents, context.Object);
        try
        {
            await service.UnloadFromTreatment(new Plans.UnloadFromTreatmentRequest { Id = created.Id }, context.Object);
            var unloaded = await repository.ReadAsync(created.Id);
            var update = await planEvents.ReadAsync(cancellation.Token);
            Assert.Multiple(() =>
            {
                Assert.That(unloaded!.TreatmentLoadingState, Is.EqualTo(LoadingState.Unloaded));
                Assert.That(update.Plan, Is.EqualTo(unloaded));
            });

            await service.LoadForTreatment(new Plans.LoadForTreatmentRequest { Id = created.Id }, context.Object);
            var pending = await repository.ReadAsync(created.Id);
            Assert.That(pending!.TreatmentLoadingState, Is.EqualTo(LoadingState.Pendingload));
            Assert.That((await planEvents.ReadAsync(cancellation.Token)).Plan, Is.EqualTo(pending));

            // The next genuine load is an ordered barrier: any erroneous unload request
            // must have arrived before it, so no timing-based absence check is needed.
            var requests = new List<Plans.Plan>();
            Plans.Plan requested;
            do
            {
                requested = (await loadEvents.ReadAsync(cancellation.Token)).Plan;
                requests.Add(requested);
            }
            while (requested.Id != created.Id || requested.TreatmentLoadingState != LoadingState.Pendingload);

            Assert.That(requests, Is.EqualTo(new[] { pending }));
        }
        finally
        {
            await cancellation.CancelAsync();
            try
            {
                await Task.WhenAll(planStream, loadStream).WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
        }
    }

    private sealed class RecordingStream<T> : IServerStreamWriter<T>
    {
        private readonly Channel<T> _events = Channel.CreateUnbounded<T>();

        public WriteOptions? WriteOptions { get; set; }

        public Task WriteAsync(T message) => _events.Writer.WriteAsync(message).AsTask();

        public Task WriteAsync(T message, CancellationToken cancellationToken) =>
            _events.Writer.WriteAsync(message, cancellationToken).AsTask();

        public ValueTask<T> ReadAsync(CancellationToken cancellationToken) => _events.Reader.ReadAsync(cancellationToken);
    }
}
