using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Infra.GryphonBoard.Comm;

namespace Heracles.Ucsi.Services
{
    /// <summary>
    /// Minimal UDP connection factory for UCSI standalone calibration commands.
    /// Creates direct UDP connections to the bench firmware without complex infrastructure.
    /// Uses configuration from appsettings for IP and port (allows customization for different test benches).
    /// </summary>
    public class UcsiGcbCommandConnectionFactory : IGcbCommandConnectionFactory
    {
        private readonly UcsiStandaloneCommandOptions _options;

        public UcsiGcbCommandConnectionFactory(UcsiStandaloneCommandOptions options)
        {
            _options = options;
        }

        public IAsyncClientConnection GetGcbCommandConnection()
        {
            // Create a simple UDP connection to the bench firmware
            // Let OS assign an ephemeral client port (0) to avoid port conflicts with telemetry listener
            return new UdpClientConnection(
                _options.RemoteAddress,
                _options.RemotePort,
                clientPort: 0);
        }
    }
}
