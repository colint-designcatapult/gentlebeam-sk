using System.Net.Sockets;
using System.Net;

namespace Xcc.Infra.Networking.Udp
{
    public static class UdpClientFactory {
        public static UdpClient GetBindedClient(int clientPort, bool reusePort = false)
        {
            return GetBindedClient(IPAddress.Any, clientPort, reusePort);
        }
        
        public static UdpClient GetBindedClient(IPAddress clientAddress, int clientPort, bool reusePort = false)
        {
            UdpClient udpClient = new();
            if (reusePort)
            {
                // Since we set client port, we probably listen to broadcast messages,
                // so to be able to function more robustly let's enable port reuse:
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }
            udpClient.Client.Bind(new IPEndPoint(clientAddress, clientPort));
            return udpClient;
        }

        public static UdpClient GetBroadcastClient()
        {
            UdpClient udpClient = new();
            udpClient.EnableBroadcast = true;
            return udpClient;
        }
    }

}
