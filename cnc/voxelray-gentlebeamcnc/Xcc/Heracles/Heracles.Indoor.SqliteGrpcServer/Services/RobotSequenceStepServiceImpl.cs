using Com.Empyreanmed.Heracles.RobotSequenceSteps.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class RobotSequenceStepServiceImpl : RobotSequenceStepService.RobotSequenceStepServiceBase
{
    private readonly SqliteProtoRepository<RobotSequenceStep> _repo;
    public RobotSequenceStepServiceImpl(SqliteProtoRepository<RobotSequenceStep> repo) => _repo = repo;

    public override async Task<ListRobotSequenceStepsResponse> ListRobotSequenceSteps(
        ListRobotSequenceStepsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListRobotSequenceStepsResponse();
        r.RobotSequenceSteps.AddRange(items);
        return r;
    }

    public override async Task<GetRobotSequenceStepResponse> GetRobotSequenceStep(
        GetRobotSequenceStepRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"RobotSequenceStep {request.Id} not found"));
        return new GetRobotSequenceStepResponse { RobotSequenceStep = item };
    }

    public override async Task<CreateRobotSequenceStepResponse> CreateRobotSequenceStep(
        CreateRobotSequenceStepRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.RobotSequenceStep, request.RobotSequenceStep.RobotSequenceId);
        return new CreateRobotSequenceStepResponse { RobotSequenceStep = created };
    }

    public override async Task<UpdateRobotSequenceStepResponse> UpdateRobotSequenceStep(
        UpdateRobotSequenceStepRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.RobotSequenceStep.Id, request.RobotSequenceStep);
        return new UpdateRobotSequenceStepResponse { RobotSequenceStep = updated };
    }

    public override async Task<DeleteRobotSequenceStepResponse> DeleteRobotSequenceStep(
        DeleteRobotSequenceStepRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteRobotSequenceStepResponse();
    }
}
