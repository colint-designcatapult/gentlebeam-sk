using Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class HeaterCurrentConfigServiceImpl : HeaterCurrentConfigService.HeaterCurrentConfigServiceBase
{
    private readonly SqliteProtoRepository<HeaterCurrentConfig> _repo;
    public HeaterCurrentConfigServiceImpl(SqliteProtoRepository<HeaterCurrentConfig> repo) => _repo = repo;

    public override async Task<ListHeaterCurrentConfigsResponse> ListHeaterCurrentConfigs(
        ListHeaterCurrentConfigsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListHeaterCurrentConfigsResponse();
        r.HeaterCurrentConfigs.AddRange(items);
        return r;
    }

    public override async Task<GetHeaterCurrentConfigResponse> GetHeaterCurrentConfig(
        GetHeaterCurrentConfigRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"HeaterCurrentConfig {request.Id} not found"));
        return new GetHeaterCurrentConfigResponse { HeaterCurrentConfig = item };
    }

    public override async Task<CreateHeaterCurrentConfigResponse> CreateHeaterCurrentConfig(
        CreateHeaterCurrentConfigRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.HeaterCurrentConfig, request.HeaterCurrentConfig.PresetConfigurationId);
        return new CreateHeaterCurrentConfigResponse { HeaterCurrentConfig = created };
    }

    public override async Task<UpdateHeaterCurrentConfigResponse> UpdateHeaterCurrentConfig(
        UpdateHeaterCurrentConfigRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.HeaterCurrentConfig.Id, request.HeaterCurrentConfig);
        return new UpdateHeaterCurrentConfigResponse { HeaterCurrentConfig = updated };
    }

    public override async Task<DeleteHeaterCurrentConfigResponse> DeleteHeaterCurrentConfig(
        DeleteHeaterCurrentConfigRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteHeaterCurrentConfigResponse();
    }
}
