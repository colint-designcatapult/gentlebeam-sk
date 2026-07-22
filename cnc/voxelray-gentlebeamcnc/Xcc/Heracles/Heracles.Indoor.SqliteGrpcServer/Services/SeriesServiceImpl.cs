using Com.Empyreanmed.Heracles.Series.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

/// <summary>
/// SQLite-backed SeriesService. DICOM binary streaming is accepted but not persisted
/// to disk (embedded server stores metadata only).
/// </summary>
public sealed class SeriesServiceImpl : SeriesService.SeriesServiceBase
{
    private readonly SqliteProtoRepository<Series> _repo;
    public SeriesServiceImpl(SqliteProtoRepository<Series> repo) => _repo = repo;

    // ── CRUD ─────────────────────────────────────────────────────────────────

    public override async Task<ListSeriesResponse> ListSeries(ListSeriesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadByParentIdAsync(request.DiagnosisId);
        var r = new ListSeriesResponse();
        r.Series.AddRange(items);
        return r;
    }

    public override async Task<ListSeriesByPatientIdResponse> ListSeriesByPatientId(
        ListSeriesByPatientIdRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        var r = new ListSeriesByPatientIdResponse();
        r.Series.AddRange(all); // full scan – patient_id is not stored as parent_id here
        return r;
    }

    public override async Task<GetSeriesResponse> GetSeries(GetSeriesRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.SeriesId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Series {request.SeriesId} not found"));
        return new GetSeriesResponse { Series = item };
    }

    public override async Task<CreateSeriesResponse> CreateSeries(CreateSeriesRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Series, request.Series.DiagnosisId);
        return new CreateSeriesResponse { Series = created };
    }

    public override async Task<UpdateSeriesResponse> UpdateSeries(UpdateSeriesRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Series.Id, request.Series);
        return new UpdateSeriesResponse { Series = updated };
    }

    public override async Task<DeleteSeriesResponse> DeleteSeries(DeleteSeriesRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.SeriesId);
        return new DeleteSeriesResponse();
    }

    // ── DICOM streaming (metadata-only for embedded server) ──────────────────

    public override async Task GetDicom(
        GetDicomRequest request,
        IServerStreamWriter<GetDicomResponse> responseStream,
        ServerCallContext context)
    {
        // No stored DICOM data in the embedded server; send nothing.
        await Task.CompletedTask;
    }

    public override async Task<SendDicomResponse> SendDicom(
        IAsyncStreamReader<SendDicomRequest> requestStream,
        ServerCallContext context)
    {
        int count = 0;
        await foreach (var _ in requestStream.ReadAllAsync(context.CancellationToken))
            count++;

        return new SendDicomResponse { Message = "OK", TotalFilesReceived = count };
    }

    public override async Task<ReceiveDicomResponse> ReceiveDicom(
        IAsyncStreamReader<ReceiveDicomRequest> requestStream,
        ServerCallContext context)
    {
        await foreach (var _ in requestStream.ReadAllAsync(context.CancellationToken)) { }
        return new ReceiveDicomResponse();
    }
}
