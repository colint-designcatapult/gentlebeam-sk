using System.Diagnostics;
using System.Net.Sockets;

namespace Empyrean.Common.Test.TestUtils
{
    public class Network
    {
        public static async Task RunOnetimeUdpServer(
            int port,
            CancellationToken cancellationToken,
            Action<UdpClient, System.Net.IPEndPoint?, byte[]> action)
        {
            await RunNTimeUdpServer(port, cancellationToken, action, 1);
        }

        public static async Task RunNTimeUdpServer(
            int port,
            CancellationToken cancellationToken,
            Action<UdpClient, System.Net.IPEndPoint?, byte[]> action, int requestsToHandle)
        {
            try
            {
                using (var server = new UdpClient(port, AddressFamily.InterNetwork))
                {
                    // Enable address reuse, just in case if some previous socket wasn't release until now
                    server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                    for (int requestIndex = 0; requestIndex < requestsToHandle; ++requestIndex)
                    {
                        var result = await server.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                        action.Invoke(server, result.RemoteEndPoint, result.Buffer);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TestUtils: RunNTimeUdpServer error - {ex.Message}");
            }
        }
    }
}
