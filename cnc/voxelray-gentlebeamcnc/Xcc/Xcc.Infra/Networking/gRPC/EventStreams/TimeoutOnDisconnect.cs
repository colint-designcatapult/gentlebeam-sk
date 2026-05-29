using System.Threading;
using System.Threading.Tasks;

namespace Xcc.Infra.Networking.gRPC.EventStreams
{
    public class TimeoutOnDisconnect : IConnectionLossStrategy
    {
        private bool _hasConnection = false;

        public TimeoutOnDisconnect(
            uint timeoutMs,
            CancellationToken cancellationToken)
        {
            TimeoutMs = timeoutMs;
            CancellationToken = cancellationToken;
        }

        public uint TimeoutMs { get; }
        public CancellationToken CancellationToken { get; }

        // We always can reconnect, as we just make a timeout on each disconnection after anyther try
        public bool CanConnect => true;
        public bool Disconnected => !_hasConnection;

        public async void OnDisconnect()
        {
            _hasConnection = false;
            await Task.Delay((int)TimeoutMs, CancellationToken);
        }
        public void OnConnect()
        {
            _hasConnection = true;
        }
    }
}
