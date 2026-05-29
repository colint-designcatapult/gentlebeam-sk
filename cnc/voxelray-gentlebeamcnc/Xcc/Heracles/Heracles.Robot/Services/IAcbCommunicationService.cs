using System;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Networking;
using Xcc.Infra.Services.GcbServices;

namespace Heracles.Robot.Services
{
    public interface IAcbCommConnectionFactory
    {
        IAsyncClientConnection GetAcbCommConnection();
    }

    public interface IAcbCommunicationService : IUdpClientRaw
    {
        [Obsolete]
        void StopListening(); // Use Stop() instead
        Task<bool> PingAsync();
    }
}
