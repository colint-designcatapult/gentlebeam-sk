using Moq;
using NUnit.Framework;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.Outdoor.Test.ViewModels;

internal class PhysicsSingleEmissionTests
{
    [Test]
    public async Task NextPreparationFailure_DoesNotReleaseNextEmission()
    {
        var preparationAttempt = 0;
        var board = new Mock<IMainBoardModel>();
        board.Setup(model => model.PrepareEmission(
                It.IsAny<GcbOperationalPoint>(), false))
            .Returns(() =>
            {
                preparationAttempt++;
                return preparationAttempt == 2
                    ? Task.FromException<bool>(new InvalidOperationException("prepare failed"))
                    : Task.FromResult(true);
            });
        board.Setup(model => model.BeamOn()).Returns(Task.CompletedTask);
        var indicators = new Mock<IGcbIndicators>();
        indicators.SetupGet(value => value.BeamOnProgress)
            .Returns(new BeamOnProgress(board.Object));
        var workflow = new SingleEmissionWorkflowHarness(
            board.Object, indicators.Object, emissionCount: 2);

        await workflow.PrepareNext(0);
        await workflow.ReleaseActive();
        workflow.MarkActiveCompleted();

        Assert.That(
            async () => await workflow.PrepareNext(1),
            Throws.TypeOf<InvalidOperationException>()
                .With.Message.EqualTo("prepare failed"));
        Assert.Multiple(() =>
        {
            Assert.That(workflow.ActiveEmissionIndex, Is.EqualTo(1));
            Assert.That(workflow.AllCompleted, Is.False);
        });
        board.Verify(model => model.BeamOn(), Times.Once);

        await workflow.PrepareNext(1);

        board.Verify(model => model.BeamOn(), Times.Once);
        Assert.That(workflow.ActiveEmissionIndex, Is.EqualTo(1));
    }
}
