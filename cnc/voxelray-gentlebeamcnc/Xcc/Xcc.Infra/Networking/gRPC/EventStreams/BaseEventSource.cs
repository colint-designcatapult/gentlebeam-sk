using Grpc.Core;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Infra.Networking.gRPC.EventStreams
{
    public abstract class BaseEventSource
    { 
        public const int ReconnectDelayMs = 3000;
        protected BaseEventSource(
            IConnectionLossStrategy connectionLossStrategy,
            CancellationToken globalCancellationToken)
        {
            _connectionLossStrategy = connectionLossStrategy;
            GlobalCancellationToken = globalCancellationToken;
        }

        private readonly IConnectionLossStrategy _connectionLossStrategy;
        protected CancellationToken GlobalCancellationToken { get; }
        private CancellationTokenSource? _currentTaskCancellationToken;
        
        public bool IsRunning => _currentTaskCancellationToken != null;

        public void Start()
        {
            if (!IsRunning)
            {
                _currentTaskCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(GlobalCancellationToken);
                var token = _currentTaskCancellationToken.Token;
                Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        try
                        {
                            await HandleEventStreamProcessing(token);
                        }
                        catch // to be able to reconnect 
                        {
                            await Task.Delay(ReconnectDelayMs, token); 
                        }
                    }
                }, token);
            }
            else
            {
                throw new InvalidOperationException("EventSource start error: already running");
            }
        }
        public void Stop()
        {
            _currentTaskCancellationToken?.Cancel();
            _currentTaskCancellationToken = null;
        }

        protected async Task HandleEventStreamProcessing(CancellationToken cancellationToken)
        {
            try
            {
                if (_connectionLossStrategy.CanConnect)
                {
                    await RunEventStreamProcessing(cancellationToken);
                    _connectionLossStrategy.OnConnect();
                }
            }
            catch (OperationCanceledException ex)
            {
                OnDisconnect(ex.Message);
                // We need to move it from running state to stop here:
                Stop();
                // We want to interrupt the stream handling task in this case,
                // as it was cancelled from outside:
                throw; 
            }
            catch (RpcException rpcExCancel)
            when (rpcExCancel.StatusCode == StatusCode.DeadlineExceeded)
            {
                OnDeadlineExceeded(rpcExCancel.Message);
            }
            catch (RpcException rpcExUnavailable)
            when (rpcExUnavailable.StatusCode == StatusCode.Unavailable)
            {
                if (!_connectionLossStrategy.Disconnected)
                {
                    _connectionLossStrategy.OnDisconnect();
                    OnDisconnect(rpcExUnavailable.Message);
                }
            }
        }

        protected virtual void OnDeadlineExceeded(string message)
        {
            Debug.WriteLine($"gRPC stream - deadline exceeded: {message}");
        }

        protected virtual void OnDisconnect(string message)
        {
            Debug.WriteLine($"gRPC stream - disconnect: {message}");
        }

        protected abstract Task RunEventStreamProcessing(CancellationToken cancellationToken);
    }
}
