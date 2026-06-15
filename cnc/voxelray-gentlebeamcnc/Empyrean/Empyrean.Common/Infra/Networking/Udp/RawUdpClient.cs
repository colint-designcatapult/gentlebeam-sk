using Empyrean.Common.Infra.Events;

namespace Empyrean.Common.Infra.Networking.Udp
{
    public class RawUdpClient : IRawUdpClient
    {
        public RawUdpClient(
            CancellationToken cancellationToken,
            IAsyncClientConnection connection,
            int? receiveTimeout = null)
        {
            _client = new BaseUdpClient(cancellationToken, connection, receiveTimeout);

            _client.UdpReceiveErrorEvent += (s, e) => UdpReceiveErrorEvent?.Invoke(s, e);
        }

        public event EventHandler<UdpReceiveErrorEventArgs>? UdpReceiveErrorEvent;

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task SendMessageAsync(byte[] packet)
        {
            return _client.SendMessageAsync(new UdpPacket(packet));
        }

        public async Task<byte[]?> SendRequestAsync(byte[] packet)
        {
            var result = await _client.SendRequestAsync(new UdpPacket(packet));
            return result.Buffer;
        }

        public async Task<byte[]?> SendRequestAsync(byte[] packet, int timeoutMs)
        {
            var result = await _client.SendRequestAsync(new UdpPacket(packet), timeoutMs);
            return result.Buffer;
        }

        public void Start()
        {
            _client.Start();
        }

        public void Stop()
        {
            _client.Stop();
        }

        protected virtual int GetMessageId(byte[] buffer)
        {
            // Get GCB package type identifier
            var packet = new UdpPacket(buffer);
            return (int)packet.PacketCounter;
        }

        protected virtual bool IsValidMessage(byte[]? buffer)
        {
            return buffer is not null;
        }

        private readonly IUdpClient _client;
    }
}
