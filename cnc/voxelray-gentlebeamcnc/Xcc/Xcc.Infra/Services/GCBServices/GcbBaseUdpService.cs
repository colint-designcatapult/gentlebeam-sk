using System;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Events;
using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;

namespace Xcc.Infra.Services.GcbServices
{   
    /// <summary>
    /// Deprecated. Use IRawUdpClient from Empyrean.Common.Infra.Networking.Udp instead
    /// </summary>
    [Obsolete]
    public interface IUdpClientRaw : IDisposable
    {
        event EventHandler<UdpReceiveErrorEventArgs> UdpReceiveErrorEvent;

        void Start();

        void Stop();

        Task<byte[]> SendRequestAsync(byte[] packet);
        Task<byte[]> SendRequestAsync(byte[] packet, int timeoutMs);
        Task SendMessageAsync(byte[] packet);
    }

    /// <summary>
    /// Deprecated. Use RawUdpClient from Empyrean.Common.Infra.Networking.Udp instead
    /// </summary>
    [Obsolete]
    public class GcbBaseUdpService : IUdpClientRaw
    {
        public GcbBaseUdpService(
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

        public async Task<byte[]> SendRequestAsync(byte[] packet)
        {
            var result = await _client.SendRequestAsync(new UdpPacket(packet));
            return result.Buffer;
        }

        public async Task<byte[]> SendRequestAsync(byte[] packet, int timeoutMs)
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
