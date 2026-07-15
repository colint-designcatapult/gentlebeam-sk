using Com.Empyreanmed.Heracles.RobotStoredPositions.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class RobotStoredPositionServiceImpl : RobotStoredPositionService.RobotStoredPositionServiceBase
{
    private readonly SqliteProtoRepository<RobotStoredPosition> _repo;
    public RobotStoredPositionServiceImpl(SqliteProtoRepository<RobotStoredPosition> repo) => _repo = repo;

    public override async Task<ListRobotStoredPositionsResponse> ListRobotStoredPositions(
        ListRobotStoredPositionsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListRobotStoredPositionsResponse();
        r.RobotStoredPositions.AddRange(items);
        return r;
    }

    public override async Task<GetRobotStoredPositionResponse> GetRobotStoredPosition(
        GetRobotStoredPositionRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"RobotStoredPosition {request.Id} not found"));
        return new GetRobotStoredPositionResponse { RobotStoredPosition = item };
    }

    public override async Task<CreateRobotStoredPositionResponse> CreateRobotStoredPosition(
        CreateRobotStoredPositionRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.RobotStoredPosition);
        return new CreateRobotStoredPositionResponse { RobotStoredPosition = created };
    }

    public override async Task<UpdateRobotStoredPositionResponse> UpdateRobotStoredPosition(
        UpdateRobotStoredPositionRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.RobotStoredPosition.Id, request.RobotStoredPosition);
        return new UpdateRobotStoredPositionResponse { RobotStoredPosition = updated };
    }

    public override async Task<DeleteRobotStoredPositionResponse> DeleteRobotStoredPosition(
        DeleteRobotStoredPositionRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteRobotStoredPositionResponse();
    }
}
