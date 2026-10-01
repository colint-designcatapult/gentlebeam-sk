using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.ViewModels;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;
using Xcc.Application.AppLayer.Service;

namespace Heracles.Application.Test.ViewModels;

[TestFixture]
internal sealed class UnifiedCalibrationServiceViewModelTests
{
    [TestCase(GcbStateNew.Cold)]
    [TestCase(GcbStateNew.Staged)]
    public async Task RunEmission_ExecutesSinglePointWorkflowInOrder(GcbStateNew initialState)
    {
        UcsiTelemetrySample? sample = CreateSample(initialState);
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        var order = new List<string>();
        GcbOperationalPoint? loadedPoint = null;
        GcbOperationalPoint? confirmedPoint = null;
        var session = new GcbSession(17);

        if(initialState == GcbStateNew.Staged)
        {
            commands.Setup(value => value.Stop())
                .Callback(() => { order.Add("Stop"); sample = CreateSample(GcbStateNew.Cold); })
                .Returns(Task.CompletedTask);
        }
        commands.Setup(value => value.ResetTimers())
            .Callback(() => order.Add("ResetTimers"))
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.ClearPlan())
            .Callback(() => order.Add("ClearPlan"))
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.WarmUp(2_000f))
            .Callback(() => { order.Add("Warmup"); sample = CreateSample(GcbStateNew.Primed); })
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.NewSession())
            .Callback(() => { order.Add("NewSession"); sample = CreateSample(GcbStateNew.Staging); })
            .ReturnsAsync(session);
        commands.Setup(value => value.SendOperationalPoint(
                OperationalPointCmdType.Load,
                It.IsAny<GcbOperationalPoint>(),
                session))
            .Callback<OperationalPointCmdType, GcbOperationalPoint, GcbSession>((_, point, _) =>
            {
                order.Add("Load");
                loadedPoint = point;
            })
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.StagePlan())
            .Callback(() => { order.Add("StagePlan"); sample = CreateSample(GcbStateNew.Staged); })
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.SendOperationalPoint(
                OperationalPointCmdType.Confirmation,
                It.IsAny<GcbOperationalPoint>(),
                session))
            .Callback<OperationalPointCmdType, GcbOperationalPoint, GcbSession>((_, point, _) =>
            {
                order.Add("Confirm");
                confirmedPoint = point;
            })
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.ReleasePlan(GCBReleaseCommandScope.Plan, session))
            .Callback(() => { order.Add("ReleasePlan"); sample = CreateSample(GcbStateNew.Ready); })
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.ReleasePlan(GCBReleaseCommandScope.Point, session))
            .Callback(() => { order.Add("ReleasePoint"); sample = CreateSample(GcbStateNew.Emission); })
            .Returns(Task.CompletedTask);

        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);
        SetValidEmission(sut);

        await sut.RunOrStopEmissionAsync();

        Assert.Multiple(() =>
        {
            string[] expectedOrder = initialState == GcbStateNew.Cold
                ? ["ResetTimers", "ClearPlan", "Warmup", "NewSession", "Load", "StagePlan", "Confirm", "ReleasePlan", "ReleasePoint"]
                : ["Stop", "ResetTimers", "ClearPlan", "Warmup", "NewSession", "Load", "StagePlan", "Confirm", "ReleasePlan", "ReleasePoint"];
            Assert.That(order, Is.EqualTo(expectedOrder));
            Assert.That(loadedPoint, Is.Not.Null);
            Assert.That(confirmedPoint, Is.EqualTo(loadedPoint));
            Assert.That(loadedPoint.Value.TotalPointTime, Is.EqualTo(2.5f));
            Assert.That(loadedPoint.Value.RemainingPointTime, Is.EqualTo(2.5f));
            Assert.That(loadedPoint.Value.SetpointKv, Is.EqualTo(50f));
            Assert.That(loadedPoint.Value.TargetMA, Is.EqualTo(2f));
            Assert.That(loadedPoint.Value.FilamentSetpoint, Is.EqualTo(2_000f));
            Assert.That(loadedPoint.Value.XCoilSetpoint, Is.EqualTo(250f));
            Assert.That(loadedPoint.Value.YCoilSetpoint, Is.EqualTo(-500f));
            Assert.That(loadedPoint.Value.FocusCoilSetpoint, Is.EqualTo(1_250f));
            Assert.That(sut.EmissionSequenceStatus, Does.Contain("reached control state Emission"));
        });
        commands.VerifyAll();
    }

    [Test]
    public async Task RunEmission_WhenWarmupIsRejected_ReportsEstopFailure()
    {
        UcsiTelemetrySample? sample = CreateSample(GcbStateNew.Cold);
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        commands.Setup(value => value.ResetTimers()).Returns(Task.CompletedTask);
        commands.Setup(value => value.ClearPlan()).Returns(Task.CompletedTask);
        commands.Setup(value => value.WarmUp(2_000f))
            .ThrowsAsync(new Exception("Cannot start warmup from the current state; verify both E-stops are released"));
        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);
        SetValidEmission(sut);

        await sut.RunOrStopEmissionAsync();

        Assert.That(sut.ErrorText, Does.Contain("E-stops"));
        commands.VerifyAll();
    }

    [Test]
    public async Task RunEmission_WithInvalidInput_SendsNothing()
    {
        UcsiTelemetrySample? sample = CreateSample(GcbStateNew.Cold);
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);

        await sut.RunOrStopEmissionAsync();

        Assert.That(sut.ErrorText, Does.Contain("invalid"));
        commands.VerifyNoOtherCalls();
    }

    [Test]
    public void EmissionInputs_PreserveCommittedValuesWithoutClamping()
    {
        UcsiTelemetrySample? sample = CreateSample(GcbStateNew.Cold);
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);

        sut.EmissionKv = 123.45;
        sut.EmissionPower = 456.78;
        sut.EmissionFilament = 999;
        sut.EmissionDurationSeconds = 181.25;
        sut.EmissionXCoilAmps = -1.75;
        sut.EmissionYCoilAmps = 1.75;
        sut.EmissionFocusCoilAmps = 3.25;

        Assert.Multiple(() =>
        {
            Assert.That(sut.EmissionKv, Is.EqualTo(123.45));
            Assert.That(sut.EmissionPower, Is.EqualTo(456.78));
            Assert.That(sut.EmissionFilament, Is.EqualTo(999));
            Assert.That(sut.EmissionDurationSeconds, Is.EqualTo(181.25));
            Assert.That(sut.EmissionXCoilAmps, Is.EqualTo(-1.75));
            Assert.That(sut.EmissionYCoilAmps, Is.EqualTo(1.75));
            Assert.That(sut.EmissionFocusCoilAmps, Is.EqualTo(3.25));
            Assert.That(sut.IsEmissionInputValid, Is.False);
        });
    }

    [Test]
    public async Task CalibrationState_AllowsDirectControlsAndDisablesNormalEmission()
    {
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.ControlBoardState).Returns(GcbStateNew.Calibration);
        telemetry.SetupGet(value => value.XCoilCurrent).Returns(250);
        telemetry.SetupGet(value => value.YCoilCurrent).Returns(-500);
        telemetry.SetupGet(value => value.FocusCurrent).Returns(1_250);
        UcsiTelemetrySample? sample = new(
            0,
            DateTimeOffset.UtcNow,
            0,
            telemetry.Object,
            Array.Empty<FaultEntry>());
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        commands.Setup(value => value.SendCoils(250, -500, 1_250)).Returns(Task.CompletedTask);
        commands.Setup(value => value.SendHvpsEmission(0x04u))
            .ReturnsAsync(new CalibrationEmissionResponse(false));
        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);
        sut.CoilsCommandXCoil = 0.25;
        sut.CoilsCommandYCoil = -0.5;
        sut.CoilsCommandFocus = 1.25;

        await sut.SendCoilsAsync();
        await sut.StopCalibrationAsync();

        Assert.Multiple(() =>
        {
            Assert.That(sut.CanUseCalibrationControls, Is.True);
            Assert.That(sut.CanStopCalibration, Is.True);
            Assert.That(sut.IsEmissionTabAvailable, Is.False);
            Assert.That(sut.EmissionTabUnavailableReason, Does.Contain("Calibration"));
            Assert.That(sut.CoilsCommandXCoil, Is.EqualTo(0.25));
            Assert.That(sut.CoilsCommandYCoil, Is.EqualTo(-0.5));
            Assert.That(sut.CoilsCommandFocus, Is.EqualTo(1.25));
            Assert.That(sut.CoilsFeedbackXCoil, Is.EqualTo(0.25));
            Assert.That(sut.CoilsFeedbackYCoil, Is.EqualTo(-0.5));
            Assert.That(sut.CoilsFeedbackFocus, Is.EqualTo(1.25));
        });
        commands.Verify(value => value.SendCoils(250, -500, 1_250), Times.Once);
        commands.Verify(value => value.SendHvpsEmission(0x04u), Times.Once);
    }

    [Test]
    public async Task NormalActiveState_RejectsDirectCalibrationCommand()
    {
        UcsiTelemetrySample? sample = CreateSample(GcbStateNew.Warmup);
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);

        await sut.SendCoilsAsync();

        Assert.That(sut.ErrorText, Does.Contain("Warmup"));
        commands.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Stop_CancelsPendingSequenceBeforeSendingDirectiveStop()
    {
        UcsiTelemetrySample? sample = CreateSample(GcbStateNew.Cold);
        var commands = new Mock<IGcbCommandInterface>(MockBehavior.Strict);
        commands.Setup(value => value.ResetTimers()).Returns(Task.CompletedTask);
        commands.Setup(value => value.ClearPlan()).Returns(Task.CompletedTask);
        commands.Setup(value => value.WarmUp(2_000f))
            .Callback(() => sample = CreateSample(GcbStateNew.Warmup))
            .Returns(Task.CompletedTask);
        commands.Setup(value => value.Stop()).Returns(Task.CompletedTask);
        UnifiedCalibrationServiceViewModel sut = CreateSut(commands, () => sample);
        SetValidEmission(sut);

        Task start = sut.RunOrStopEmissionAsync();
        await Task.Delay(30);
        await sut.RunOrStopEmissionAsync();
        await start;

        commands.Verify(value => value.ResetTimers(), Times.Once);
        commands.Verify(value => value.ClearPlan(), Times.Once);
        commands.Verify(value => value.WarmUp(2_000f), Times.Once);
        commands.Verify(value => value.Stop(), Times.Once);
        commands.Verify(value => value.NewSession(), Times.Never);
    }

    [Test]
    public async Task DetailedStatus_WatchdogUsesSystemInterlockBit()
    {
        var watchdogMask = 1UL << (int)SystemInterlock.WatchdogReady;
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.ControlBoardState).Returns(GcbStateNew.Cold);
        telemetry.SetupGet(value => value.Interlocks).Returns(new SystemInterlocks(
            (uint)watchdogMask,
            0,
            watchdogMask,
            watchdogMask,
            0));
        UcsiTelemetrySample? sample = new(
            0,
            DateTimeOffset.UtcNow,
            0,
            telemetry.Object,
            Array.Empty<FaultEntry>());
        UnifiedCalibrationServiceViewModel sut = CreateSut(
            new Mock<IGcbCommandInterface>(MockBehavior.Strict),
            () => sample);

        await sut.TickAsync();

        int watchdogIndex = Array.IndexOf(Enum.GetValues<SystemInterlock>(), SystemInterlock.WatchdogReady);
        Assert.Multiple(() =>
        {
            Assert.That(sut.Interlocks[watchdogIndex].Value, Is.EqualTo("Ready"));
            Assert.That(sut.Interlocks[watchdogIndex].IsActive, Is.True);
        });
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public async Task ConfigurationSet_RequiresLoadedFirmwareValuesAndAuditsMatchingChangedReadbackOnly()
    {
        var records = new List<string>();
        var audit = new Mock<IActionAuditService>();
        audit.Setup(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((action, details) => records.Add($"{action}: {details}"));
        var uart = new Mock<IUcsiHvpsUartCommandInterface>();
        uart.SetupGet(x => x.IsConnected).Returns(true);
        float firmwareValue = 100;
        uart.Setup(x => x.RequestSystemConfig()).ReturnsAsync(() => new SystemConfigResponse(Enumerable.Repeat(firmwareValue, 32).ToArray()));
        uart.Setup(x => x.SetSystemConfigValue(0, 200)).Returns(Task.CompletedTask);
        var sut = CreateSut(new Mock<IGcbCommandInterface>(), () => null, uart.Object, audit.Object);
        var item = sut.ConfigItems[0];
        item.InputValue = "200";
        Assert.That(item.SetCommand.CanExecute(), Is.False);
        item.SetCommand.Execute();
        uart.Verify(x => x.SetSystemConfigValue(It.IsAny<int>(), It.IsAny<float>()), Times.Never);

        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        Assert.That(item.SetCommand.CanExecute(), Is.True);
        Assert.That(records, Is.Empty);

        item.SetCommand.Execute();
        Assert.That(records, Is.Empty);
        firmwareValue = 200;
        await sut.TickAsync();
        await sut.TickAsync();
        Assert.That(records, Has.Count.EqualTo(1));
        Assert.That(records[0], Does.Contain("Entity=HVPSConfiguration; Id=0").And.Contain("Fields=Value").And.Not.Contain("200"));

        item.SetCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        Assert.That(records, Has.Count.EqualTo(1));
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public async Task ConfigurationSet_MismatchedReadbackCannotBeAuditedByALaterRefresh()
    {
        var audit = new Mock<IActionAuditService>();
        var uart = new Mock<IUcsiHvpsUartCommandInterface>();
        uart.SetupGet(x => x.IsConnected).Returns(true);
        float firmwareValue = 100;
        uart.Setup(x => x.RequestSystemConfig()).ReturnsAsync(() => new SystemConfigResponse(Enumerable.Repeat(firmwareValue, 32).ToArray()));
        uart.Setup(x => x.SetSystemConfigValue(0, 200)).Returns(Task.CompletedTask);
        var sut = CreateSut(new Mock<IGcbCommandInterface>(), () => null, uart.Object, audit.Object);
        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        sut.ConfigItems[0].InputValue = "200";
        sut.ConfigItems[0].SetCommand.Execute();
        firmwareValue = 150;
        await sut.TickAsync();
        await sut.TickAsync();
        firmwareValue = 200;
        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        audit.Verify(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public async Task ConfigurationSet_FailedCommandAndFailedInitialReadStaySilent()
    {
        var audit = new Mock<IActionAuditService>();
        var uart = new Mock<IUcsiHvpsUartCommandInterface>();
        uart.SetupGet(x => x.IsConnected).Returns(true);
        uart.Setup(x => x.RequestSystemConfig()).ThrowsAsync(new InvalidOperationException("Read failed"));
        var sut = CreateSut(new Mock<IGcbCommandInterface>(), () => null, uart.Object, audit.Object);
        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        Assert.That(sut.ConfigItems[0].SetCommand.CanExecute(), Is.False);

        uart.Setup(x => x.RequestSystemConfig()).ReturnsAsync(new SystemConfigResponse(Enumerable.Repeat(100f, 32).ToArray()));
        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        uart.Setup(x => x.SetSystemConfigValue(0, 200)).ThrowsAsync(new InvalidOperationException("Write failed"));
        sut.ConfigItems[0].InputValue = "200";
        sut.ConfigItems[0].SetCommand.Execute();
        Assert.That(sut.ConfigItems[0].SetCommand.CanExecute(), Is.True);
        audit.Verify(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public async Task ConfigurationSet_ReadbackAndWriteCompletionMustBothSucceed()
    {
        var confirmed = new TaskCompletionSource<string>();
        var audit = new Mock<IActionAuditService>();
        audit.Setup(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, string>((_, details) => confirmed.TrySetResult(details));
        var uart = new Mock<IUcsiHvpsUartCommandInterface>();
        uart.SetupGet(x => x.IsConnected).Returns(true);
        float firmwareValue = 100;
        uart.Setup(x => x.RequestSystemConfig()).ReturnsAsync(() => new SystemConfigResponse(Enumerable.Repeat(firmwareValue, 32).ToArray()));
        var write = new TaskCompletionSource();
        uart.Setup(x => x.SetSystemConfigValue(0, 200)).Returns(write.Task);
        var sut = CreateSut(new Mock<IGcbCommandInterface>(), () => null, uart.Object, audit.Object);
        sut.RefreshSystemConfigCommand.Execute();
        await sut.TickAsync();
        await sut.TickAsync();
        sut.ConfigItems[0].InputValue = "200";
        sut.ConfigItems[0].SetCommand.Execute();
        firmwareValue = 200;
        await sut.TickAsync();
        await sut.TickAsync();
        Assert.That(confirmed.Task.IsCompleted, Is.False);

        write.SetResult();
        var details = await confirmed.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.That(details, Does.Contain("Entity=HVPSConfiguration; Id=0"));
        audit.Verify(x => x.RegisterAction(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    private static UnifiedCalibrationServiceViewModel CreateSut(
        Mock<IGcbCommandInterface> commands,
        Func<UcsiTelemetrySample?> currentSample,
        IUcsiHvpsUartCommandInterface? hvpsUart = null,
        IActionAuditService? audit = null)
    {
        var coordinator = new Mock<ITelemetrySessionCoordinator>();
        coordinator.SetupGet(value => value.Mode).Returns(UcsiMode.Live);
        coordinator.SetupGet(value => value.CurrentSample).Returns(currentSample);
        coordinator.Setup(value => value.AdvancePresentationAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var catalog = new TelemetryParameterCatalog();
        var hostCommands = new Mock<IUcsiHostCommands>();
        hostCommands.SetupGet(value => value.ClearFaultsUnavailableReason).Returns("Unavailable");
        hostCommands.SetupGet(value => value.CanStartEmission).Returns(true);

        return new UnifiedCalibrationServiceViewModel(
            coordinator.Object,
            catalog,
            hostCommands.Object,
            new UcsiLogBuffer(),
            commands.Object,
            Mock.Of<ISystemTelemetryProcessor>(),
            hvpsUart ?? Mock.Of<IUcsiHvpsUartCommandInterface>(),
            new SessionDataExportService(catalog),
            audit ?? Mock.Of<IActionAuditService>());
    }

    private static UcsiTelemetrySample CreateSample(GcbStateNew state)
    {
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.ControlBoardState).Returns(state);
        return new UcsiTelemetrySample(0, DateTimeOffset.UtcNow, 0, telemetry.Object, Array.Empty<FaultEntry>());
    }

    private static void SetValidEmission(UnifiedCalibrationServiceViewModel sut)
    {
        sut.EmissionKv = 50;
        sut.EmissionPower = 100;
        sut.EmissionFilament = 2_000;
        sut.EmissionDurationSeconds = 2.5;
        sut.EmissionXCoilAmps = 0.25;
        sut.EmissionYCoilAmps = -0.5;
        sut.EmissionFocusCoilAmps = 1.25;
    }
}
