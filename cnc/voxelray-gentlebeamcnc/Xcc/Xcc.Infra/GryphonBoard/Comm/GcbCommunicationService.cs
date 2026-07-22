using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;

namespace Xcc.Infra.GryphonBoard.Comm
{
    public class GcbCommunicationService : RawUdpClient, IGcbCommunicationService
    {
        public GcbCommunicationService(
            IAppGlobals appGlobals,
            IGcbCommandConnectionFactory connectionFactory)
            : base(
                appGlobals.AppCancellationTokenSource.Token,
                connectionFactory.GetGcbCommandConnection(),
                receiveTimeout: 500)
        {
        }
    }
}
