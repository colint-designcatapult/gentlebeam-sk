using Empyrean.Common.Infra.Networking;
using Moq;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;

namespace Xcc.Test.Xcc.Infra.GryphonBoard.Comm
{
    internal class GcbCommunicationServiceTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            var mockGlobals = new Mock<IAppGlobals>();
            mockGlobals.Setup(g => g.AppCancellationTokenSource).Returns(new CancellationTokenSource());
            var mockConnectionFactory = new Mock<IGcbCommandConnectionFactory>();
            mockConnectionFactory
                .Setup(factory => factory.GetGcbCommandConnection())
                .Returns(new Mock<IAsyncClientConnection>().Object);

            Assert.DoesNotThrow(() =>
            {
                var sut = new GcbCommunicationService(mockGlobals.Object, mockConnectionFactory.Object);
            });
        }
    }
}
