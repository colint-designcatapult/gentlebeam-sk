using Com.Empyreanmed.Heracles.Visits.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class VisitServiceImpl : VisitsService.VisitsServiceBase
{
    private readonly SqliteProtoRepository<Visit> _repo;
    public VisitServiceImpl(SqliteProtoRepository<Visit> repo) => _repo = repo;

    public override async Task<ListVisitsResponse> ListVisits(ListVisitsRequest request, ServerCallContext context)
    {
        IList<Visit> items = request.HasPatientId
            ? await _repo.ReadByParentIdAsync(request.PatientId)
            : await _repo.ReadAllAsync();
        var r = new ListVisitsResponse();
        r.Visits.AddRange(items);
        return r;
    }

    public override async Task<GetVisitResponse> GetVisit(GetVisitRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.VisitId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Visit {request.VisitId} not found"));
        return new GetVisitResponse { Visit = item };
    }

    public override async Task<CreateVisitResponse> CreateVisit(CreateVisitRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Visit, request.Visit.PatientId);
        return new CreateVisitResponse { Visit = created };
    }

    public override async Task<UpdateVisitResponse> UpdateVisit(UpdateVisitRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Visit.Id, request.Visit);
        return new UpdateVisitResponse { Visit = updated };
    }

    public override async Task<DeleteVisitResponse> DeleteVisit(DeleteVisitRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.VisitId);
        return new DeleteVisitResponse();
    }
}
