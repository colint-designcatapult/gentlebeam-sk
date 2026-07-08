using Com.Empyreanmed.Heracles.CoilConfigurations.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class CoilConfigurationServiceImpl : CoilConfigurationService.CoilConfigurationServiceBase
{
    private readonly SqliteProtoRepository<CoilConfiguration> _repo;
    public CoilConfigurationServiceImpl(SqliteProtoRepository<CoilConfiguration> repo) => _repo = repo;

    public override async Task<ListCoilConfigurationsResponse> ListCoilConfigurations(
        ListCoilConfigurationsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListCoilConfigurationsResponse();
        r.CoilConfigurations.AddRange(items);
        return r;
    }

    public override async Task<GetCoilConfigurationResponse> GetCoilConfiguration(
        GetCoilConfigurationRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"CoilConfiguration {request.Id} not found"));
        return new GetCoilConfigurationResponse { CoilConfiguration = item };
    }

    public override async Task<CreateCoilConfigurationResponse> CreateCoilConfiguration(
        CreateCoilConfigurationRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.CoilConfiguration, request.CoilConfiguration.PresetConfigurationId);
        return new CreateCoilConfigurationResponse { CoilConfiguration = created };
    }

    public override async Task<UpdateCoilConfigurationResponse> UpdateCoilConfiguration(
        UpdateCoilConfigurationRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.CoilConfiguration.Id, request.CoilConfiguration);
        return new UpdateCoilConfigurationResponse { CoilConfiguration = updated };
    }

    public override async Task<DeleteCoilConfigurationResponse> DeleteCoilConfiguration(
        DeleteCoilConfigurationRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteCoilConfigurationResponse();
    }
}
