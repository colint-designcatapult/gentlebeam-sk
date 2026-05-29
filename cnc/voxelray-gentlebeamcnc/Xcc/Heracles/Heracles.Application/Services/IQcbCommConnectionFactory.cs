using Empyrean.Common.Infra.Networking;

namespace Heracles.Application.Services
{
    [System.Obsolete]
    public interface IQcbCommConnectionFactory
    {
        [System.Obsolete]
        IAsyncClientConnection GetQcbCommConnection();
    }
}
