using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Helpers;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class MagnetometerValuesTests
    {
        [Test]
        public void MagnetometerValues_Defaults()
        {
            var sut = new MagnetometerValues();

            Assert.That(sut.Back, Is.EqualTo(default(Vector3)).Within(G.Precision));
            Assert.That(sut.Front, Is.EqualTo(default(Vector3)).Within(G.Precision));
        }
        
        [Test]
        public void MagnetometerValues_Ctor()
        {
            var mockTelemetry = new Mock<ISystemTelemetry>();
            mockTelemetry.Setup(t => t.Mag1).Returns(new TelemetryVector3(1.1f, 2.2f, 3.3f));
            mockTelemetry.Setup(t => t.Mag2).Returns(new TelemetryVector3(4.4f, 5.5f, 6.6f));
            
            var sut = new MagnetometerValues(mockTelemetry.Object);

            Assert.That(sut.Back[0, 0], Is.EqualTo(1.1f));
            Assert.That(sut.Back[1, 0], Is.EqualTo(2.2f));
            Assert.That(sut.Back[2, 0], Is.EqualTo(3.3f));

            Assert.That(sut.Front[0, 0], Is.EqualTo(4.4f));
            Assert.That(sut.Front[1, 0], Is.EqualTo(5.5f));
            Assert.That(sut.Front[2, 0], Is.EqualTo(6.6f));
        }
    }
}