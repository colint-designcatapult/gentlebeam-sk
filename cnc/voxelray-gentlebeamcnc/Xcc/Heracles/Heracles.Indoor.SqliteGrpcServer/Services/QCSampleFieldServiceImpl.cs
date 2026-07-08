using Com.Empyreanmed.Heracles.QcsampleFields.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class QCSampleFieldServiceImpl : QCSampleFieldService.QCSampleFieldServiceBase
{
    private readonly SqliteProtoRepository<QCSampleField> _repo;
    public QCSampleFieldServiceImpl(SqliteProtoRepository<QCSampleField> repo) => _repo = repo;

    public override async Task<ListQCSampleFieldsResponse> ListQCSampleFields(
        ListQCSampleFieldsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListQCSampleFieldsResponse();
        r.Qcsamplefields.AddRange(items);
        return r;
    }

    public override async Task<GetQCSampleFieldResponse> GetQCSampleField(
        GetQCSampleFieldRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.QcsamplefieldId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"QCSampleField {request.QcsamplefieldId} not found"));
        return new GetQCSampleFieldResponse { Qcsamplefield = item };
    }

    public override async Task<CreateQCSampleFieldResponse> CreateQCSampleField(
        CreateQCSampleFieldRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Qcsamplefield, request.Qcsamplefield.QcsampleId);
        return new CreateQCSampleFieldResponse { Qcsamplefield = created };
    }

    public override async Task<UpdateQCSampleFieldResponse> UpdateQCSampleField(
        UpdateQCSampleFieldRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Qcsamplefield.Id, request.Qcsamplefield);
        return new UpdateQCSampleFieldResponse { Qcsamplefield = updated };
    }

    public override async Task<DeleteQCSampleFieldResponse> DeleteQCSampleField(
        DeleteQCSampleFieldRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.QcsamplefieldId);
        return new DeleteQCSampleFieldResponse();
    }
}
