using Com.Empyreanmed.Heracles.Settings.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class SettingsServiceImpl : SettingsService.SettingsServiceBase
{
    private readonly SqliteProtoRepository<Settings> _repo;
    public SettingsServiceImpl(SqliteProtoRepository<Settings> repo) => _repo = repo;

    public override async Task<GetSettingsResponse> GetSettings(
        GetSettingsRequest request, ServerCallContext context)
    {
        var settings = await _repo.ReadSingleAsync();
        if (settings is null)
        {
            settings = new Settings();
            settings = await _repo.CreateAsync(settings);
        }
        return new GetSettingsResponse { Settings = settings };
    }

    public override async Task<UpdateSettingsResponse> UpdateSettings(
        UpdateSettingsRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpsertSingleAsync(request.Settings);
        return new UpdateSettingsResponse { Settings = updated };
    }
}
