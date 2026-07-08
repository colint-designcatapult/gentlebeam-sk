using Com.Empyreanmed.Heracles.SafetyChecks.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class SafetyCheckServiceImpl : SafetyCheckService.SafetyCheckServiceBase
{
    private readonly SqliteProtoRepository<SafetyCheck> _repo;
    public SafetyCheckServiceImpl(SqliteProtoRepository<SafetyCheck> repo) => _repo = repo;

    public override async Task<ListSafetyChecksResponse> ListSafetyChecks(
        ListSafetyChecksRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListSafetyChecksResponse();
        r.SafetyChecks.AddRange(items);
        return r;
    }

    public override async Task<GetSafetyCheckResponse> GetSafetyCheck(
        GetSafetyCheckRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"SafetyCheck {request.Id} not found"));
        return new GetSafetyCheckResponse { SafetyCheck = item };
    }

    public override async Task<CreateSafetyCheckResponse> CreateSafetyCheck(
        CreateSafetyCheckRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.SafetyCheck);
        return new CreateSafetyCheckResponse { SafetyCheck = created };
    }

    public override async Task<UpdateSafetyCheckResponse> UpdateSafetyCheck(
        UpdateSafetyCheckRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.SafetyCheck.Id, request.SafetyCheck);
        return new UpdateSafetyCheckResponse { SafetyCheck = updated };
    }

    public override async Task<DeleteSafetyCheckResponse> DeleteSafetyCheck(
        DeleteSafetyCheckRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteSafetyCheckResponse();
    }
}
