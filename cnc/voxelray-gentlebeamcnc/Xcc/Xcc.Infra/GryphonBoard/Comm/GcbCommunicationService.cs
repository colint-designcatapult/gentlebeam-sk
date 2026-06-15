using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm.Udp;
using Xcc.Infra.Services.GcbServices;

namespace Xcc.Infra.GryphonBoard.Comm
{
    public class GcbCommunicationService : GcbBaseUdpService, IGcbCommunicationService
    {
        public GcbCommunicationService(
            IAppGlobals appGlobals,
            IGcbCommandConnection connection)
            : base(appGlobals.AppCancellationTokenSource.Token,
                  connection, receiveTimeout: 500)
        {
        }
    }
}
