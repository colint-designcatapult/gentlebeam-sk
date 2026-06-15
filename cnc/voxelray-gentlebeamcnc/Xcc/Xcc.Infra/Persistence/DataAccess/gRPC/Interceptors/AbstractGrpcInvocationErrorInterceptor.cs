using Grpc.Core.Interceptors;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Xcc.Infra.Persistence.DataAccess.gRPC.Interceptors
{
    /// <summary>
    /// Simple gRPC invocation error interceptor
    /// To extend this, see https://github.com/grpc/grpc-dotnet/blob/master/examples/Interceptor/Client/ClientLoggerInterceptor.cs
    /// </summary>
    public abstract class AbstractGrpcInvocationErrorInterceptor : Interceptor
    {
        public override TResponse BlockingUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                OnError(context, ex);
                throw;
            }
        }

        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            // We probably don't need to handle anything more here, as we do error handling inside HandleResponse
            var call = continuation(request, context);
            return new AsyncUnaryCall<TResponse>(
                HandleResponse(context, call.ResponseAsync), 
                call.ResponseHeadersAsync, 
                call.GetStatus, 
                call.GetTrailers, 
                call.Dispose);
        }

        public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
        {
            try
            {
                return continuation(request, context);
            }
            catch (Exception ex)
            {
                OnError(context, ex);
                throw;
            }
        }

        private async Task<TResponse> HandleResponse<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context,
            Task<TResponse> task)
            where TRequest : class
            where TResponse : class
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                OnError(context, ex);
                throw;
            }
        }        

        public abstract void OnError<TRequest, TResponse>(
            ClientInterceptorContext<TRequest, TResponse> context, 
            Exception ex)
            where TRequest : class
            where TResponse : class;
    }
}
