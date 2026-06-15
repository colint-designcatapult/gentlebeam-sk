using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.System.V1;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.Common.Invokers;

public class GrpcSystemMethodsInvoker : AbstractBaseGrpcInvoker<Com.Empyreanmed.Heracles.System.V1.System>
{
    public GrpcSystemMethodsInvoker(IGrpcChannelManager grpcSettings)
        : base(grpcSettings)
    {
        Client = new SystemService.SystemServiceClient(Channel);
    }

    public SystemService.SystemServiceClient Client { get; }
    
    public async Task<Com.Empyreanmed.Heracles.System.V1.System> GetSystemInfoAsync()
    {
        var request = new GetSystemInfoRequest();

        var response = await CallWithOptions(Client.GetSystemInfoAsync, request);
        return response.System;
    }
}