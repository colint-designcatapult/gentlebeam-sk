using System.Net.Sockets;
using Empyrean.Common.Infra.Events;

namespace Empyrean.Common.Infra.Networking.Udp
{
    public class BaseUdpClient : IUdpClient
    {
        private readonly int DefaultReceiveTimeout = 5000;
        private readonly int _receiveTimeout;
        /// <summary>
        /// Semaphore for Send operation thread-safety:
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly CancellationToken? _globalToken;
        private readonly UdpMessageLine _messageLine = new();

        private CancellationTokenSource _recvCancellationTokenSource = null!;
        private Task _recvTask = null!;
        private bool _disposed;

        protected IAsyncClientConnection Connection { get; }

        public event EventHandler<UdpReceiveErrorEventArgs>? UdpReceiveErrorEvent;

        public BaseUdpClient(
            CancellationToken cancellationToken,
            IAsyncClientConnection connection,
            int? receiveTimeout = null)
        {
            Connection = connection;
            _receiveTimeout = receiveTimeout != null ? receiveTimeout.Value : DefaultReceiveTimeout;
            _globalToken = cancellationToken;
        }

        public void Start()
        {
            if (_recvCancellationTokenSource != null)
            {
                throw new InvalidOperationException("Cannot start UDP Service, it is already running");
            }
            _recvCancellationTokenSource = _globalToken is null
                ? new CancellationTokenSource()
                : CancellationTokenSource.CreateLinkedTokenSource(_globalToken.Value);

            var localToken = _recvCancellationTokenSource.Token;
            _recvTask = Task.Run(async () =>
            {
                bool noConnection = false;
                while (!localToken.IsCancellationRequested)
                {
                    try
                    {
                        var responseMessage = await Connection.ReceiveAsync(localToken);
                        noConnection = false;
                        if (IsValidMessage(responseMessage))
                        {
                            _messageLine.AddResponse(GetMessageId(responseMessage), responseMessage);
                        }
                    }
                    catch (SocketException ex) when 
                        (ex.SocketErrorCode == SocketError.OperationAborted || ex.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        UdpReceiveErrorEvent?.Invoke(this,
                            new UdpReceiveErrorEventArgs { Exception = ex, Message = $"UdpService failure: {ex.Message}" });
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        if (noConnection == false)
                        {
                            noConnection = true;

                            UdpReceiveErrorEvent?.Invoke(this,
                                new UdpReceiveErrorEventArgs() { Exception = ex, Message = $"UdpService receiving error: {ex.Message}" });
                        }
                    }
                }
            }, localToken);
        }

        public void Stop()
        {
            _recvCancellationTokenSource?.Cancel();
            _recvCancellationTokenSource = null!;
        }
        
        public async Task<UdpPacket> SendRequestAsync(UdpPacket packet)
        {
            return await SendRequestAsync(packet, _receiveTimeout);
        }

        public async Task<UdpPacket> SendRequestAsync(UdpPacket packet, int timeoutMs)
        {
            if (Connection == null)
                return null!;

            if (_recvCancellationTokenSource is null || _recvCancellationTokenSource.IsCancellationRequested)
            {
                throw new InvalidOperationException("UdpService request sending error: Receiving task is not running");
            }

            int messageId = (int) packet.PacketCounter;

            _messageLine.AddRequest(messageId);

            byte[]? response = null;
            try
            {
                int bytesSent = await SendAsync(packet.Buffer);

                if (bytesSent > 0)
                    response = await _messageLine.WaitForResponseAsync(messageId, timeoutMs, _recvCancellationTokenSource.Token);

                if (response == null)
                    throw new UdpException("UdpService: response is null");
            }
            catch (Exception ex)
            {
                throw new UdpException(ex.Message);
            }
            finally
            {
                // Remove request from the line anyway, as we aren't going to wait for it more:
                _messageLine.RemoveRequest(messageId);
            }

            return new UdpPacket(response);
        }

        public async Task<int> SendMessageAsync(UdpPacket packet)
        {
            if (Connection == null)
                return 0;

            try
            {
                return await SendAsync(packet.Buffer);
            }
            catch (Exception ex)
            {
                throw new UdpException($"UdpService message sending error: {ex.Message}");
            }
        }

        protected virtual int GetMessageId(byte[] buffer)
        {
            // Get GCB package type identifier
            UdpPacket packet = new UdpPacket(buffer);
            return (int)packet.PacketCounter;
        }

        protected virtual bool IsValidMessage(byte[]? buffer)
        {
            return buffer is not null;
        }

        private async Task<int> SendAsync(byte[] buffer)
        {
            await _semaphore.WaitAsync();
            int bytesSent;
            try
            {
                bytesSent = await Connection.SendAsync(buffer);
            }
            finally
            {
                _semaphore.Release();
            }
            return bytesSent;
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _recvCancellationTokenSource?.Cancel();
                _recvCancellationTokenSource?.Dispose();
                Connection?.Dispose(); 
            }

            _disposed = true;
        }
        #endregion
    }
}
