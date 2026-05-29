using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Xcc.Infra.Persistence.DataAccess.gRPC.Interceptors;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Application.AppLayer.UserSessions
{
    public class GrpcRequestFailureInterceptor(IGrpcBearerTokenUserSessionManager sessionManager)
        : AbstractGrpcInvocationErrorInterceptor
    {
        public override void OnError<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, Exception ex)
        {
            if (ex is RpcException rpcEx)
            {
                switch (rpcEx.StatusCode)
                {
                    case StatusCode.Unavailable:
                    case StatusCode.PermissionDenied:
                        try
                        {
                            sessionManager.ExpireUserSession();
                        }
                        catch { } // we ignore any errors coming from session not existing
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
