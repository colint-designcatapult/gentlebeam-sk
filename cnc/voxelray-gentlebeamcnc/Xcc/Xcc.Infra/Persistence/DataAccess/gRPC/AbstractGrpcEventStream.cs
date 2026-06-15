using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Infra.Persistence.DataAccess.gRPC
{
    public abstract class AbstractGrpcEventStream<StreamArgsType>(
        IBearerTokenUserSessionManager sessionManager) : IEventStream<StreamArgsType>
    {

        protected abstract Task HandleStreamAsync(Action<StreamArgsType> streamCallback, CancellationToken token);

        public async Task RunStreamAsync(Action<StreamArgsType> streamCallback, CancellationToken cancellationToken)
        {
            try
            {
                var sessionCancellationToken = 
                    sessionManager.UserSession?.CancellationToken ?? 
                    throw new NullReferenceException("User session does not exist");

                using var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken, sessionCancellationToken);

                await HandleStreamAsync(streamCallback, tokenSource.Token);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                throw new OperationCanceledException($"{GetType().Name} - stream reading was cancelled", ex);
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.Unavailable || ex.StatusCode == StatusCode.PermissionDenied)
                {
                    sessionManager.ExpireUserSession();
                }
                throw;
            }
            catch (Exception ex)
            {
                string msg = $"{GetType().Name} - Failed to receive events";
                throw new DataServiceException(msg, ex);
            }
        }
    }
}