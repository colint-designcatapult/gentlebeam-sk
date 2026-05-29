using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Warmup;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Models.RDBMS;
using Moq;
using NUnit.Framework;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Commands;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Services;

namespace Heracles.Outdoor.Test.Services
{
    internal class WarmUpServiceTests
    {
        private Mock<IMainBoardAPI> mainBoardApiMock;
        private Mock<IWarmupCommands> warmupCommandsMock;
        private Mock<IWarmupHistory> warmupHistoryMock;
        private Mock<IGcbIndicators> gcbIndicators;
        private Mock<ICollimatorModel> collimatorModelMock;

        [SetUp]
        public void SetUp()
        {
            mainBoardApiMock = new Mock<IMainBoardAPI>();
            warmupCommandsMock = new Mock<IWarmupCommands>();
            warmupHistoryMock = new Mock<IWarmupHistory>();
            collimatorModelMock = new Mock<ICollimatorModel>();
            collimatorModelMock.SetupGet(c => c.ActiveHead).Returns(new Head { Id = 1 });

            gcbIndicators = new Mock<IGcbIndicators>();
            gcbIndicators.SetupGet(x => x.WarmUpProgress).Returns((WarmUpProgress)null!);
        }

        private IWarmupService GetWarmUpService()
        {
            return new WarmupService(warmupCommandsMock.Object, mainBoardApiMock.Object, warmupHistoryMock.Object, collimatorModelMock.Object);
        }

        [Test]
        public void WarmUpAsync_NoHead_Test()
        {
            var service = GetWarmUpService();
            var warmupParameters = WarmupParameters.FastWarmup(3500.0f);

            Assert.DoesNotThrowAsync(() => service.RunSafeWarmupAsync(warmupParameters));

            //gcbIndicators.Verify(g => g.WarmUpProgress, Times.Once);
            mainBoardApiMock.Verify(wm => wm.SafeWarmup(It.IsAny<WarmupParameters>()), Times.Once);

            //// Must escalate through popup service:
            //popUpService.Verify(ps => ps.LogAndShowMessage(
            //    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ReportType>(),
            //    It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()), Times.Once);
        }

        [Test]
        public void WarmUpAsync_ActiveHead_Test()
        {
            var service = GetWarmUpService();
            IHead head = new Head{ Id = 1 };
            var warmupParameters = WarmupParameters.Conditioning(3500.0f, head.Id);
            
            Assert.DoesNotThrowAsync(() => service.RunSafeWarmupAsync(warmupParameters));
            //gcbIndicators.Verify(g => g.WarmUpProgress, Times.Once);
            mainBoardApiMock.Verify(wm => wm.SafeWarmup(It.IsAny<WarmupParameters>()), Times.Once);
            // Must register the warmup event:
            //warmupHistoryMock.Verify(wm => wm.OnNewWarmupEvent(It.IsAny<IWarmUp>()), Times.Exactly(2));
        }
    }
}
