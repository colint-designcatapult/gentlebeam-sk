using Moq;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.GryphonBoard.Comm.Udp;

namespace Xcc.Test.Xcc.Infra.GryphonBoard.Comm
{
    internal class DummyGcbCommunicationServiceTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => { var sut = new DummyGcbCommunicationService(); });
        }
        
        [Test]
        public void Start_DoesNotThrow()
        {
            var sut = new DummyGcbCommunicationService();
            Assert.DoesNotThrow(() => sut.Start());
        }

        [Test]
        public void Stop_DoesNotThrow()
        {
            var sut = new DummyGcbCommunicationService();
            Assert.DoesNotThrow(() => sut.Stop());
        }

        [Test]
        public void Dispose_DoesNotThrow()
        {
            var sut = new DummyGcbCommunicationService();
            Assert.DoesNotThrow(() => sut.Dispose());
        }

        [Test]
        public void SendMessageAsync_DoesNotThrow()
        {
            var sut = new DummyGcbCommunicationService();
            byte[] buffer = { 1, 2, 3 };

            Assert.DoesNotThrowAsync(async () => await sut.SendMessageAsync(buffer));
        }

        [Test]
        public async Task SendRequestAsync_NoTimeout_ReturnsSameBuffer()
        {
            var sut = new DummyGcbCommunicationService();
            
            byte[] buffer = { 1, 2, 3 };
            var result = await sut.SendRequestAsync(buffer);

            Assert.That(result, Is.EqualTo(buffer));
        }

        [Test]
        public async Task SendRequestAsync_WithTimeout_ReturnsSameBuffer()
        {
            var sut = new DummyGcbCommunicationService();
            
            byte[] buffer = { 1, 2, 3 };
            var result = await sut.SendRequestAsync(buffer, timeoutMs: 1000);

            Assert.That(result, Is.EqualTo(buffer));
        }
    }
}
