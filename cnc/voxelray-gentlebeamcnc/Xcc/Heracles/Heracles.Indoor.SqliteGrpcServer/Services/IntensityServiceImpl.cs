using Com.Empyreanmed.Heracles.Intensities.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class IntensityServiceImpl : IntensityService.IntensityServiceBase
{
    private readonly SqliteProtoRepository<Intensity> _repo;
    public IntensityServiceImpl(SqliteProtoRepository<Intensity> repo) => _repo = repo;

    public override async Task<ListIntensitiesResponse> ListIntensities(
        ListIntensitiesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListIntensitiesResponse();
        r.Intensities.AddRange(items);
        return r;
    }

    public override async Task<GetIntensityResponse> GetIntensity(
        GetIntensityRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Intensity {request.Id} not found"));
        return new GetIntensityResponse { Intensity = item };
    }

    public override async Task<CreateIntensityResponse> CreateIntensity(
        CreateIntensityRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Intensity, request.Intensity.QcsampleFieldsId);
        return new CreateIntensityResponse { Intensity = created };
    }

    public override async Task<UpdateIntensityResponse> UpdateIntensity(
        UpdateIntensityRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Intensity.Id, request.Intensity);
        return new UpdateIntensityResponse { Intensity = updated };
    }

    public override async Task<DeleteIntensityResponse> DeleteIntensity(
        DeleteIntensityRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteIntensityResponse();
    }
}
