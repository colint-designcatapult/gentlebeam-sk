using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;
using System;

namespace Xcc.Infra.QualityCheck.Comm.Udp
{
    [Obsolete("QCB communication is obsolete - replaced by collimator I2C telemetry")]
    public interface IQcbCommandConnection : IUdpClientConnection
    {
    }

    [Obsolete("QCB communication is obsolete - replaced by collimator I2C telemetry")]
    public class QcbCommandConnection : UdpClientConnection, IQcbCommandConnection
    {
        [Obsolete("QcbCommandsEndPoint property removed")]
        public QcbCommandConnection(ICoreSettings coreSettings)
            : base("172.31.1.231",  // hardcoded placeholder
                  7000,  // hardcoded placeholder
                  clientPort: QcbClientPort,
                  reusePort: true)
        {
        }

        public static int QcbClientPort => 58001;
    }
}
