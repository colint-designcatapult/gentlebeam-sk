using Empyrean.Common.Infra.Events;

namespace Empyrean.Common.Infra.Networking.Udp;

public interface IUdpClient : IDisposable
{
    event EventHandler<UdpReceiveErrorEventArgs> UdpReceiveErrorEvent;

    void Start();

    void Stop();

    Task<UdpPacket> SendRequestAsync(UdpPacket packet);
    Task<UdpPacket> SendRequestAsync(UdpPacket packet, int timeoutMs);
    Task<int> SendMessageAsync(UdpPacket packet);
}