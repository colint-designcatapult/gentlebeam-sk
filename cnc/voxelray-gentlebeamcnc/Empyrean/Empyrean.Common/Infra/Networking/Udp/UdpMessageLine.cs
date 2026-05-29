using System.Diagnostics;

namespace Empyrean.Common.Infra.Networking.Udp
{
    public class UdpMessageLine
    {
        private readonly object _dataLock = new object();
        private readonly Dictionary<int, byte[]?> _messageLine = new();

        public void AddRequest(int messageId)
        {
            lock (_dataLock)
            {
                // add empty value
                _messageLine[messageId] = null;
            }
        }

        public void AddResponse(int messageId, byte[] response)
        {
            lock (_dataLock)
            {
                if (_messageLine.ContainsKey(messageId))
                {
                    _messageLine[messageId] = response;
                }
            }
        }

        public async Task<byte[]?> WaitForResponseAsync(int messageId, int timeout, CancellationToken cancellationToken)
        {
            byte[]? response = null;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (stopwatch.ElapsedMilliseconds < timeout && !cancellationToken.IsCancellationRequested)
            {
                if (_messageLine.TryGetValue(messageId, out response) && response != null)
                {
                    break;
                }
                await Task.Delay(50);
            }

            return response;
        }

        public void RemoveRequest(int messageId)
        {
            lock (_dataLock)
            {
                _messageLine.Remove(messageId);
            }
        }
    }
}
