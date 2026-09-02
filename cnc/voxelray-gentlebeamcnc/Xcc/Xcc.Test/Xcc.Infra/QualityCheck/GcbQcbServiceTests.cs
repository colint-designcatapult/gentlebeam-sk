using System;
using System.Threading.Tasks;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Infra.QualityCheck;

namespace Xcc.Test.Xcc.Infra.QualityCheck
{
    internal class GcbQcbServiceTests
    {
        private readonly Mock<IGcbCommandInterface> _commands = new();

        [Test]
        public async Task StartAndStop_UseMainControlQcCommands()
        {
            float[] expected = [12.5f, 23.5f, 0, 0, 0];
            _commands.Setup(command => command.StopQcbReadings())
                .ReturnsAsync(new QcReadings(expected));
            var service = new GcbQcbService(_commands.Object);

            QcbCommandResponseStatus status = await service.StartQCReadingsAsync(5, 50);
            QcReadings? readings = await service.StopQCReadingsAsync(5);

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(QcbCommandResponseStatus.StartConfirmed));
                Assert.That(readings?.Data, Is.EqualTo(expected));
            });
            _commands.Verify(command => command.StartQcbReadings(50), Times.Once);
            _commands.Verify(command => command.StopQcbReadings(), Times.Once);
        }

        [Test]
        public void UnsupportedDiodeCount_IsRejected()
        {
            var service = new GcbQcbService(_commands.Object);

            Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                () => service.StartQCReadingsAsync(2, 50));
        }
    }
}
