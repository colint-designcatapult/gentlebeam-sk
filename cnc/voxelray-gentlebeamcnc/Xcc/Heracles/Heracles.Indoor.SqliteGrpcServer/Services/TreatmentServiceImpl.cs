using Com.Empyreanmed.Heracles.Treatments.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class TreatmentServiceImpl : TreatmentService.TreatmentServiceBase
{
    private readonly SqliteProtoRepository<Treatment> _repo;
    public TreatmentServiceImpl(SqliteProtoRepository<Treatment> repo) => _repo = repo;

    public override async Task<ListTreatmentsByPlanIdResponse> ListTreatmentsByPlanId(
        ListTreatmentsByPlanIdRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        var filtered = all.Where(t => t.PlanId == request.Id).ToList();
        var r = new ListTreatmentsByPlanIdResponse();
        r.Treatments.AddRange(filtered);
        return r;
    }

    public override async Task<ListTreatmentsByDiagnosisIdResponse> ListTreatmentsByDiagnosisId(
        ListTreatmentsByDiagnosisIdRequest request, ServerCallContext context)
    {
        // Treatments don't directly store diagnosis_id; return all and let caller filter.
        var all = await _repo.ReadAllAsync();
        var r = new ListTreatmentsByDiagnosisIdResponse();
        r.Treatments.AddRange(all);
        return r;
    }

    public override async Task<GetTreatmentResponse> GetTreatment(GetTreatmentRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.TreatmentId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Treatment {request.TreatmentId} not found"));
        return new GetTreatmentResponse { Treatment = item };
    }

    public override async Task<CreateTreatmentResponse> CreateTreatment(CreateTreatmentRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Treatment, request.Treatment.PlanId);
        return new CreateTreatmentResponse { Treatment = created };
    }

    public override async Task<UpdateTreatmentResponse> UpdateTreatment(UpdateTreatmentRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Treatment.Id, request.Treatment);
        return new UpdateTreatmentResponse { Treatment = updated };
    }

    public override async Task<DeleteTreatmentResponse> DeleteTreatment(DeleteTreatmentRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.TreatmentId);
        return new DeleteTreatmentResponse();
    }
}
