using System.Net.Sockets;
using System.Threading;
using System;

namespace Xcc.Infra.Networking.Udp
{
    public class UdpReceiver(int clientPort, bool reusePort = false)
        : UdpServer(serverPort: clientPort, reusePort: reusePort)
    {
        public event EventHandler<UdpReceiveResult>? UdpReceiveEvent;

        protected override void HandleRequest(UdpClient client, CancellationToken cancellationToken, UdpReceiveResult incomingData)
        {
            UdpReceiveEvent?.Invoke(this, incomingData);
        }
    }
}
