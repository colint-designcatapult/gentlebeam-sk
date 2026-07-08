using Com.Empyreanmed.Heracles.CollimatorConfigurations.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class CollimatorConfigurationServiceImpl
    : CollimatorConfigurationService.CollimatorConfigurationServiceBase
{
    private readonly SqliteProtoRepository<CollimatorConfiguration> _repo;
    public CollimatorConfigurationServiceImpl(SqliteProtoRepository<CollimatorConfiguration> repo) => _repo = repo;

    public override async Task<ListCollimatorConfigurationsResponse> ListCollimatorConfigurations(
        ListCollimatorConfigurationsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListCollimatorConfigurationsResponse();
        r.CollimatorConfigurations.AddRange(items);
        return r;
    }

    public override async Task<SearchCollimatorConfigurationResponse> SearchCollimatorConfiguration(
        SearchCollimatorConfigurationRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        IEnumerable<CollimatorConfiguration> filtered = all;
        if (request.HasTargetType)
            filtered = filtered.Where(c => c.Type == request.TargetType);
        if (request.HasEnergy)
            filtered = filtered.Where(c => c.Energy == request.Energy);
        if (request.HasSsd)
            filtered = filtered.Where(c => c.Ssd == request.Ssd);
        var match = filtered.FirstOrDefault();
        return new SearchCollimatorConfigurationResponse
        {
            CollimatorConfigurations = match ?? new CollimatorConfiguration()
        };
    }

    public override async Task<GetCollimatorConfigurationResponse> GetCollimatorConfiguration(
        GetCollimatorConfigurationRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"CollimatorConfiguration {request.Id} not found"));
        return new GetCollimatorConfigurationResponse { CollimatorConfiguration = item };
    }

    public override async Task<CreateCollimatorConfigurationResponse> CreateCollimatorConfiguration(
        CreateCollimatorConfigurationRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.CollimatorConfiguration);
        return new CreateCollimatorConfigurationResponse { CollimatorConfiguration = created };
    }

    public override async Task<UpdateCollimatorConfigurationResponse> UpdateCollimatorConfiguration(
        UpdateCollimatorConfigurationRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(
            request.CollimatorConfiguration.Id, request.CollimatorConfiguration);
        return new UpdateCollimatorConfigurationResponse { CollimatorConfiguration = updated };
    }

    public override async Task<DeleteCollimatorConfigurationResponse> DeleteCollimatorConfiguration(
        DeleteCollimatorConfigurationRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteCollimatorConfigurationResponse();
    }
}
