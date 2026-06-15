using System;
using System.Diagnostics;
using Grpc.Core.Interceptors;
using Xcc.Core.Logging;
using Xcc.Infra.Persistence.DataAccess.gRPC.Interceptors;

namespace Xcc.Infra.Logging.gRPC
{
    public class ErrorLoggingInterceptor(ILogWriter logWriter)
    : AbstractGrpcInvocationErrorInterceptor
    {
        public override void OnError<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context, Exception ex)
        {
            // We don't write log's create requests, as we handle this case separately in DbLogRepository
            if (context.Method.Name != "CreateLog")
            {
                _ = logWriter.LogAsync(
                    $"Record&Verify Error. ServiceCall={context.Method.ServiceName}/{context.Method.Name}. Reason: {ex.Message}",
                    Core.Enums.LogRecordSeverity.Error, Core.Enums.LogRecordType.System);
                Debug.WriteLine($"{DateTime.Now.TimeOfDay} Record&Verify Error. ServiceCall={context.Method.ServiceName}/{context.Method.Name}. Reason: {ex.Message}");
            }
        }
    }
}
