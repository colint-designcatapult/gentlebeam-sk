using Moq;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Commands;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models.RDBMS;

namespace Xcc.Test.Xcc.Application.AppLayer.Warmup
{
    internal class WarmUpServiceTests
    {
        private Mock<IWarmupHistory> warmupHistory;
        private Mock<IMainBoardModel> mainBoardModel;
        private Mock<IWarmupCommands> warmupCommands;

        class WarmupService(IWarmupCommands warmupCommands, IMainBoardAPI mainBoardAPI, IWarmupHistory warmupHistory) : AbstractWarmupService(warmupCommands, mainBoardAPI, warmupHistory)
        {
            protected override long GetActiveHeadId()
            {
                return 1;
            }
        }

        [SetUp]
        public void SetUp()
        {
            warmupHistory = new Mock<IWarmupHistory>();
            mainBoardModel = new Mock<IMainBoardModel>();
            warmupCommands = new Mock<IWarmupCommands>();
            mainBoardModel.Setup(mb => mb.SafeWarmup(It.IsAny<WarmupParameters>())).Returns(Task.FromResult(true));
        }

        private WarmupService GetWarmUpService()
        {
            return new WarmupService(warmupCommands.Object, mainBoardModel.Object, warmupHistory.Object);
        }

        [Test]
        public void WarmUpAsync_NoHead_Test()
        {
            var service = GetWarmUpService();
            var warmupParameters = WarmupParameters.FastWarmup(3500.0f);

            Assert.DoesNotThrowAsync(() => service.RunSafeWarmupAsync(warmupParameters));
            Assert.Multiple(() =>
            {

                mainBoardModel.Verify(wm => wm.SafeWarmup(It.IsAny<WarmupParameters>()), Times.Once);
                // Must not register the warmup event:
                warmupCommands.Verify(c => c.CreateAsync(It.IsAny<IWarmUp>()), Times.Never);
                warmupHistory.Verify(wm => wm.OnNewWarmupEvent(It.IsAny<IWarmUp>()), Times.Never);
            });
        }

        [Test]
        public void WarmUpAsync_ActiveHead_Test()
        {
            var service = GetWarmUpService();
            var warmupParameters = WarmupParameters.Conditioning(3500.0f, activeHeadId: 1);

            Assert.DoesNotThrowAsync(() => service.RunSafeWarmupAsync(warmupParameters));
            Assert.Multiple(() =>
            {
                mainBoardModel.Verify(wm => wm.SafeWarmup(It.IsAny<WarmupParameters>()), Times.Once);
                // Must register the warmup event:
                warmupCommands.Verify(c => c.CreateAsync(It.IsAny<IWarmUp>()), Times.Once);
                warmupHistory.Verify(wm => wm.OnNewWarmupEvent(It.IsAny<IWarmUp>()), Times.Once);
            });
        }
    }
}
