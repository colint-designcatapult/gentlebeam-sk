using Com.Empyreanmed.Heracles.System.V1;
using Grpc.Core;
using System.Reflection;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class SystemServiceImpl : SystemService.SystemServiceBase
{
    public override Task<GetSystemInfoResponse> GetSystemInfo(
        GetSystemInfoRequest request, ServerCallContext context)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        var info = new Com.Empyreanmed.Heracles.System.V1.System { Version = version };
        return Task.FromResult(new GetSystemInfoResponse { System = info });
    }
}
