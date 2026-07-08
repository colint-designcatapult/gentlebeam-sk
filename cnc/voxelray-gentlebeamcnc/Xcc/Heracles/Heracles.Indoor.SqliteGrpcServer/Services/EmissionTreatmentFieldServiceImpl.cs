using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class EmissionTreatmentFieldServiceImpl
    : EmissionTreatmentFieldService.EmissionTreatmentFieldServiceBase
{
    private readonly SqliteProtoRepository<EmissionTreatmentField> _repo;
    public EmissionTreatmentFieldServiceImpl(SqliteProtoRepository<EmissionTreatmentField> repo) => _repo = repo;

    public override async Task<ListEmissionTreatmentFieldsResponse> ListEmissionTreatmentFields(
        ListEmissionTreatmentFieldsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.ActualTreatmentFieldId);
        var r = new ListEmissionTreatmentFieldsResponse();
        r.EmissionTreatmentFields.AddRange(items);
        return r;
    }

    public override async Task<GetEmissionTreatmentFieldResponse> GetEmissionTreatmentField(
        GetEmissionTreatmentFieldRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.EmissionTreatmentFieldId)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"EmissionTreatmentField {request.EmissionTreatmentFieldId} not found"));
        return new GetEmissionTreatmentFieldResponse { EmissionTreatmentField = item };
    }

    public override async Task<CreateEmissionTreatmentFieldResponse> CreateEmissionTreatmentField(
        CreateEmissionTreatmentFieldRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(
            request.EmissionTreatmentField, request.EmissionTreatmentField.ActualTreatmentFieldId);
        return new CreateEmissionTreatmentFieldResponse { EmissionTreatmentField = created };
    }

    public override async Task<UpdateEmissionTreatmentFieldResponse> UpdateEmissionTreatmentField(
        UpdateEmissionTreatmentFieldRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(
            request.EmissionTreatmentField.Id, request.EmissionTreatmentField);
        return new UpdateEmissionTreatmentFieldResponse { EmissionTreatmentField = updated };
    }

    public override async Task<DeleteEmissionTreatmentFieldResponse> DeleteEmissionTreatmentField(
        DeleteEmissionTreatmentFieldRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.EmissionTreatmentFieldId);
        return new DeleteEmissionTreatmentFieldResponse();
    }
}
