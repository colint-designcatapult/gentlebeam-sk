using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class DiagnosisServiceImpl : DiagnosisService.DiagnosisServiceBase
{
    private readonly SqliteProtoRepository<Diagnosis> _repo;
    public DiagnosisServiceImpl(SqliteProtoRepository<Diagnosis> repo) => _repo = repo;

    public override async Task<ListDiagnosesResponse> ListDiagnoses(ListDiagnosesRequest request, ServerCallContext context)
    {
        IList<Diagnosis> items = request.HasPatientId
            ? await _repo.ReadByParentIdAsync(request.PatientId)
            : await _repo.ReadAllAsync();

        var r = new ListDiagnosesResponse();
        r.Diagnoses.AddRange(items);
        return r;
    }

    public override async Task<GetDiagnosisResponse> GetDiagnosis(GetDiagnosisRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Diagnosis {request.Id} not found"));
        return new GetDiagnosisResponse { Diagnosis = item };
    }

    public override async Task<CreateDiagnosisResponse> CreateDiagnosis(CreateDiagnosisRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Diagnosis, request.Diagnosis.PatientId);
        return new CreateDiagnosisResponse { Diagnosis = created };
    }

    public override async Task<UpdateDiagnosisResponse> UpdateDiagnosis(UpdateDiagnosisRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Diagnosis.Id, request.Diagnosis);
        return new UpdateDiagnosisResponse { Diagnosis = updated };
    }

    public override async Task<DeleteDiagnosisResponse> DeleteDiagnosis(DeleteDiagnosisRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync((long)request.Id);
        return new DeleteDiagnosisResponse();
    }
}
