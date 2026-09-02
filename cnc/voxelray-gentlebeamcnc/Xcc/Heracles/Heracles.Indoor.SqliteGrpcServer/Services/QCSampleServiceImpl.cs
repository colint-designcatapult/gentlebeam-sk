using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Com.Empyreanmed.Heracles.Users.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class QCSampleServiceImpl : QCSampleService.QCSampleServiceBase
{
    private readonly SqliteProtoRepository<QCSample> _repo;
    private readonly SqliteProtoRepository<User> _users;

    public QCSampleServiceImpl(
        SqliteProtoRepository<QCSample> repo,
        SqliteProtoRepository<User> users)
    {
        _repo = repo;
        _users = users;
    }

    public override async Task<ListQCSamplesResponse> ListQCSamples(
        ListQCSamplesRequest request, ServerCallContext context)
    {
        IList<QCSample> items = request.HasCollimatorConfigurationId
            ? await _repo.ReadByParentIdAsync(request.CollimatorConfigurationId)
            : await _repo.ReadAllAsync();
        var r = new ListQCSamplesResponse();
        r.Qcsamples.AddRange(items);
        return r;
    }

    public override async Task<GetQCSampleResponse> GetQCSample(
        GetQCSampleRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.QcsampleId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"QCSample {request.QcsampleId} not found"));
        return new GetQCSampleResponse { Qcsample = item };
    }

    public override async Task<CreateQCSampleResponse> CreateQCSample(
        CreateQCSampleRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Qcsample, request.Qcsample.CollimatorConfigurationId);
        return new CreateQCSampleResponse { Qcsample = created };
    }

    public override async Task<UpdateQCSampleResponse> UpdateQCSample(
        UpdateQCSampleRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Qcsample.Id, request.Qcsample);
        return new UpdateQCSampleResponse { Qcsample = updated };
    }

    public override async Task<DeleteQCSampleResponse> DeleteQCSample(
        DeleteQCSampleRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.QcsampleId);
        return new DeleteQCSampleResponse();
    }

    public override async Task<ApproveQCSampleResponse> ApproveQCSample(
        ApproveQCSampleRequest request, ServerCallContext context)
    {
        var users = await _users.ReadAllAsync();
        var user = users.FirstOrDefault(value =>
            value.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));
        if (user is null || user.Password != request.Password)
        {
            throw new RpcException(new Status(
                StatusCode.Unauthenticated,
                "Invalid username or password"));
        }

        var item = await _repo.ReadAsync(request.QcsampleId)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"QCSample {request.QcsampleId} not found"));
        item.ApprovedBy = user.EmailAddress;
        var updated = await _repo.UpdateAsync(item.Id, item);
        return new ApproveQCSampleResponse { ApprovedQcsample = updated };
    }
}
