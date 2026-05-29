using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;

namespace Xcc.Infra.GryphonBoard.Comm.Udp
{
    public interface IGcbTelemetryConnection : IUdpClientConnection
    {
    }

    public class GcbTelemetryConnection : UdpClientConnection, IGcbTelemetryConnection
    {
        public GcbTelemetryConnection(ICoreSettings coreSettings)
            : base(coreSettings.GCBTelemetryEndPoint.Ip(),
                  coreSettings.GCBTelemetryEndPoint.Port!.Value,
                  clientPort: coreSettings.GCBTelemetryEndPoint.Port.Value, // now it's always the same as the remote one
                  reusePort: true)
        {
        }
    }
}
