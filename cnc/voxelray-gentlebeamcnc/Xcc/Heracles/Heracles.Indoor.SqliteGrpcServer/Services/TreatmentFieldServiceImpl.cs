using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class TreatmentFieldServiceImpl : TreatmentFieldService.TreatmentFieldServiceBase
{
    private readonly SqliteProtoRepository<TreatmentField> _repo;
    public TreatmentFieldServiceImpl(SqliteProtoRepository<TreatmentField> repo) => _repo = repo;

    public override async Task<ListTreatmentFieldsResponse> ListTreatmentFields(
        ListTreatmentFieldsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.PlanId);
        var r = new ListTreatmentFieldsResponse();
        r.TreatmentFields.AddRange(items);
        return r;
    }

    public override async Task<GetTreatmentFieldResponse> GetTreatmentField(
        GetTreatmentFieldRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.TreatmentFieldId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"TreatmentField {request.TreatmentFieldId} not found"));
        return new GetTreatmentFieldResponse { TreatmentField = item };
    }

    public override async Task<CreateTreatmentFieldResponse> CreateTreatmentField(
        CreateTreatmentFieldRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.TreatmentField, request.TreatmentField.PlanId);
        return new CreateTreatmentFieldResponse { TreatmentField = created };
    }

    public override async Task<CreateBatchTreatmentFieldsResponse> CreateBatchTreatmentFields(
        CreateBatchTreatmentFieldsRequest request, ServerCallContext context)
    {
        var created = new List<TreatmentField>(request.TreatmentFields.Count);
        foreach (var tf in request.TreatmentFields)
        {
            created.Add(await _repo.CreateAsync(tf, tf.PlanId));
        }
        var r = new CreateBatchTreatmentFieldsResponse();
        r.TreatmentFields.AddRange(created);
        return r;
    }

    public override async Task<UpdateTreatmentFieldResponse> UpdateTreatmentField(
        UpdateTreatmentFieldRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.TreatmentField.Id, request.TreatmentField);
        return new UpdateTreatmentFieldResponse { TreatmentField = updated };
    }

    public override async Task<DeleteTreatmentFieldResponse> DeleteTreatmentField(
        DeleteTreatmentFieldRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.TreatmentFieldId);
        return new DeleteTreatmentFieldResponse();
    }
}
