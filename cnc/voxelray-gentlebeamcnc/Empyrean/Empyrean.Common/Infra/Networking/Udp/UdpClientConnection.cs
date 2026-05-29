using System.Net;
using System.Net.Sockets;

namespace Empyrean.Common.Infra.Networking.Udp
{
    public interface IUdpClientConnection : IAsyncClientConnection
    {
        void SetEndpoint(string hostAddress, int hostPort);
    }

    public class UdpClientConnection : IUdpClientConnection
    {
        private readonly UdpClient _udpClient;
        private string _hostAddress;
        private int _port;
        private bool _disposed;

        public UdpClientConnection(string hostAddress, int hostPort)
        {
            _hostAddress = hostAddress;
            _port = hostPort;
            _udpClient = new();
        }

        public UdpClientConnection(string hostAddress, int hostPort, int clientPort, bool reusePort = false)
        {
            _hostAddress = hostAddress;
            _port = hostPort;
            _udpClient = new();
            if (reusePort)
            {
                // Since we set client port, we probably listen to broadcast messages,
                // so to be able to function more robustly let's enable port reuse:
                _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, clientPort));
        }
        
        public async Task<byte[]> ReceiveAsync(CancellationToken cancellationToken)
        {
            var response = await _udpClient.ReceiveAsync(cancellationToken);
            return response.Buffer;
        }

        public async Task<int> SendAsync(byte[] data)
        {
            return await _udpClient.SendAsync(data, data.Length, _hostAddress, _port);
        }

        public void SetEndpoint(string hostAddress, int hostPort)
        {
            _hostAddress = hostAddress;
            _port = hostPort;
        }

        ~UdpClientConnection()
        {
            Dispose(false);
        }

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
                if (_udpClient != null)
                {
                    _udpClient.Close();
                    _udpClient.Dispose();
                }
            }

            _disposed = true;
        }
    }
}
