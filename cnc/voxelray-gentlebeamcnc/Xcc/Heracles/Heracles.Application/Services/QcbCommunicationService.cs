using System;
using Xcc.Core.Models;
using Xcc.Infra.QualityCheck.Comm;
using Xcc.Infra.Services.GcbServices;

namespace Heracles.Application.Services
{
    [Obsolete]
    public class QcbCommunicationService : GcbBaseUdpService, IQcbCommunicationService
    {
        private static readonly int RECEIVE_TIMEOUT = 5000;

        public QcbCommunicationService(
            IAppGlobals appGlobals,
            IQcbCommConnectionFactory connectionFactory)
            : base(
                  appGlobals.AppCancellationTokenSource.Token,
                  connectionFactory.GetQcbCommConnection(),
                  receiveTimeout: RECEIVE_TIMEOUT)
        {
        }
    }
}
