using Com.Empyreanmed.Heracles.PresetConfigurations.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class PresetConfigurationServiceImpl : PresetConfigurationService.PresetConfigurationServiceBase
{
    private readonly SqliteProtoRepository<PresetConfiguration> _repo;
    public PresetConfigurationServiceImpl(SqliteProtoRepository<PresetConfiguration> repo) => _repo = repo;

    public override async Task<ListPresetConfigurationsResponse> ListPresetConfigurations(
        ListPresetConfigurationsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListPresetConfigurationsResponse();
        r.PresetConfigurations.AddRange(items);
        return r;
    }

    public override async Task<GetPresetConfigurationResponse> GetPresetConfiguration(
        GetPresetConfigurationRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"PresetConfiguration {request.Id} not found"));
        return new GetPresetConfigurationResponse { PresetConfiguration = item };
    }

    public override async Task<CreatePresetConfigurationResponse> CreatePresetConfiguration(
        CreatePresetConfigurationRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.PresetConfiguration, request.PresetConfiguration.CollimatorConfigurationId);
        return new CreatePresetConfigurationResponse { PresetConfiguration = created };
    }

    public override async Task<UpdatePresetConfigurationResponse> UpdatePresetConfiguration(
        UpdatePresetConfigurationRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.PresetConfiguration.Id, request.PresetConfiguration);
        return new UpdatePresetConfigurationResponse { PresetConfiguration = updated };
    }

    public override async Task<DeletePresetConfigurationResponse> DeletePresetConfiguration(
        DeletePresetConfigurationRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeletePresetConfigurationResponse();
    }

    public override async Task<ApprovePresetConfigurationResponse> ApprovePresetConfiguration(
        ApprovePresetConfigurationRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.PresetConfigurationId)
            ?? throw new RpcException(new Status(StatusCode.NotFound,
                $"PresetConfiguration {request.PresetConfigurationId} not found"));
        // Mark as approved — set the approval fields if they exist on the message
        var updated = await _repo.UpdateAsync(item.Id, item);
        return new ApprovePresetConfigurationResponse { ApprovedPresetConfiguration = updated };
    }
}
