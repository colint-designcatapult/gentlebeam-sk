using Com.Empyreanmed.Heracles.Logs.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class LogServiceImpl : LogService.LogServiceBase
{
    private readonly SqliteProtoRepository<Log> _repo;
    public LogServiceImpl(SqliteProtoRepository<Log> repo) => _repo = repo;

    public override async Task<ListLogsResponse> ListLogs(
        ListLogsRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListLogsResponse();
        r.Logs.AddRange(items);
        return r;
    }

    public override async Task<GetLogResponse> GetLog(
        GetLogRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Log {request.Id} not found"));
        return new GetLogResponse { Log = item };
    }

    public override async Task<CreateLogResponse> CreateLog(
        CreateLogRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Log);
        return new CreateLogResponse { Log = created };
    }

    public override async Task<UpdateLogResponse> UpdateLog(
        UpdateLogRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Log.Id, request.Log);
        return new UpdateLogResponse { Log = updated };
    }

    public override async Task<DeleteLogResponse> DeleteLog(
        DeleteLogRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteLogResponse();
    }

    public override async Task<SearchLogByMessageResponse> SearchLogByMessage(
        SearchLogByMessageRequest request, ServerCallContext context)
    {
        var all = await _repo.ReadAllAsync();
        var r = new SearchLogByMessageResponse();
        r.Logs.AddRange(all.Where(l => l.Message.Contains(request.Message, StringComparison.OrdinalIgnoreCase)));
        return r;
    }
}
