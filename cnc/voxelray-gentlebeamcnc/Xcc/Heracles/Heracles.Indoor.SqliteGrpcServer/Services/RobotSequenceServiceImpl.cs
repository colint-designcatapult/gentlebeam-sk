using Com.Empyreanmed.Heracles.RobotSequences.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class RobotSequenceServiceImpl : RobotSequenceService.RobotSequenceServiceBase
{
    private readonly SqliteProtoRepository<RobotSequence> _repo;
    public RobotSequenceServiceImpl(SqliteProtoRepository<RobotSequence> repo) => _repo = repo;

    public override async Task<ListRobotSequencesResponse> ListRobotSequences(
        ListRobotSequencesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListRobotSequencesResponse();
        r.Sequences.AddRange(items);
        return r;
    }

    public override async Task<GetRobotSequenceResponse> GetRobotSequence(
        GetRobotSequenceRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.SequenceId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"RobotSequence {request.SequenceId} not found"));
        return new GetRobotSequenceResponse { Sequence = item };
    }

    public override async Task<CreateRobotSequenceResponse> CreateRobotSequence(
        CreateRobotSequenceRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Sequence);
        return new CreateRobotSequenceResponse { Sequence = created };
    }

    public override async Task<UpdateRobotSequenceResponse> UpdateRobotSequence(
        UpdateRobotSequenceRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Sequence.Id, request.Sequence);
        return new UpdateRobotSequenceResponse { Sequence = updated };
    }

    public override async Task<DeleteRobotSequenceResponse> DeleteRobotSequence(
        DeleteRobotSequenceRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.SequenceId);
        return new DeleteRobotSequenceResponse();
    }
}
