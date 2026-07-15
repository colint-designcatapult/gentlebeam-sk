using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class ActualTreatmentFieldServiceImpl
    : ActualTreatmentFieldService.ActualTreatmentFieldServiceBase
{
    private readonly SqliteProtoRepository<ActualTreatmentField> _repo;
    public ActualTreatmentFieldServiceImpl(SqliteProtoRepository<ActualTreatmentField> repo) => _repo = repo;

    public override async Task<ListActualTreatmentFieldsResponse> ListActualTreatmentFields(
        ListActualTreatmentFieldsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.TreatmentFieldId);
        var r = new ListActualTreatmentFieldsResponse();
        r.ActualTreatmentFields.AddRange(items);
        return r;
    }

    public override async Task<GetActualTreatmentFieldResponse> GetActualTreatmentField(
        GetActualTreatmentFieldRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.ActualTreatmentFieldId)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"ActualTreatmentField {request.ActualTreatmentFieldId} not found"));
        return new GetActualTreatmentFieldResponse { ActualTreatmentField = item };
    }

    public override async Task<CreateActualTreatmentFieldResponse> CreateActualTreatmentField(
        CreateActualTreatmentFieldRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(
            request.ActualTreatmentField, request.ActualTreatmentField.TreatmentId);
        return new CreateActualTreatmentFieldResponse { ActualTreatmentField = created };
    }

    public override async Task<UpdateActualTreatmentFieldResponse> UpdateActualTreatmentField(
        UpdateActualTreatmentFieldRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(
            request.ActualTreatmentField.Id, request.ActualTreatmentField);
        return new UpdateActualTreatmentFieldResponse { ActualTreatmentField = updated };
    }

    public override async Task<DeleteActualTreatmentFieldResponse> DeleteActualTreatmentField(
        DeleteActualTreatmentFieldRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.ActualTreatmentFieldId);
        return new DeleteActualTreatmentFieldResponse();
    }
}
