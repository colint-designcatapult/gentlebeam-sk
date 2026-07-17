using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;
using Xcc.Infra.QualityCheck.Comm.Udp;

namespace Xcc.Infra.QualityCheck.Comm
{
    public class QcbCommunicationService : RawUdpClient, IQcbCommunicationService
    {
        private static readonly int RECEIVE_TIMEOUT = 5000;

        public QcbCommunicationService(
            IAppGlobals appGlobals,
            IQcbCommandConnection connection)
            : base(
                  appGlobals.AppCancellationTokenSource.Token,
                  connection,
                  receiveTimeout: RECEIVE_TIMEOUT)
        {
        }
    }
}
