using Com.Empyreanmed.Heracles.CorrectionMatrix.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class CorrectionMatrixServiceImpl : CorrectionMatrixService.CorrectionMatrixServiceBase
{
    private readonly SqliteProtoRepository<CorrectionMatrix> _repo;
    public CorrectionMatrixServiceImpl(SqliteProtoRepository<CorrectionMatrix> repo) => _repo = repo;

    public override async Task<ListCorrectionMatricesResponse> ListCorrectionMatrices(
        ListCorrectionMatricesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListCorrectionMatricesResponse();
        r.CorrectionMatrices.AddRange(items);
        return r;
    }

    public override async Task<GetCorrectionMatrixResponse> GetCorrectionMatrix(
        GetCorrectionMatrixRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"CorrectionMatrix {request.Id} not found"));
        return new GetCorrectionMatrixResponse { CorrectionMatrix = item };
    }

    public override async Task<CreateCorrectionMatrixResponse> CreateCorrectionMatrix(
        CreateCorrectionMatrixRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.CorrectionMatrix, request.CorrectionMatrix.PresetConfigurationId);
        return new CreateCorrectionMatrixResponse { CorrectionMatrix = created };
    }

    public override async Task<UpdateCorrectionMatrixResponse> UpdateCorrectionMatrix(
        UpdateCorrectionMatrixRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.CorrectionMatrix.Id, request.CorrectionMatrix);
        return new UpdateCorrectionMatrixResponse { CorrectionMatrix = updated };
    }

    public override async Task<DeleteCorrectionMatrixResponse> DeleteCorrectionMatrix(
        DeleteCorrectionMatrixRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteCorrectionMatrixResponse();
    }
}
