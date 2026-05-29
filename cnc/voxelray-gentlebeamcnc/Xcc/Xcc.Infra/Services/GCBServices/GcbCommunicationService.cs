using Empyrean.Common.Infra.Networking;
using System;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;

namespace Xcc.Infra.Services.GcbServices
{
    [Obsolete]
    public interface IGcbCommunicationConnectionFactory
    {
        IAsyncClientConnection CreateGcbCommandConnection();
    }
    [Obsolete]
    public class GcbCommunicationService : GcbBaseUdpService, IGcbCommunicationService
    {
        public GcbCommunicationService(
            IAppGlobals appGlobals,
            IGcbCommunicationConnectionFactory connectionFactory)
            : base(appGlobals.AppCancellationTokenSource.Token, connectionFactory.CreateGcbCommandConnection(),  receiveTimeout: 500)
        {
        }
    }
}
