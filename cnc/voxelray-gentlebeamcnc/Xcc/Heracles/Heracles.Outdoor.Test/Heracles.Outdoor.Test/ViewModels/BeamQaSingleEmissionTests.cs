using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.System;
using Heracles.Application.Enums;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Heracles.External.ViewModels.QualityCheck;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.Outdoor.Test.ViewModels;

internal sealed class BeamQaViewModelHarness : BeamQaViewModel
{
    public BeamQaViewModelHarness(
        IEventAggregator eventAggregator,
        IHeraclesExternalSettings settings,
        IGCBDataStore gcbDataStore,
        IUIStateMachine uiStateMachine,
        IWarmupService warmupService,
        IMainBoardModel mainBoardModel,
        IGcbIndicators indicators,
        IAuthorizedUserStore userStore,
        QcReportService reportService,
        IDispatcherService dispatcherService,
        IQcbService qcbService,
        ICollimatorModel collimatorModel)
        : base(
            Mock.Of<IRegionManager>(),
            eventAggregator,
            settings,
            gcbDataStore,
            Mock.Of<ILogWriter>(),
            uiStateMachine,
            Mock.Of<IDialogService>(),
            warmupService,
            collimatorModel,
            Mock.Of<IPopUpService>(),
            mainBoardModel,
            indicators,
            userStore,
            reportService,
            dispatcherService,
            qcbService,
            Mock.Of<ICollimatorCalibrationModel>(),
            Mock.Of<ICollimatorConfigurationStore>(),
            new ApplicatorCompatibilityService(collimatorModel),
            Mock.Of<ISafetyCheckModel>(),
            Mock.Of<IBearerTokenUserSessionManager>())
    {
    }

    public bool CanPrepareSelectedEmission() => CanPrepare();

    public Task PrepareSelectedEmission() => PrepareAsync(tryKeepPrevPlan: false);

    public Task RunSelectedEmission() => OnBeamOnClicked();

    protected override GcbOperationalPoint BuildGcbOperationalPoint(int fieldIndex)
    {
        var field = QcPlan.Fields[fieldIndex];
        return new GcbOperationalPoint
        {
            TotalPointTime = (float)field.Duration,
            RemainingPointTime = (float)(field.Duration - field.Actual),
            SetpointKv = 40 + field.DisplayValue,
        };
    }

    protected override Task UpdateAfterEmission(CancellationToken token) =>
        Task.CompletedTask;
}

internal sealed class BeamQaTestContext
{
    private GcbStateNew telemetryState = GcbStateNew.StandBy;
    private bool boardPlanStaged;
    private UIMacroState uiState = UIMacroState.StandBy;

    public BeamQaTestContext()
    {
        SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

        Settings.SetupGet(value => value.QcFieldDuration).Returns(1);
        Telemetry.SetupGet(value => value.ControlBoardState).Returns(() => telemetryState);
        GcbDataStore.SetupGet(value => value.SystemTelemetry).Returns(Telemetry.Object);

        UiStateMachine.SetupGet(value => value.State).Returns(() => uiState);
        UiStateMachine.SetupGet(value => value.LeftButton).Returns(new LeftButtonInfo { IsEnabled = true });
        UiStateMachine.SetupGet(value => value.CentralButton).Returns(new CentralButtonInfo { IsEnabled = true });
        UiStateMachine.SetupGet(value => value.RightButton).Returns(new RightButtonInfo { IsEnabled = true });
        UiStateMachine.SetupProperty(value => value.IsPlanStaged, false);
        UiStateMachine.SetupProperty(value => value.IsPlanLoadedForTreatment, false);
        UiStateMachine.SetupProperty(value => value.TabName, ExternalTabName.QA);
        UiStateMachine
            .Setup(value => value.RequestStateSwitch(It.IsAny<UIMacroState>()))
            .Callback<UIMacroState>(value => uiState = value);

        MainBoard.SetupGet(value => value.IsPlanStaged).Returns(() => boardPlanStaged);
        MainBoard
            .Setup(value => value.PrepareEmission(It.IsAny<GcbOperationalPoint>(), false))
            .Callback(() => boardPlanStaged = true)
            .ReturnsAsync(true);
        MainBoard.Setup(value => value.BeamOn()).Returns(() =>
        {
            Calls.Add("BeamOn");
            telemetryState = GcbStateNew.Emission;
            return Task.CompletedTask;
        });
        MainBoard.Setup(value => value.UpdateCurrentEmissionFromGCB()).Returns(Task.CompletedTask);
        MainBoard.SetupGet(value => value.CurrentEmission).Returns(() => CompletedEmission);
        MainBoard.Setup(value => value.ResetTimers()).Returns(Task.CompletedTask);
        MainBoard.Setup(value => value.ClearPlan()).Callback(() =>
        {
            boardPlanStaged = false;
            telemetryState = GcbStateNew.StandBy;
        }).Returns(Task.CompletedTask);

        Indicators.SetupGet(value => value.BeamOnProgress)
            .Returns(new BeamOnProgress(MainBoard.Object));
        QcbService.Setup(value => value.PingBoardAsync()).ReturnsAsync(true);
        QcbService
            .Setup(value => value.StartQCReadingsAsync())
            .Callback(() => Calls.Add("StartQC"))
            .ReturnsAsync(QcbCommandResponseStatus.StartConfirmed);
        QcbService
            .Setup(value => value.StopQCReadingsAsync())
            .Callback(() => Calls.Add("StopQC"))
            .ReturnsAsync(Readings);

        Dispatcher
            .Setup(value => value.Invoke(It.IsAny<Action>()))
            .Callback<Action>(action => action());

        var user = Mock.Of<IUser>(value => value.EmailAddress == "qa@example.test");
        UserStore.SetupGet(value => value.AuthorizedUser).Returns(user);

        ReportList.SetupGet(value => value.CurrentCollimatorConfigurationId)
            .Returns(ConfigurationId);
        Repository
            .Setup(value => value.CreateQcSampleAsync(
                It.IsAny<IQcSampleHeader>(),
                It.IsAny<IEnumerable<IQcSampleFieldEntry>>()))
            .Returns((IQcSampleHeader header, IEnumerable<IQcSampleFieldEntry> fields) =>
            {
                var storedFields = fields.Select(field =>
                    new QcField(
                        field.Name,
                        field.Intensities.Accumulations.Select(reading => (double?)reading).ToArray()));
                return Task.FromResult<(IQcSampleHeader, IEnumerable<QcField>)>(
                    (header, storedFields));
            });

        var reportService = new QcReportService(
            ReportList.Object,
            Repository.Object,
            UserStore.Object,
            Mock.Of<ILogWriter>());
        ViewModel = new BeamQaViewModelHarness(
            new EventAggregator(),
            Settings.Object,
            GcbDataStore.Object,
            UiStateMachine.Object,
            WarmupService.Object,
            MainBoard.Object,
            Indicators.Object,
            UserStore.Object,
            reportService,
            Dispatcher.Object,
            QcbService.Object,
            CollimatorModel.Object);
        ViewModel.ApplicatorCompatibilityStatus = ApplicatorCompatibilityStatus.Compatible;

        var configuration = new Mock<ICollimatorConfiguration>();
        configuration.SetupGet(value => value.Id).Returns(ConfigurationId);
        configuration.SetupGet(value => value.Type).Returns(TargetType.TargetType_30mm_SSD_7_Fields);
        configuration.SetupGet(value => value.Energy).Returns(Energy.Energy_50);

        var first = new QcSampleFieldEntry(configuration.Object, filamentSetpoint: 1.0)
        {
            Name = TreatmentFieldName.Plus4L2,
            DisplayValue = 0,
        };
        var second = new QcSampleFieldEntry(configuration.Object, filamentSetpoint: 1.0)
        {
            Name = TreatmentFieldName.Plus4L1,
            DisplayValue = 1,
        };
        ViewModel.QcPlan.AddField(first);
        ViewModel.QcPlan.AddField(second);
        FirstField = ViewModel.QcPlan.Fields.Single(value => value.Name == first.Name);
        SecondField = ViewModel.QcPlan.Fields.Single(value => value.Name == second.Name);
    }

    public const long ConfigurationId = 77;

    public Mock<IHeraclesExternalSettings> Settings { get; } = new();
    public Mock<ISystemTelemetry> Telemetry { get; } = new();
    public Mock<IGCBDataStore> GcbDataStore { get; } = new();
    public Mock<IUIStateMachine> UiStateMachine { get; } = new();
    public Mock<IWarmupService> WarmupService { get; } = new();
    public Mock<IMainBoardModel> MainBoard { get; } = new();
    public Mock<IGcbIndicators> Indicators { get; } = new();
    public Mock<IAuthorizedUserStore> UserStore { get; } = new();
    public Mock<IQcReportListModel> ReportList { get; } = new();
    public Mock<IQcRepository> Repository { get; } = new();
    public Mock<IDispatcherService> Dispatcher { get; } = new();
    public Mock<IQcbService> QcbService { get; } = new();
    public Mock<ICollimatorModel> CollimatorModel { get; } = new();
    public List<string> Calls { get; } = [];

    public BeamQaViewModelHarness ViewModel { get; }
    public IQcSampleFieldEntry FirstField { get; }
    public IQcSampleFieldEntry SecondField { get; }
    public QcReadings Readings { get; } = new(1u, 2u, 10u, 11u);
    public GcbOperationalPoint CompletedEmission { get; set; } = new()
    {
        TotalPointTime = 1,
        RemainingPointTime = 0,
    };
}

internal class BeamQaSingleEmissionTests
{
    [Test]
    public void NoSelection_DisablesPrepare()
    {
        var context = new BeamQaTestContext();

        Assert.That(context.ViewModel.CanPrepareSelectedEmission(), Is.False);
        context.MainBoard.Verify(
            value => value.PrepareEmission(It.IsAny<GcbOperationalPoint>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Test]
    public async Task SelectedSecondEmission_PreparesOnlySelectedEmission()
    {
        var context = new BeamQaTestContext();
        context.ViewModel.SelectedEmission = context.SecondField;

        Assert.That(context.ViewModel.CanPrepareSelectedEmission(), Is.True);
        await context.ViewModel.PrepareSelectedEmission();

        Assert.That(context.ViewModel.ActiveEmissionIndex, Is.EqualTo(1));
        context.MainBoard.Verify(value => value.PrepareEmission(
            It.Is<GcbOperationalPoint>(point => point.SetpointKv == 41), false), Times.Once);
        context.MainBoard.Verify(value => value.PrepareEmission(
            It.Is<GcbOperationalPoint>(point => point.SetpointKv == 40), false), Times.Never);
        context.MainBoard.Verify(value => value.BeamOn(), Times.Never);
    }

    [Test]
    public async Task CompletedSelectedEmission_DoesNotPrepareAnotherEmission()
    {
        var context = new BeamQaTestContext();
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();

        await context.ViewModel.RunSelectedEmission();

        context.MainBoard.Verify(
            value => value.PrepareEmission(It.IsAny<GcbOperationalPoint>(), false),
            Times.Once);
        context.MainBoard.Verify(value => value.BeamOn(), Times.Once);
        context.MainBoard.Verify(value => value.ResetTimers(), Times.Once);
        context.MainBoard.Verify(value => value.ClearPlan(), Times.Once);
        context.WarmupService.VerifyNoOtherCalls();
        context.UiStateMachine.Verify(
            value => value.RequestStateSwitch(UIMacroState.StandBy),
            Times.Once);
        Assert.That(context.ViewModel.SelectedEmission, Is.Null);
        Assert.That(context.Calls, Is.EqualTo(new[] { "StartQC", "BeamOn", "StopQC" }));
    }

    [Test]
    public async Task IncompleteSelectedEmission_IsNotSavedOrFollowedByAnother()
    {
        var context = new BeamQaTestContext
        {
            CompletedEmission = new GcbOperationalPoint
            {
                TotalPointTime = 1,
                RemainingPointTime = 0.5f,
            },
        };
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();

        await context.ViewModel.RunSelectedEmission();

        context.MainBoard.Verify(
            value => value.PrepareEmission(It.IsAny<GcbOperationalPoint>(), false),
            Times.Once);
        context.Repository.Verify(value => value.CreateQcSampleAsync(
            It.IsAny<IQcSampleHeader>(),
            It.IsAny<IEnumerable<IQcSampleFieldEntry>>()), Times.Never);
    }

    [Test]
    public async Task CompletedSelectedEmission_SavesOnlySelectedField()
    {
        var context = new BeamQaTestContext();
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();

        await context.ViewModel.RunSelectedEmission();

        context.Repository.Verify(value => value.CreateQcSampleAsync(
            It.IsAny<IQcSampleHeader>(),
            It.Is<IEnumerable<IQcSampleFieldEntry>>(fields =>
                fields.Count() == 1 && ReferenceEquals(fields.Single(), context.SecondField))),
            Times.Once);
        context.ReportList.Verify(value => value.AddNewSample(
            It.Is<QcSampleBindable>(sample =>
                sample.Fields.Count == 1
                && sample.Fields.Single().Values.Count == 2)), Times.Once);
    }

    [Test]
    public async Task AnotherEmission_RequiresNewOperatorCycle()
    {
        var context = new BeamQaTestContext();
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();
        await context.ViewModel.RunSelectedEmission();

        Assert.That(context.ViewModel.SelectedEmission, Is.Null);
        Assert.That(context.ViewModel.CanPrepareSelectedEmission(), Is.False);

        context.ViewModel.SelectedEmission = context.FirstField;
        await context.ViewModel.PrepareSelectedEmission();
        await context.ViewModel.RunSelectedEmission();

        context.MainBoard.Verify(
            value => value.PrepareEmission(It.IsAny<GcbOperationalPoint>(), false),
            Times.Exactly(2));
        context.MainBoard.Verify(value => value.BeamOn(), Times.Exactly(2));
    }
    [Test]
    public async Task RejectedStart_PreventsBeamAndRunsOneCleanupStop()
    {
        var context = new BeamQaTestContext();
        context.QcbService
            .Setup(value => value.StartQCReadingsAsync())
            .ReturnsAsync(QcbCommandResponseStatus.StartRejected);
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();

        await context.ViewModel.RunSelectedEmission();

        context.MainBoard.Verify(value => value.BeamOn(), Times.Never);
        context.QcbService.Verify(value => value.StopQCReadingsAsync(), Times.Once);
        context.Repository.Verify(value => value.CreateQcSampleAsync(
            It.IsAny<IQcSampleHeader>(),
            It.IsAny<IEnumerable<IQcSampleFieldEntry>>()), Times.Never);
    }

    [Test]
    public async Task LostStartResponse_StillRunsOneCleanupStop()
    {
        var context = new BeamQaTestContext();
        context.QcbService
            .Setup(value => value.StartQCReadingsAsync())
            .ThrowsAsync(new TimeoutException("lost response"));
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();

        await context.ViewModel.RunSelectedEmission();

        context.MainBoard.Verify(value => value.BeamOn(), Times.Never);
        context.QcbService.Verify(value => value.StopQCReadingsAsync(), Times.Once);
    }

    [Test]
    public async Task StopFailure_NeverSavesPartialData()
    {
        var context = new BeamQaTestContext();
        context.QcbService
            .Setup(value => value.StopQCReadingsAsync())
            .ThrowsAsync(new QcAcquisitionException("failed"));
        context.ViewModel.SelectedEmission = context.SecondField;
        await context.ViewModel.PrepareSelectedEmission();

        await context.ViewModel.RunSelectedEmission();

        context.QcbService.Verify(value => value.StopQCReadingsAsync(), Times.Exactly(2));
        context.Repository.Verify(value => value.CreateQcSampleAsync(
            It.IsAny<IQcSampleHeader>(),
            It.IsAny<IEnumerable<IQcSampleFieldEntry>>()), Times.Never);
    }

}
