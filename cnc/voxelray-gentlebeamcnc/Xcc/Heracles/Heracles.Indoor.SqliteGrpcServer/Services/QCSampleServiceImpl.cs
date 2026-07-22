using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class QCSampleServiceImpl : QCSampleService.QCSampleServiceBase
{
    private readonly SqliteProtoRepository<QCSample> _repo;
    public QCSampleServiceImpl(SqliteProtoRepository<QCSample> repo) => _repo = repo;

    public override async Task<ListQCSamplesResponse> ListQCSamples(
        ListQCSamplesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
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
        var item = await _repo.ReadAsync(request.QcsampleId)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"QCSample {request.QcsampleId} not found"));
        var updated = await _repo.UpdateAsync(item.Id, item);
        return new ApproveQCSampleResponse { ApprovedQcsample = updated };
    }
}
