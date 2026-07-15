using Com.Empyreanmed.Heracles.OutputFactors.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class OutputFactorServiceImpl : OutputFactorService.OutputFactorServiceBase
{
    private readonly SqliteProtoRepository<OutputFactor> _repo;
    public OutputFactorServiceImpl(SqliteProtoRepository<OutputFactor> repo) => _repo = repo;

    public override async Task<ListOutputFactorsResponse> ListOutputFactors(
        ListOutputFactorsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListOutputFactorsResponse();
        r.OutputFactors.AddRange(items);
        return r;
    }

    public override async Task<GetOutputFactorResponse> GetOutputFactor(
        GetOutputFactorRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"OutputFactor {request.Id} not found"));
        return new GetOutputFactorResponse { OutputFactor = item };
    }

    public override async Task<CreateOutputFactorResponse> CreateOutputFactor(
        CreateOutputFactorRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.OutputFactor, request.OutputFactor.PresetConfigurationId);
        return new CreateOutputFactorResponse { OutputFactor = created };
    }

    public override async Task<UpdateOutputFactorResponse> UpdateOutputFactor(
        UpdateOutputFactorRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.OutputFactor.Id, request.OutputFactor);
        return new UpdateOutputFactorResponse { OutputFactor = updated };
    }

    public override async Task<DeleteOutputFactorResponse> DeleteOutputFactor(
        DeleteOutputFactorRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteOutputFactorResponse();
    }
}
