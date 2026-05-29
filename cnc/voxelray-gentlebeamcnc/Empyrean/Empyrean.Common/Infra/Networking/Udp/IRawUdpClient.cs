using Empyrean.Common.Infra.Events;

namespace Empyrean.Common.Infra.Networking.Udp;

public interface IRawUdpClient : IDisposable
{
    event EventHandler<UdpReceiveErrorEventArgs> UdpReceiveErrorEvent;

    void Start();

    void Stop();

    Task<byte[]?> SendRequestAsync(byte[] packet);
    Task<byte[]?> SendRequestAsync(byte[] packet, int timeoutMs);
    Task SendMessageAsync(byte[] packet);
}