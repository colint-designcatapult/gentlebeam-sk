using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Xcc.Infra.Networking.Udp
{
    public interface IUdpBroadcaster
    {
        IPEndPoint EndPoint { get; }
        int Send(byte[] data);
        Task<int> SendAsync(byte[] data);
    }

    public class UdpBroadcaster(IPAddress broadcastAddress, int broadcastPort) : IUdpBroadcaster, IDisposable
    {
        public UdpBroadcaster(int broadcastPort)
            : this(IPAddress.Broadcast, broadcastPort)
        { }

        public int Send(byte[] data)
        {
            return _client.Send(data, data.Length, _broadcastEndpoint);
        }

        public Task<int> SendAsync(byte[] data)
        {
            return _client.SendAsync(data, data.Length, _broadcastEndpoint);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public IPEndPoint EndPoint => _broadcastEndpoint;

        private UdpClient _client = UdpClientFactory.GetBroadcastClient();
        private IPEndPoint _broadcastEndpoint = new(broadcastAddress, broadcastPort);
    }
}
