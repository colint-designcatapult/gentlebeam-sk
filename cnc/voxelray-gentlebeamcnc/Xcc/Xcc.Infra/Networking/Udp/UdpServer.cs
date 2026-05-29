using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Xcc.Infra.Networking.Udp
{
    public abstract class UdpServer(int serverPort, bool reusePort = false, int restartTimeoutMs = 50)
    {
        public void Start(CancellationToken? token = null)
        {
            if (_servCancellationTokenSource != null)
            {
                throw new InvalidOperationException("Cannot start UDP receiver, it is already running");
            }

            _servCancellationTokenSource = (token is null)
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(token.Value);

            var localToken = _servCancellationTokenSource.Token;

            _servingTask = Task.Run(async () =>
            {
                while (!localToken.IsCancellationRequested)
                {
                    using var _udpClient = UdpClientFactory.GetBindedClient(serverPort, reusePort);
                    try
                    {
                        while (!localToken.IsCancellationRequested)
                        {
                            var incomingData = await _udpClient.ReceiveAsync(localToken);
                            HandleRequest(_udpClient, localToken, incomingData);
                        }
                    }
                    finally
                    {
                        _udpClient.Close();
                    }
                    await Task.Delay(restartTimeoutMs, localToken);
                }
            }, localToken);
        }

        protected abstract void HandleRequest(UdpClient client, CancellationToken cancellationToken, UdpReceiveResult incomingData);

        public async Task Stop()
        {
            _servCancellationTokenSource?.Cancel();
            _servCancellationTokenSource = null;
            if (_servingTask != null)
            {
                await _servingTask;
                _servingTask = null;
            }
        }

        private CancellationTokenSource? _servCancellationTokenSource;
        private Task? _servingTask;
    }
}
