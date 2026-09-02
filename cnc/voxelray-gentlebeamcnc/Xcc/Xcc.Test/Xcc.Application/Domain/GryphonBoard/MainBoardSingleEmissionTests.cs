using Moq;
using Prism.Events;
using Xcc.Application.Domain.GryphonBoard;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard;
using Xcc.Core.Models;

namespace Xcc.Test.Xcc.Application.Domain.GryphonBoard;

internal class MainBoardSingleEmissionTests
{
    [Test]
    public async Task PrepareAndBeamOn_SendsOneScalarEmissionAndOnePointRelease()
    {
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        var dataStore = new Mock<IGCBDataStore>();
        var logWriter = new Mock<ILogWriter>();
        var model = new MainBoardModelBase(
            dataStore.Object,
            logWriter.Object,
            commands.Object,
            new EventAggregator());
        var emission = new GcbOperationalPoint
        {
            TotalPointTime = 10,
            RemainingPointTime = 10,
            SetpointKv = 50,
            TargetMA = 0.5f,
            FilamentSetpoint = 3000,
            XCoilSetpoint = 1,
            YCoilSetpoint = 2,
            FocusCoilSetpoint = 3,
        };

        void SetState(GcbStateNew state) => model.OnSystemTelemetryChanged(
            new SystemNormalTelemetry { ControlBoardState = state });

        commands.Setup(command => command.NewSession())
            .Callback(() => SetState(GcbStateNew.Staging))
            .ReturnsAsync(new GcbSession(11));
        commands.Setup(command => command.SendOperationalPoint(
                OperationalPointCmdType.Load,
                It.Is<GcbOperationalPoint>(point => point.Equals(emission)),
                new GcbSession(11)))
            .Returns(Task.CompletedTask);
        commands.Setup(command => command.StagePlan())
            .Callback(() => SetState(GcbStateNew.Staged))
            .Returns(Task.CompletedTask);
        commands.Setup(command => command.SendOperationalPoint(
                OperationalPointCmdType.Confirmation,
                It.Is<GcbOperationalPoint>(point => point.Equals(emission)),
                new GcbSession(11)))
            .Returns(Task.CompletedTask);
        commands.Setup(command => command.ReleasePlan(
                GCBReleaseCommandScope.Plan,
                new GcbSession(11)))
            .Callback(() => SetState(GcbStateNew.Ready))
            .Returns(Task.CompletedTask);
        commands.Setup(command => command.ReleasePlan(
                GCBReleaseCommandScope.Point,
                new GcbSession(11)))
            .Callback(() => SetState(GcbStateNew.Cold))
            .Returns(Task.CompletedTask);

        SetState(GcbStateNew.Primed);

        Assert.That(await model.PrepareEmission(emission, false), Is.True);
        Assert.That(model.State, Is.EqualTo(GcbStateNew.Ready));

        await model.BeamOn();

        Assert.Multiple(() =>
        {
            commands.Verify(command => command.NewSession(), Times.Once);
            commands.Verify(command => command.StagePlan(), Times.Once);
            commands.Verify(command => command.SendOperationalPoint(
                OperationalPointCmdType.Load,
                It.IsAny<GcbOperationalPoint>(),
                It.IsAny<GcbSession>()), Times.Once);
            commands.Verify(command => command.SendOperationalPoint(
                OperationalPointCmdType.Confirmation,
                It.IsAny<GcbOperationalPoint>(),
                It.IsAny<GcbSession>()), Times.Once);
            commands.Verify(command => command.ReleasePlan(
                GCBReleaseCommandScope.Plan,
                new GcbSession(11)), Times.Once);
            commands.Verify(command => command.ReleasePlan(
                GCBReleaseCommandScope.Point,
                new GcbSession(11)), Times.Once);
            Assert.That(model.State, Is.EqualTo(GcbStateNew.Cold));
        });
        commands.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ResumeEmission_ReconfirmsAndReleasesSameScalarSession()
    {
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        var model = new MainBoardModelBase(
            Mock.Of<IGCBDataStore>(),
            Mock.Of<ILogWriter>(),
            commands.Object,
            new EventAggregator());
        var requested = new GcbOperationalPoint
        {
            TotalPointTime = 10,
            RemainingPointTime = 10,
            SetpointKv = 50,
            TargetMA = 0.5f,
            FilamentSetpoint = 3000,
            FocusCoilSetpoint = 3,
        };
        var interrupted = requested;
        interrupted.RemainingPointTime = 4;

        void SetState(GcbStateNew state) => model.OnSystemTelemetryChanged(
            new SystemNormalTelemetry { ControlBoardState = state });

        commands.Setup(command => command.QueryPoint()).ReturnsAsync(interrupted);
        commands.Setup(command => command.SendOperationalPoint(
                OperationalPointCmdType.Confirmation,
                It.Is<GcbOperationalPoint>(point => point.Equals(interrupted)),
                new GcbSession(11)))
            .Returns(Task.CompletedTask);
        commands.Setup(command => command.ReleasePlan(
                GCBReleaseCommandScope.Plan,
                new GcbSession(11)))
            .Callback(() => SetState(GcbStateNew.Ready))
            .Returns(Task.CompletedTask);
        commands.Setup(command => command.ReleasePlan(
                GCBReleaseCommandScope.Point,
                new GcbSession(11)))
            .Callback(() => SetState(GcbStateNew.Cold))
            .Returns(Task.CompletedTask);

        model.SetCurrentEmission(requested);
        model.SetSession(new GcbSession(11));
        SetState(GcbStateNew.Staged);

        await model.ResumeEmission();
        await model.BeamOn();

        Assert.Multiple(() =>
        {
            Assert.That(model.CurrentEmission?.RemainingPointTime, Is.EqualTo(4));
            Assert.That(model.Session, Is.EqualTo(new GcbSession(11)));
            Assert.That(model.State, Is.EqualTo(GcbStateNew.Cold));
        });
        commands.Verify(command => command.NewSession(), Times.Never);
        commands.Verify(command => command.SendOperationalPoint(
            OperationalPointCmdType.Load,
            It.IsAny<GcbOperationalPoint>(),
            It.IsAny<GcbSession>()), Times.Never);
        commands.Verify(command => command.QueryPoint(), Times.Once);
        commands.Verify(command => command.SendOperationalPoint(
            OperationalPointCmdType.Confirmation,
            It.IsAny<GcbOperationalPoint>(),
            new GcbSession(11)), Times.Once);
        commands.Verify(command => command.ReleasePlan(
            GCBReleaseCommandScope.Plan,
            new GcbSession(11)), Times.Once);
        commands.Verify(command => command.ReleasePlan(
            GCBReleaseCommandScope.Point,
            new GcbSession(11)), Times.Once);
        commands.VerifyNoOtherCalls();
    }
}
