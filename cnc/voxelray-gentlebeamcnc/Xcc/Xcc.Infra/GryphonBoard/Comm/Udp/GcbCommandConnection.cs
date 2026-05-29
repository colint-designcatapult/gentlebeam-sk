using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;

namespace Xcc.Infra.GryphonBoard.Comm.Udp
{
    public interface IGcbCommandConnection : IUdpClientConnection
    {
    }

    public class GcbCommandConnection : UdpClientConnection, IGcbCommandConnection
    {
        public GcbCommandConnection(ICoreSettings coreSettings)
            : base(coreSettings.GCBCommandsEndPoint.Ip(),
                  coreSettings.GCBCommandsEndPoint.Port!.Value,
                  clientPort: coreSettings.GCBCommandsEndPoint.Port!.Value, // now it's always the same as the remote one
                  reusePort: true)
        {
        }
    }
}
