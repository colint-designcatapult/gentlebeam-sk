using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;

namespace Heracles.Ucsi.Services;

/// <summary>
/// UCSI-specific GCB communication service.
/// </summary>
public class UcsiGcbCommunicationService : GcbCommunicationService
{
    public UcsiGcbCommunicationService(
        IAppGlobals appGlobals,
        IGcbCommandConnectionFactory connectionFactory)
        : base(appGlobals, connectionFactory)
    {
    }
}
