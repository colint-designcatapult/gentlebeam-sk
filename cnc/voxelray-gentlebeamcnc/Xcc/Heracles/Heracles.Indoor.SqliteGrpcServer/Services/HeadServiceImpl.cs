using Com.Empyreanmed.Heracles.Head.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class HeadServiceImpl : HeadService.HeadServiceBase
{
    private readonly SqliteProtoRepository<Head> _repo;
    public HeadServiceImpl(SqliteProtoRepository<Head> repo) => _repo = repo;

    public override async Task<ListHeadsResponse> ListHeads(ListHeadsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListHeadsResponse();
        r.Heads.AddRange(items);
        return r;
    }

    public override async Task<GetHeadResponse> GetHead(GetHeadRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Head {request.Id} not found"));
        return new GetHeadResponse { Head = item };
    }

    public override async Task<CreateHeadResponse> CreateHead(CreateHeadRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Head);
        return new CreateHeadResponse { Head = created };
    }

    public override async Task<UpdateHeadResponse> UpdateHead(UpdateHeadRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Head.Id, request.Head);
        return new UpdateHeadResponse { Head = updated };
    }

    public override async Task<DeleteHeadResponse> DeleteHead(DeleteHeadRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteHeadResponse();
    }
}
