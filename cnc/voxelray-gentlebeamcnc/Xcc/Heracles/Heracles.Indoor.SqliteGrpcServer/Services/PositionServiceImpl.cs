using Com.Empyreanmed.Heracles.Positions.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class PositionServiceImpl : PositionService.PositionServiceBase
{
    private readonly SqliteProtoRepository<Position> _repo;
    public PositionServiceImpl(SqliteProtoRepository<Position> repo) => _repo = repo;

    public override async Task<ListPositionsResponse> ListPositions(ListPositionsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.Id);
        var r = new ListPositionsResponse();
        r.Positions.AddRange(items);
        return r;
    }

    public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Position {request.Id} not found"));
        return new GetPositionResponse { Position = item };
    }

    public override async Task<CreatePositionResponse> CreatePosition(CreatePositionRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Position, request.Position.SimulationId);
        return new CreatePositionResponse { Position = created };
    }

    public override async Task<UpdatePositionResponse> UpdatePosition(UpdatePositionRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Position.Id, request.Position);
        return new UpdatePositionResponse { Position = updated };
    }

    public override async Task<DeletePositionResponse> DeletePosition(DeletePositionRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeletePositionResponse();
    }
}
