using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
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
            var expected = new QcReadings(12u, 23u, 34u, 45u);
            _commands.Setup(command => command.StopQcbReadings())
                .ReturnsAsync(expected);
            var service = new GcbQcbService(_commands.Object);

            QcbCommandResponseStatus status = await service.StartQCReadingsAsync();
            QcReadings readings = await service.StopQCReadingsAsync();

            Assert.Multiple(() =>
            {
                Assert.That(status, Is.EqualTo(QcbCommandResponseStatus.StartConfirmed));
                Assert.That(readings, Is.SameAs(expected));
            });
            _commands.Verify(command => command.StartQcbReadings(), Times.Once);
            _commands.Verify(command => command.StopQcbReadings(), Times.Once);
        }
    }
}
