using System;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Events;

namespace Xcc.Infra.GryphonBoard.Comm
{
    public class DummyGcbCommunicationService : IGcbCommunicationService
    {
#pragma warning disable CS0067
        public event EventHandler<UdpReceiveErrorEventArgs>? UdpReceiveErrorEvent;
#pragma warning restore CS0067

        public void Dispose()
        {
        }
        public Task<byte[]> SendRequestAsync(byte[] buffer)
        {
            return SendRequestAsync(buffer, 0);
        }

        public Task<byte[]> SendRequestAsync(byte[] buffer, int timeoutMs)
        {
            return Task.FromResult(buffer);
        }

        public Task SendMessageAsync(byte[] buffer)
        {
            return Task.CompletedTask;
        }


        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
