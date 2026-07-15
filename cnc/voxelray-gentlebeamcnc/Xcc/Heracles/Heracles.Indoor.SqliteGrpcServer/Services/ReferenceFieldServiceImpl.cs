using Com.Empyreanmed.Heracles.ReferenceFields.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class ReferenceFieldServiceImpl : ReferenceFieldService.ReferenceFieldServiceBase
{
    private readonly SqliteProtoRepository<ReferenceField> _repo;
    public ReferenceFieldServiceImpl(SqliteProtoRepository<ReferenceField> repo) => _repo = repo;

    public override async Task<ListReferenceFieldsResponse> ListReferenceFields(
        ListReferenceFieldsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListReferenceFieldsResponse();
        r.ReferenceFields.AddRange(items);
        return r;
    }

    public override async Task<GetReferenceFieldResponse> GetReferenceField(
        GetReferenceFieldRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"ReferenceField {request.Id} not found"));
        return new GetReferenceFieldResponse { ReferenceField = item };
    }

    public override async Task<CreateReferenceFieldResponse> CreateReferenceField(
        CreateReferenceFieldRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.ReferenceField, request.ReferenceField.PresetConfigurationId);
        return new CreateReferenceFieldResponse { ReferenceField = created };
    }

    public override async Task<UpdateReferenceFieldResponse> UpdateReferenceField(
        UpdateReferenceFieldRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.ReferenceField.Id, request.ReferenceField);
        return new UpdateReferenceFieldResponse { ReferenceField = updated };
    }

    public override async Task<DeleteReferenceFieldResponse> DeleteReferenceField(
        DeleteReferenceFieldRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteReferenceFieldResponse();
    }
}
