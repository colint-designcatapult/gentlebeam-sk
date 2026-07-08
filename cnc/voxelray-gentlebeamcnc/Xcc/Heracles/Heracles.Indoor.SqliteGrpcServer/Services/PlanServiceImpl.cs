using System.Threading.Channels;
using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.Plans.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

/// <summary>
/// SQLite-backed implementation of PlanService including server-streaming event endpoints.
/// </summary>
public sealed class PlanServiceImpl : PlanService.PlanServiceBase
{
    // ── broadcast channels ──────────────────────────────────────────────────
    private static readonly Channel<LoadForTreatmentEventsResponse> _lftChannel =
        System.Threading.Channels.Channel.CreateUnbounded<LoadForTreatmentEventsResponse>();

    private static readonly Channel<PlanEventsResponse> _planEventChannel =
        System.Threading.Channels.Channel.CreateUnbounded<PlanEventsResponse>();

    private readonly SqliteProtoRepository<Plan> _repo;

    public PlanServiceImpl(SqliteProtoRepository<Plan> repo) => _repo = repo;

    // ── CRUD ─────────────────────────────────────────────────────────────────
            
    public override async Task<ListPlansResponse> ListPlans(ListPlansRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.PrescriptionId);
        var r = new ListPlansResponse();
        r.Plans.AddRange(items);
        return r;
    }

    public override async Task<GetPlanResponse> GetPlan(GetPlanRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.PlanId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Plan {request.PlanId} not found"));
        return new GetPlanResponse { Plan = item };
    }

    public override async Task<CreatePlanResponse> CreatePlan(CreatePlanRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Plan, request.Plan.PrescriptionId);
        BroadcastPlanEvent(created);
        return new CreatePlanResponse { Plan = created };
    }

    public override async Task<UpdatePlanResponse> UpdatePlan(UpdatePlanRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Plan.Id, request.Plan);
        BroadcastPlanEvent(updated);
        return new UpdatePlanResponse { Plan = updated };
    }

    public override async Task<DeletePlanResponse> DeletePlan(DeletePlanRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.PlanId);
        return new DeletePlanResponse();
    }

    // ── treatment-load state transitions ─────────────────────────────────────

    public override async Task<LoadForTreatmentResponse> LoadForTreatment(
        LoadForTreatmentRequest request, ServerCallContext context)
    {
        var plan = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Plan {request.Id} not found"));

        plan.TreatmentLoadingState = request.IsPartial
            ? TREATMENTLOADINGSTATE.Partialpendingload
            : TREATMENTLOADINGSTATE.Pendingload;

        var updated = await _repo.UpdateAsync(plan.Id, plan);
        BroadcastPlanEvent(updated);

        _lftChannel.Writer.TryWrite(new LoadForTreatmentEventsResponse { Plan = updated });
        return new LoadForTreatmentResponse();
    }

    public override async Task<TreatmentLoadAckResponse> TreatmentLoadAck(
        TreatmentLoadAckRequest request, ServerCallContext context)
    {
        var plan = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Plan {request.Id} not found"));

        plan.TreatmentLoadingState = TREATMENTLOADINGSTATE.Loaded;
        var updated = await _repo.UpdateAsync(plan.Id, plan);
        BroadcastPlanEvent(updated);
        return new TreatmentLoadAckResponse();
    }

    public override async Task<UnloadFromTreatmentResponse> UnloadFromTreatment(
        UnloadFromTreatmentRequest request, ServerCallContext context)
    {
        var plan = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Plan {request.Id} not found"));

        plan.TreatmentLoadingState = TREATMENTLOADINGSTATE.Unloaded;
        var updated = await _repo.UpdateAsync(plan.Id, plan);
        BroadcastPlanEvent(updated);

        _lftChannel.Writer.TryWrite(new LoadForTreatmentEventsResponse { Plan = updated });
        return new UnloadFromTreatmentResponse();
    }

    public override async Task<FindPendingPlanResponse> FindPendingPlan(
        FindPendingPlanRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        var plan = all.FirstOrDefault(p =>
            p.TreatmentLoadingState is TREATMENTLOADINGSTATE.Pendingload
                or TREATMENTLOADINGSTATE.Partialpendingload);
        return new FindPendingPlanResponse { Plan = plan };
    }

    public override async Task<FindLoadedPlanResponse> FindLoadedPlan(
        FindLoadedPlanRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        var plan = all.FirstOrDefault(p => p.TreatmentLoadingState == TREATMENTLOADINGSTATE.Loaded);
        return new FindLoadedPlanResponse { Plan = plan };
    }

    public override async Task<UpdatePlanPrescriptionSimulationStatusResponse> UpdatePlanPrescriptionSimulationStatus(
        UpdatePlanPrescriptionSimulationStatusRequest request, ServerCallContext context)
    {
        var plan = await _repo.ReadAsync(request.PlanId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Plan {request.PlanId} not found"));

        plan.Status = request.Status;
        var updated = await _repo.UpdateAsync(plan.Id, plan);
        BroadcastPlanEvent(updated);

        return new UpdatePlanPrescriptionSimulationStatusResponse { UpdatedPlan = updated };
    }

    // ── server-streaming endpoints ────────────────────────────────────────────

    public override async Task LoadForTreatmentEvents(
        LoadForTreatmentEventsRequest request,
        IServerStreamWriter<LoadForTreatmentEventsResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var evt in _lftChannel.Reader.ReadAllAsync(context.CancellationToken))
        {
            await responseStream.WriteAsync(evt, context.CancellationToken);
        }
    }

    public override async Task PlanEvents(
        PlanEventsRequest request,
        IServerStreamWriter<PlanEventsResponse> responseStream,
        ServerCallContext context)
    {
        await foreach (var evt in _planEventChannel.Reader.ReadAllAsync(context.CancellationToken))
        {
            await responseStream.WriteAsync(evt, context.CancellationToken);
        }
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static void BroadcastPlanEvent(Plan plan)
        => _planEventChannel.Writer.TryWrite(new PlanEventsResponse { Plan = plan });
}
