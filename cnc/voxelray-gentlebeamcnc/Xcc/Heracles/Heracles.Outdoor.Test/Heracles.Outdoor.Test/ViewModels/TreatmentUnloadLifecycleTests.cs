using System.Collections.ObjectModel;
using System.Windows.Threading;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Models;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.Core.Models.RDBMS;
using Heracles.External.AppServices;
using Heracles.External.AppServices.Plan;
using Heracles.External.AppServices.System;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Heracles.External.ViewModels;
using Moq;
using NUnit.Framework;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Domain.System;
using Xcc.Application.Models;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;
using OutdoorPlanModel = Heracles.External.Models.PlanModel;

namespace Heracles.Outdoor.Test.ViewModels;

internal sealed class TreatmentUnloadLifecycleTests
{
    [TestCase(TreatmentLoadingState.Unloaded)]
    [TestCase(TreatmentLoadingState.Loaded)]
    public void LoadStream_NonPendingNotification_DoesNotRepopulatePlan(TreatmentLoadingState state)
    {
        using var context = new LifecycleContext();
        var observedPlanIds = new List<long>();
        context.PlanModel.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(OutdoorPlanModel.Plan) && context.PlanModel.Plan is { } plan)
                observedPlanIds.Add(plan.Id);
        };

        context.PublishPlan(33, state);
        // The legitimate request is also a queue barrier: no arbitrary delay is needed to prove
        // the preceding notification was ignored rather than briefly displayed and replaced.
        context.PublishPlan(44, TreatmentLoadingState.PendingLoad);
        PumpUntil(() => context.PlanModel.Plan?.Id == 44 && context.ViewModel.PrepareCommand.CanExecute());

        Assert.That(observedPlanIds, Is.EqualTo(new[] { 44L }));
    }

    [TestCase(TreatmentLoadingState.PendingLoad)]
    [TestCase(TreatmentLoadingState.PartialPendingLoad)]
    public void LoadStream_PendingRequest_LoadsTreatmentAndEnablesPrepare(TreatmentLoadingState state)
    {
        using var context = new LifecycleContext();

        context.PublishPlan(33, state);
        PumpUntil(() => context.ViewModel.PrepareCommand.CanExecute());

        Assert.Multiple(() =>
        {
            Assert.That(context.PlanModel.Plan.Id, Is.EqualTo(33));
            Assert.That(context.PlanModel.Plan.TreatmentLoadingState, Is.EqualTo(state));
            Assert.That(context.PlanModel.TreatmentFields.Single().Name, Is.EqualTo(TreatmentFieldName.PlusC));
            Assert.That(context.ActiveTreatment?.PlanId, Is.EqualTo(33));
        });
    }

    [Test]
    public void DelayedUnload_BlocksPrepareAndCompletesOnlyAfterLocalPlanAndTreatmentClear()
    {
        using var context = new LifecycleContext();
        context.LoadPlan();
        var serverCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var serverCalled = false;
        context.PlanCommands.Setup(value => value.UnloadFromTreatmentAsync(33))
            .Callback(() => serverCalled = true).Returns(serverCompletion.Task);

        var unload = context.ViewModel.Unload();
        try
        {
            Assert.That(context.ViewModel.PrepareCommand.CanExecute(), Is.False);
            PumpUntil(() => serverCalled);
            Assert.Multiple(() =>
            {
                Assert.That(unload.IsCompleted, Is.False);
                Assert.That(context.ViewModel.PrepareCommand.CanExecute(), Is.False);
                Assert.That(context.PlanModel.Plan, Is.Not.Null);
                Assert.That(context.ActiveTreatment, Is.Not.Null);
            });
        }
        finally
        {
            serverCompletion.TrySetResult();
            PumpUntil(() => unload.IsCompleted);
        }
        unload.GetAwaiter().GetResult();

        Assert.Multiple(() =>
        {
            Assert.That(context.PlanModel.Plan, Is.Null);
            Assert.That(context.PlanModel.TreatmentFields, Is.Empty);
            Assert.That(context.ActiveTreatment, Is.Null);
            Assert.That(context.ViewModel.PrepareCommand.CanExecute(), Is.False);
        });
    }

    [Test]
    public void FailedUnload_KeepsPrepareBlockedUntilSuccessfulRetryAndNewPendingPlan()
    {
        using var context = new LifecycleContext();
        context.LoadPlan();
        context.PlanCommands.SetupSequence(value => value.UnloadFromTreatmentAsync(33))
            .ThrowsAsync(new IOException("Server unavailable"))
            .Returns(Task.CompletedTask);

        var unload = context.ViewModel.Unload();
        PumpUntil(() => unload.IsCompleted);
        Assert.Throws<IOException>(() => unload.GetAwaiter().GetResult());
        Assert.Multiple(() =>
        {
            Assert.That(context.PlanModel.Plan.Id, Is.EqualTo(33));
            Assert.That(context.ActiveTreatment, Is.Not.Null);
            Assert.That(context.ViewModel.PrepareCommand.CanExecute(), Is.False);
        });

        context.ViewModel.CurrentTaskCommand.Execute();
        PumpUntil(() => context.PlanModel.Plan is null && context.ActiveTreatment is null);
        context.PublishPlan(44, TreatmentLoadingState.PendingLoad);
        PumpUntil(() => context.ViewModel.PrepareCommand.CanExecute());
        Assert.That(context.PlanModel.Plan.Id, Is.EqualTo(44));
    }

    [Test]
    public void NavigationRecovery_CompletedBoardPlan_UnloadsWithoutDeadlockingSerializedQueue()
    {
        using var context = new LifecycleContext();
        context.ConfigureCompletedRecovery();
        var unloaded = false;
        context.PlanCommands.Setup(value => value.UnloadFromTreatmentAsync(33))
            .Callback(() => unloaded = true).Returns(Task.CompletedTask);
        var navigation = new NavigationContext(
            Mock.Of<IRegionNavigationService>(value => value.Region == new Region { Name = "Treatment" }),
            new Uri("Treatment", UriKind.Relative));

        context.ViewModel.OnNavigatedTo(navigation);
        PumpUntil(() => unloaded && context.PlanModel.Plan is null && context.ActiveTreatment is null);

        context.PublishPlan(44, TreatmentLoadingState.PendingLoad);
        PumpUntil(() => context.ViewModel.PrepareCommand.CanExecute());
        Assert.That(context.PlanModel.Plan.Id, Is.EqualTo(44));
    }

    private static void PumpUntil(Func<bool> completed)
    {
        if (completed())
            return;

        var frame = new DispatcherFrame();
        var deadline = DateTime.UtcNow.AddSeconds(5);
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(1)
        };
        timer.Tick += (_, _) =>
        {
            if (completed() || DateTime.UtcNow >= deadline)
                frame.Continue = false;
        };
        timer.Start();
        try
        {
            Dispatcher.PushFrame(frame);
        }
        finally
        {
            timer.Stop();
        }
        Assert.That(completed(), Is.True, "Treatment lifecycle did not settle within five seconds.");
    }

    private sealed class LifecycleContext : IDisposable
    {
        private const TargetType Target = TargetType.TargetType_30mm_SSD_7_Fields;
        private readonly CancellationTokenSource _lifetime = new();
        private Action<LoadForTreatmentEventsStreamArgs> _publish = null!;

        public LifecycleContext()
        {
            var collimatorConfiguration = Mock.Of<ICollimatorConfiguration>(value =>
                value.Id == 77 && value.Type == Target && value.Energy == Energy.Energy_100);
            var collimator = new Mock<ICollimatorModel>();
            collimator.SetupGet(value => value.ActiveCollimator)
                .Returns(Mock.Of<ICollimator>(value => value.Configuration == collimatorConfiguration));
            collimator.SetupGet(value => value.CollimatorConfigurations)
                .Returns(new ObservableCollection<ICollimatorConfiguration> { collimatorConfiguration });
            collimator.Setup(value => value.FindConfigurationByType(Target, Energy.Energy_100))
                .Returns(collimatorConfiguration);
            PlanModel = new OutdoorPlanModel(PlanCommands.Object, collimator.Object);
            PlanCommands.Setup(value => value.UnloadFromTreatmentAsync(It.IsAny<long>()))
                .Returns(Task.CompletedTask);

            var fields = new Mock<IPlanRepository>();
            fields.Setup(value => value.FetchTreatmentFieldsAsync(It.IsAny<long>(), Target))
                .ReturnsAsync([new TreatmentField
                {
                    Id = 55, Name = TreatmentFieldName.PlusC, DwellTime = 5, Energy = Energy.Energy_100
                }]);
            var prescriptions = new Mock<IEmrPrescriptionCommands>();
            prescriptions.Setup(value => value.ReadAsync(5)).ReturnsAsync(new Prescription
            {
                Id = 5, SimulationId = 6, Energy = Energy.Energy_100, DailyDose = 2
            });
            var simulations = new Mock<IEmrSimulationCommands>();
            simulations.Setup(value => value.ReadAsync(6)).ReturnsAsync(new Simulation
            {
                Id = 6, DiagnosisId = 7, TargetType = Target, LesionDepth = 1
            });
            var diagnoses = new Mock<IEmrDiagnosisCommands>();
            diagnoses.Setup(value => value.ReadAsync(7)).ReturnsAsync(new Diagnosis { Id = 7, PatientId = 8 });
            var patients = new Mock<IPatientRepository>();
            patients.Setup(value => value.FetchAsync(8)).ReturnsAsync(Mock.Of<IPatient>());
            var loading = new PlanLoadingService(new TreatmentInfoStore(), patients.Object,
                diagnoses.Object, simulations.Object, prescriptions.Object, fields.Object);

            var calibrationInfo = new Mock<ICollimatorCalibrationInfo>();
            calibrationInfo.SetupGet(value => value.CollimatorConfiguration).Returns(collimatorConfiguration);
            calibrationInfo.SetupGet(value => value.HeaterCurrent).Returns(2);
            calibrationInfo.Setup(value => value.GetCoilConfiguration(It.IsAny<TreatmentFieldName>()))
                .Returns(new CoilConfigurationInfo());
            calibrationInfo.Setup(value => value.GetOutputFactor(It.IsAny<TreatmentFieldName>()))
                .Returns(new OutputFactorInfo(1, 1));
            var calibrationStore = new CollimatorCalibrationInfoStore { [77] = calibrationInfo.Object };
            var calibration = new Mock<ICollimatorCalibrationModel>();
            calibration.Setup(value => value.FetchCalibrationDataAsync(false)).ReturnsAsync(calibrationStore);
            var profile = new CollimatorProfileService(collimator.Object, calibration.Object);
            var user = Mock.Of<IUser>(value => value.EmailAddress == "clinician@example.test"
                && value.Role == new UserRole("Clinician") { Permissions = new UserPermissions { Treatment = true } });
            var userStore = Mock.Of<IAuthorizedUserStore>(value => value.AuthorizedUser == user);
            var treatment = new Mock<ITreatmentModel>();
            treatment.Setup(value => value.SetTreatment(It.IsAny<ITreatment>()))
                .Callback<ITreatment>(value => ActiveTreatment = value);
            treatment.Setup(value => value.CloseTreatment()).Callback(() => ActiveTreatment = null);
            Dialogs.Setup(value => value.ShowDialog(It.IsAny<string>(), It.IsAny<IDialogParameters>(),
                    It.IsAny<Action<IDialogResult>>()))
                .Callback<string, IDialogParameters, Action<IDialogResult>>((_, _, callback) =>
                    callback(new DialogResult(ButtonResult.OK)));
            var preparation = new TreatmentPreparationService(loading, Dialogs.Object, profile,
                Treatments.Object, PlanModel, Mock.Of<IActualTreatmentFieldModel>(), treatment.Object, userStore);

            var stream = new Mock<ILoadForTreatmentEventStream>();
            stream.Setup(value => value.RunStreamAsync(It.IsAny<Action<LoadForTreatmentEventsStreamArgs>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<Action<LoadForTreatmentEventsStreamArgs>, CancellationToken>((callback, _) => _publish = callback)
                .Returns(Task.CompletedTask);
            var source = new LoadSourceHarness(stream.Object);
            var ui = new Mock<IUIStateMachine>();
            ui.SetupGet(value => value.State).Returns(UIMacroState.StandBy);
            ui.SetupGet(value => value.LeftButton).Returns(new LeftButtonInfo());
            ui.SetupGet(value => value.CentralButton).Returns(new CentralButtonInfo());
            ui.SetupGet(value => value.RightButton).Returns(new RightButtonInfo());
            Board.Setup(value => value.CanPrepare()).Returns(true);
            var configurations = new Mock<ICollimatorConfigurationStore>();
            configurations.SetupGet(value => value.HeaterCurrent)
                .Returns(Mock.Of<IHeaterCurrentConfig>(value => value.HeaterCurrent == 2));
            configurations.SetupGet(value => value.CoilConfigurations)
                .Returns(new List<ICoilConfigurationEntry>
                {
                    Mock.Of<ICoilConfigurationEntry>(value => value.FieldName == TreatmentFieldName.PlusC)
                });
            var dataStore = Mock.Of<IGCBDataStore>(value => value.SystemTelemetry == Mock.Of<ISystemTelemetry>());
            var globals = Mock.Of<IAppGlobals>(value => value.AppCancellationTokenSource == _lifetime);
            ViewModel = new UnloadHarness(globals, dataStore, PlanModel, source, Board.Object,
                configurations.Object, treatment.Object, preparation, ui.Object, collimator.Object, userStore,
                Dialogs.Object);
            source.Connect().GetAwaiter().GetResult();
        }

        public Mock<IEmrPlanCommands> PlanCommands { get; } = new();
        public Mock<ITreatmentRepository> Treatments { get; } = new();
        public Mock<IMainBoardModel> Board { get; } = new();
        public Mock<IDialogService> Dialogs { get; } = new();
        public OutdoorPlanModel PlanModel { get; }
        public UnloadHarness ViewModel { get; }
        public ITreatment? ActiveTreatment { get; private set; }

        public void PublishPlan(long id, TreatmentLoadingState state) =>
            _publish(new LoadForTreatmentEventsStreamArgs(new Plan
            {
                Id = id, PrescriptionId = 5, CollimatorType = Target, TreatmentLoadingState = state
            }, null));

        public void ConfigureCompletedRecovery()
        {
            var field = new TreatmentField
            {
                Id = 55, Name = TreatmentFieldName.PlusC, DwellTime = 5, Energy = Energy.Energy_100
            };
            var plan = new Plan
            {
                Id = 33, PrescriptionId = 5, CollimatorType = Target,
                TreatmentLoadingState = TreatmentLoadingState.Loaded
            };
            var hydrated = new Plan(plan, [field]);
            var previous = new Treatment(new Treatment { Id = 66, PlanId = 33 }, hydrated,
                [new ActualTreatmentField(field) { ActualDuration = 5, Completed = 0 }]);
            PlanCommands.Setup(value => value.FindLoadedPlanAsync()).ReturnsAsync(plan);
            Treatments.Setup(value => value.FetchLatestTreatmentByPlanAsync(It.Is<IPlan>(value => value.Id == 33)))
                .ReturnsAsync(previous);
            Board.Setup(value => value.QueryEmissionFromGCB()).Returns(() =>
                Task.FromResult(ViewModel.ExpectedEmission()));
            GcbOperationalPoint? current = null;
            Board.Setup(value => value.SetCurrentEmission(It.IsAny<GcbOperationalPoint>()))
                .Callback<GcbOperationalPoint>(value => current = value);
            Board.SetupGet(value => value.CurrentEmission).Returns(() => current);
            Board.Setup(value => value.ResetTimers()).Returns(Task.CompletedTask);
            Board.Setup(value => value.ClearPlan()).Callback(() => current = null).Returns(Task.CompletedTask);
        }

        public void LoadPlan()
        {
            PublishPlan(33, TreatmentLoadingState.PendingLoad);
            PumpUntil(() => ViewModel.PrepareCommand.CanExecute());
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
    }

    private sealed class LoadSourceHarness(ILoadForTreatmentEventStream stream)
        : LoadForTreatmentEventSource(stream, CancellationToken.None)
    {
        public Task Connect() => RunEventStreamProcessing(CancellationToken.None);
    }

    private sealed class UnloadHarness : TreatmentViewModel
    {
        public UnloadHarness(IAppGlobals globals, IGCBDataStore dataStore, OutdoorPlanModel plan,
            LoadForTreatmentEventSource source, IMainBoardModel board,
            ICollimatorConfigurationStore configurations, ITreatmentModel treatment,
            TreatmentPreparationService preparation, IUIStateMachine ui,
            ICollimatorModel collimator, IAuthorizedUserStore userStore, IDialogService dialogs)
            : base(Mock.Of<IRegionManager>(), dialogs, Mock.Of<IHeraclesExternalSettings>(),
                globals, CreateEventAggregator(), Mock.Of<ILogWriter>(), dataStore, plan, source,
                new PlanEventSource(Mock.Of<IPlanEventStream>(), CancellationToken.None), board,
                Mock.Of<IGcbIndicators>(), configurations, treatment, Mock.Of<IActualTreatmentFieldModel>(),
                Mock.Of<ITreatmentDoseCalculation>(), preparation, null!, new ApplicatorCompatibilityService(collimator),
                Mock.Of<IWarmupService>(), ui, collimator, Mock.Of<IPopUpService>(), Mock.Of<ISafetyCheckModel>(),
                Mock.Of<IBearerTokenUserSessionManager>(), userStore)
        {
        }

        private static IEventAggregator CreateEventAggregator()
        {
            var dispatcher = System.Windows.Application.Current.Dispatcher;
            dispatcher.VerifyAccess();
            var events = new EventAggregator();
            events.GetEvent<SystemTelemetryChangedEvent>().SynchronizationContext =
                new DispatcherSynchronizationContext(dispatcher);
            return events;
        }

        public Task Unload() => SetPlanUnloadTaskAsync();
        public GcbOperationalPoint ExpectedEmission() => BuildGcbOperationalPoint(0);
    }
}
