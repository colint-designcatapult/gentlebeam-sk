using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;

namespace Heracles.Ucsi.Services;

/// <summary>
/// UCSI-specific GCB communication service that uses independent cancellation
/// instead of app-level cancellation token. This ensures the UDP receive loop
/// continues to run even during app shutdown sequences.
/// </summary>
public class UcsiGcbCommunicationService : GcbCommunicationService
{
    public UcsiGcbCommunicationService(IGcbCommandConnectionFactory connectionFactory)
        : base(
            new UcsiAppGlobalsStub(),
            connectionFactory)
    {
    }

    /// <summary>
    /// Stub that provides CancellationToken.None instead of an app-level token.
    /// This allows the receive task to run independently from app shutdown.
    /// </summary>
    private class UcsiAppGlobalsStub : IAppGlobals
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        
        public CancellationTokenSource AppCancellationTokenSource 
        { 
            get => _cts;
            set => _cts = value;
        }
    }
}
