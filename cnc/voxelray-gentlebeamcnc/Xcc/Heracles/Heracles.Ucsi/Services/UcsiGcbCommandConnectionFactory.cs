using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Infra.GryphonBoard.Comm;

namespace Heracles.Ucsi.Services
{
    /// <summary>
    /// Minimal UDP connection factory for UCSI standalone calibration commands.
    /// Creates direct UDP connections to the bench firmware without complex infrastructure.
    /// </summary>
    public class UcsiGcbCommandConnectionFactory : IGcbCommandConnectionFactory
    {
        // Bench hardcoded address - these match the telemetry that's already working
        private const string GCB_BENCH_IP = "172.31.1.100";
        private const int GCB_COMMANDS_PORT = 40020;

        public IAsyncClientConnection GetGcbCommandConnection()
        {
            // Create a simple UDP connection to the bench firmware
            // Let OS assign an ephemeral client port (0) to avoid port conflicts with telemetry listener
            return new UdpClientConnection(
                GCB_BENCH_IP,
                GCB_COMMANDS_PORT,
                clientPort: 0);
        }
    }
}
