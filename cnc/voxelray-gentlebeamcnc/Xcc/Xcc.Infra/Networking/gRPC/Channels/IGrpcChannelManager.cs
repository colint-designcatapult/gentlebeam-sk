using Grpc.Core;
using Grpc.Core.Interceptors;

using System;

namespace Xcc.Infra.Networking.gRPC.Channels
{
    public interface IGrpcChannelManager
    {
        CallInvoker? Channel { get; }
        Metadata Headers { get; }
        uint RpcTimeoutMs { get; }

        DateTime GetRpcDeadline(int timeoutMs = -1);
        void InterceptChannel(Interceptor interceptor);
        void ShutdownChannel();
    }
}
