using Moq;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.GryphonBoard.Comm.Udp;

namespace Xcc.Test.Xcc.Infra.GryphonBoard.Comm
{
    internal class GcbCommunicationServiceTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            var mockGlobals = new Mock<IAppGlobals>();
            mockGlobals.Setup(g => g.AppCancellationTokenSource).Returns(new CancellationTokenSource());

            var mockConnection = new Mock<IGcbCommandConnection>();

            Assert.DoesNotThrow(() =>
            {
                var sut = new GcbCommunicationService(mockGlobals.Object, mockConnection.Object);
            });
        }
    }
}
