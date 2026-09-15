using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.External.Models;
using Heracles.Core.Models;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.Outdoor.Test.ViewModels;

internal sealed class SingleEmissionWorkflowHarness : Heracles.External.ViewModels.OperatePlanViewModelBase
{
    private readonly bool[] completed;

    public SingleEmissionWorkflowHarness(
        IMainBoardModel mainBoardModel,
        IGcbIndicators indicators,
        int emissionCount)
        : base(
            Mock.Of<IRegionManager>(),
            CreateEventAggregator(),
            Mock.Of<IHeraclesExternalSettings>(),
            Mock.Of<IGCBDataStore>(),
            Mock.Of<IUIStateMachine>(),
            Mock.Of<ILogWriter>(),
            Mock.Of<IWarmupService>(),
            Mock.Of<IPopUpService>(),
            Mock.Of<IDialogService>(),
            mainBoardModel,
            indicators,
            Mock.Of<ICollimatorModel>(),
            Mock.Of<ICollimatorConfigurationStore>(),
            Mock.Of<ISafetyCheckModel>(),
            Mock.Of<IBearerTokenUserSessionManager>())
    {
        completed = new bool[emissionCount];
    }

    private static IEventAggregator CreateEventAggregator()
    {
        var events = new EventAggregator();
        events.GetEvent<SystemTelemetryChangedEvent>().SynchronizationContext =
            SynchronizationContext.Current ?? new SynchronizationContext();
        return events;
    }

    public Task<bool> PrepareNext(int startIndex) =>
        PrepareNextEmissionAsync(startIndex);
    public Task<bool> PrepareExact(int emissionIndex) =>
        PrepareEmissionAsync(emissionIndex);


    public Task ReleaseActive() => MainBoardModel.BeamOn();

    public void MarkActiveCompleted() => completed[ActiveEmissionIndex] = true;

    public bool AllCompleted => completed.All(value => value);

    protected override GcbOperationalPoint BuildGcbOperationalPoint(int fieldIndex) =>
        new()
        {
            TotalPointTime = 10,
            RemainingPointTime = 10,
            SetpointKv = 40 + fieldIndex,
        };

    protected override int FindNextPendingEmissionIndex(int startIndex)
    {
        for (int index = Math.Max(0, startIndex); index < completed.Length; index++)
        {
            if (!completed[index])
            {
                return index;
            }
        }

        return -1;
    }

    protected override Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry) =>
        Task.CompletedTask;

    protected override void CheckForApplicatorCompatibility()
    {
    }

    protected override Task OnBeamOnClicked() => Task.CompletedTask;

    protected override void RecalculateInitialXrayTime()
    {
    }

    protected override Task SetPlanUnloadTaskAsync() => Task.CompletedTask;
}

internal class OperatePlanEmissionPreparationTests
{
    [Test]
    public async Task SequentialPreparation_RequiresDistinctBeamOnActions()
    {
        var board = new Mock<IMainBoardModel>();
        board.Setup(model => model.PrepareEmission(
                It.IsAny<GcbOperationalPoint>(), false))
            .ReturnsAsync(true);
        board.Setup(model => model.BeamOn()).Returns(Task.CompletedTask);
        var indicators = new Mock<IGcbIndicators>();
        indicators.SetupGet(value => value.BeamOnProgress)
            .Returns(new BeamOnProgress(board.Object));
        var workflow = new SingleEmissionWorkflowHarness(
            board.Object, indicators.Object, emissionCount: 2);

        await workflow.PrepareNext(0);

        Assert.That(workflow.ActiveEmissionIndex, Is.Zero);
        board.Verify(model => model.PrepareEmission(
            It.Is<GcbOperationalPoint>(point => point.SetpointKv == 40), false), Times.Once);
        board.Verify(model => model.BeamOn(), Times.Never);

        await workflow.ReleaseActive();
        workflow.MarkActiveCompleted();
        await workflow.PrepareNext(1);

        Assert.That(workflow.ActiveEmissionIndex, Is.EqualTo(1));
        board.Verify(model => model.BeamOn(), Times.Once);
        board.Verify(model => model.PrepareEmission(
            It.Is<GcbOperationalPoint>(point => point.SetpointKv == 41), false), Times.Once);

        await workflow.ReleaseActive();
        workflow.MarkActiveCompleted();

        Assert.That(workflow.AllCompleted, Is.True);
        board.Verify(model => model.BeamOn(), Times.Exactly(2));
    }

    [Test]
    public async Task ExactPreparation_StagesRequestedIndexWithoutScanning()
    {
        var board = new Mock<IMainBoardModel>();
        board.Setup(model => model.PrepareEmission(
                It.IsAny<GcbOperationalPoint>(), false))
            .ReturnsAsync(true);
        var indicators = new Mock<IGcbIndicators>();
        indicators.SetupGet(value => value.BeamOnProgress)
            .Returns(new BeamOnProgress(board.Object));
        var workflow = new SingleEmissionWorkflowHarness(
            board.Object, indicators.Object, emissionCount: 2);

        await workflow.PrepareExact(1);

        Assert.That(workflow.ActiveEmissionIndex, Is.EqualTo(1));
        board.Verify(model => model.PrepareEmission(
            It.Is<GcbOperationalPoint>(point => point.SetpointKv == 41), false), Times.Once);
        board.Verify(model => model.PrepareEmission(
            It.Is<GcbOperationalPoint>(point => point.SetpointKv == 40), false), Times.Never);
    }
}
