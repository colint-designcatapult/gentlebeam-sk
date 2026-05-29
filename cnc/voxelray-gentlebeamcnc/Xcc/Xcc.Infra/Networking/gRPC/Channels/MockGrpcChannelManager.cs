using Grpc.Core;
using Grpc.Core.Interceptors;
using System;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Infra.Networking.gRPC.Channels
{
    public class MockGrpcChannelManager(
        IBearerTokenUserSessionManager bearerTokenUserSessionManager) : IGrpcChannelManager
    {
        public TimeSpan SessionExpirationTimeout = TimeSpan.FromHours(1) + TimeSpan.FromSeconds(15);

        public Metadata Headers { get; set; } = new Metadata();

        public uint RpcTimeoutMs { get; set; }

        public CallInvoker Channel => throw new NotImplementedException();

        public DateTime GetRpcDeadline(int timeoutMs = -1)
        {
            throw new NotImplementedException();
        }

        public void InterceptChannel(Interceptor interceptor)
        {
            throw new NotImplementedException();
        }

        public void ShutdownChannel() 
        {
            bearerTokenUserSessionManager.CloseUserSession();
        }
    }
}
