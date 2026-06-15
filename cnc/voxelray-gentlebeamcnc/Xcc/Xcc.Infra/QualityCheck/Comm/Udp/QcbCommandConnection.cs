using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;

namespace Xcc.Infra.QualityCheck.Comm.Udp
{
    public interface IQcbCommandConnection : IUdpClientConnection
    {
    }

    public class QcbCommandConnection : UdpClientConnection, IQcbCommandConnection
    {
        public QcbCommandConnection(ICoreSettings coreSettings)
            : base(coreSettings.QcbCommandsEndPoint.Ip(),
                  coreSettings.QcbCommandsEndPoint.Port!.Value,
                  clientPort: QcbClientPort,
                  reusePort: true)
        {
        }

        public static int QcbClientPort => 58001;
    }
}
