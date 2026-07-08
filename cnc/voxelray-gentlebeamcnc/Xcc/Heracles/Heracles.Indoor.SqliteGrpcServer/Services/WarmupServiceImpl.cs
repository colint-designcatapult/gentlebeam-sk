using Com.Empyreanmed.Heracles.Warmups.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class WarmupServiceImpl : WarmupService.WarmupServiceBase
{
    private readonly SqliteProtoRepository<Warmup> _repo;
    public WarmupServiceImpl(SqliteProtoRepository<Warmup> repo) => _repo = repo;

    public override async Task<ListWarmupsResponse> ListWarmups(
        ListWarmupsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListWarmupsResponse();
        r.Warmups.AddRange(items);
        return r;
    }

    public override async Task<GetWarmupResponse> GetWarmup(
        GetWarmupRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Warmup {request.Id} not found"));
        return new GetWarmupResponse { Warmup = item };
    }

    public override async Task<CreateWarmupResponse> CreateWarmup(
        CreateWarmupRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Warmup, request.Warmup.HeadId);
        return new CreateWarmupResponse { Warmup = created };
    }

    public override async Task<UpdateWarmupResponse> UpdateWarmup(
        UpdateWarmupRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Warmup.Id, request.Warmup);
        return new UpdateWarmupResponse { Warmup = updated };
    }

    public override async Task<DeleteWarmupResponse> DeleteWarmup(
        DeleteWarmupRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteWarmupResponse();
    }
}
