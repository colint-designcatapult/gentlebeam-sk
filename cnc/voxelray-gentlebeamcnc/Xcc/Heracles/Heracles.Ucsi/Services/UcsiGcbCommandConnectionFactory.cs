using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
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
        private readonly ILogWriter _logWriter;

        public UcsiGcbCommandConnectionFactory(UcsiStandaloneCommandOptions options, ILogWriter logWriter)
        {
            _options = options;
            _logWriter = logWriter;
        }

        public IAsyncClientConnection GetGcbCommandConnection()
        {
            // Create a simple UDP connection to the bench firmware
            // Bind to port 20 (same as firmware) so responses come back to us
            // This matches the third-party calibration software behavior
            _ = _logWriter.LogAsync($"[UDP_INIT] Creating connection to {_options.RemoteAddress}:{_options.RemotePort} (local port: 20)", LogRecordSeverity.Info, LogRecordType.System);
            return new UdpClientConnection(
                _options.RemoteAddress,
                _options.RemotePort,
                clientPort: 20);
        }
    }
}
