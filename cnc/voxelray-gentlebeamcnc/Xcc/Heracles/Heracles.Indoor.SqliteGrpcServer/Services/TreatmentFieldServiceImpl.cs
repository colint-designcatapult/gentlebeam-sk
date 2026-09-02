using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;
using System.Threading;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class TreatmentFieldServiceImpl : TreatmentFieldService.TreatmentFieldServiceBase
{
    private readonly SqliteProtoRepository<TreatmentField> _repo;
    private readonly SemaphoreSlim _createGate = new(1, 1);

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
        var created = await CreateSingleAsync(request.TreatmentField, context.CancellationToken);
        return new CreateTreatmentFieldResponse { TreatmentField = created };
    }

    public override async Task<CreateBatchTreatmentFieldsResponse> CreateBatchTreatmentFields(
        CreateBatchTreatmentFieldsRequest request, ServerCallContext context)
    {
        if (request.TreatmentFields.Count != 1)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Treatment field batches must contain exactly one PlusC field."));
        }

        var created = await CreateSingleAsync(request.TreatmentFields[0], context.CancellationToken);
        var response = new CreateBatchTreatmentFieldsResponse();
        response.TreatmentFields.Add(created);
        return response;
    }

    public override async Task<UpdateTreatmentFieldResponse> UpdateTreatmentField(
        UpdateTreatmentFieldRequest request, ServerCallContext context)
    {
        var treatmentField = request.TreatmentField;
        if (treatmentField is null || treatmentField.FieldName != FIELDNAME.PlusC)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Treatment fields must be named PlusC."));
        }

        var existing = await _repo.ReadAsync(treatmentField.Id)
            ?? throw new RpcException(new Status(
                StatusCode.NotFound,
                $"TreatmentField {treatmentField.Id} not found"));

        if (existing.FieldName != FIELDNAME.PlusC || existing.PlanId != treatmentField.PlanId)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Treatment fields cannot change name or plan."));
        }

        var updated = await _repo.UpdateAsync(treatmentField.Id, treatmentField);
        return new UpdateTreatmentFieldResponse { TreatmentField = updated };
    }

    private async Task<TreatmentField> CreateSingleAsync(
        TreatmentField? treatmentField,
        CancellationToken cancellationToken)
    {
        if (treatmentField is null || treatmentField.FieldName != FIELDNAME.PlusC)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Treatment fields must be named PlusC."));
        }

        await _createGate.WaitAsync(cancellationToken);
        try
        {
            var existingFields = await _repo.ReadByParentIdAsync(treatmentField.PlanId);
            if (existingFields.Count != 0)
            {
                throw new RpcException(new Status(
                    StatusCode.AlreadyExists,
                    $"Plan {treatmentField.PlanId} already has a treatment field."));
            }

            return await _repo.CreateAsync(treatmentField, treatmentField.PlanId);
        }
        finally
        {
            _createGate.Release();
        }
    }

    public override async Task<DeleteTreatmentFieldResponse> DeleteTreatmentField(
        DeleteTreatmentFieldRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.TreatmentFieldId);
        return new DeleteTreatmentFieldResponse();
    }
}
