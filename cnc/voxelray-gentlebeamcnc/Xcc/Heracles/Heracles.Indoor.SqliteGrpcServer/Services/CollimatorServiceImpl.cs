using Com.Empyreanmed.Heracles.Collimators.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class CollimatorServiceImpl : CollimatorService.CollimatorServiceBase
{
    private readonly SqliteProtoRepository<Collimator> _repo;
    public CollimatorServiceImpl(SqliteProtoRepository<Collimator> repo) => _repo = repo;

    public override async Task<ListCollimatorsResponse> ListCollimators(
        ListCollimatorsRequest request, ServerCallContext context)
    {
        IList<Collimator> items = request.HasCollimatorConfigurationId
            ? await _repo.ReadByParentIdAsync(request.CollimatorConfigurationId)
            : await _repo.ReadAllAsync();
        var r = new ListCollimatorsResponse();
        r.Collimators.AddRange(items);
        return r;
    }

    public override async Task<GetCollimatorResponse> GetCollimator(
        GetCollimatorRequest request, ServerCallContext context)
    {
        // serial is a string; try to parse as id for embedded server
        if (!long.TryParse(request.Serial, out var id))
        {
            var all = await _repo.ReadAllAsync();
            var bySerial = all.FirstOrDefault(c => c.Serial == request.Serial);
            if (bySerial is null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Collimator serial={request.Serial} not found"));
            return new GetCollimatorResponse { Collimator = bySerial };
        }
        var item = await _repo.ReadAsync(id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Collimator {id} not found"));
        return new GetCollimatorResponse { Collimator = item };
    }

    public override async Task<CreateCollimatorResponse> CreateCollimator(
        CreateCollimatorRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Collimator, request.Collimator.CollimatorConfigurationId);
        return new CreateCollimatorResponse { Collimator = created };
    }

    public override async Task<UpdateCollimatorResponse> UpdateCollimator(
        UpdateCollimatorRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Collimator.Id, request.Collimator);
        return new UpdateCollimatorResponse { Collimator = updated };
    }

    public override async Task<DeleteCollimatorResponse> DeleteCollimator(
        DeleteCollimatorRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteCollimatorResponse();
    }
}
