using System;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;

namespace Heracles.Robot.Services
{
    public interface IAcbCommConnectionFactory
    {
        IAsyncClientConnection GetAcbCommConnection();
    }

    public interface IAcbCommunicationService : IRawUdpClient
    {
        [Obsolete]
        void StopListening(); // Use Stop() instead
        Task<bool> PingAsync();
    }
}
