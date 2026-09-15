using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Models;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.External.Models;
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
using Xcc.Application.Models;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;
using OutdoorPlanModel = Heracles.External.Models.IPlanModel;

namespace Heracles.Outdoor.Test.ViewModels;

internal sealed class TreatmentDeliveryAuditTests
{
    [Test]
    public async Task CompletedDelivery_RecordsBeforeConfirmationWithMeasuredDurationAndStableIdsOnce()
    {
        using var context = new DeliveryContext();
        var auditCountAtConfirmation = -1;
        context.OnConfirmation = () => auditCountAtConfirmation = context.UserAudits.Count;
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 5);
            context.Publish(GcbStateNew.Cold);
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();
        Assert.That(auditCountAtConfirmation, Is.EqualTo(1));

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(context.UserAudits[0], Does.Contain("user id=11"));
            Assert.That(context.UserAudits[0], Does.Contain("patient id=22"));
            Assert.That(context.UserAudits[0], Does.Contain("plan id=33"));
            Assert.That(context.UserAudits[0], Does.Contain("treatment id=44"));
            Assert.That(context.UserAudits[0], Does.Contain("field id=55"));
            Assert.That(context.UserAudits[0], Does.Contain("outcome=completed"));
            Assert.That(context.UserAudits[0], Does.Contain("observed delivered duration seconds=5"));
        });
    }

    [Test]
    public async Task InterruptedDelivery_RecordsPartialDurationEvenAfterStopDisablesViewUpdates()
    {
        using var context = new DeliveryContext();
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 1);
            context.ViewModel.IsCurrentViewModelRunning = false;
            context.Publish(GcbStateNew.Termination, 2);
            context.Publish(GcbStateNew.Cold);
            return Task.FromException(new TaskCanceledException());
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(context.UserAudits[0], Does.Contain("outcome=interrupted"));
            Assert.That(context.UserAudits[0], Does.Contain("observed delivered duration seconds=2"));
        });
    }

    [Test]
    public async Task ResumedDelivery_RecordsSeparateSegmentsWithoutCountingPreviousDurationAgain()
    {
        using var context = new DeliveryContext();
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 2);
            context.Publish(GcbStateNew.Cold);
            return Task.FromException(new TaskCanceledException());
        };
        await context.ViewModel.Deliver();
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 3);
            context.Publish(GcbStateNew.Cold);
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(context.UserAudits[0], Does.Contain("outcome=interrupted"));
            Assert.That(context.UserAudits[0], Does.Contain("observed delivered duration seconds=2"));
            Assert.That(context.UserAudits[1], Does.Contain("outcome=completed"));
            Assert.That(context.UserAudits[1], Does.Contain("observed delivered duration seconds=3"));
            Assert.That(SegmentId(context.UserAudits[1]), Is.Not.EqualTo(SegmentId(context.UserAudits[0])));
        });
    }

    [Test]
    public async Task RejectedResume_WithPreviousDeliveredDuration_DoesNotRecordAnotherDelivery()
    {
        using var context = new DeliveryContext();
        context.CurrentEmission = new GcbOperationalPoint
        {
            TotalPointTime = 5,
            RemainingPointTime = 3,
            InitialRemainingPointTime = 3
        };
        context.Deliver = () => Task.FromException(new InvalidOperationException("Start rejected"));

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Is.Empty);
    }

    [Test]
    public async Task StartAcknowledgedWithoutEmission_DoesNotRecordDelivery()
    {
        using var context = new DeliveryContext();
        context.Deliver = () => Task.CompletedTask;

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Is.Empty);
    }

    [Test]
    public async Task PersistenceRetryAndRepeatedTelemetry_DoNotDuplicateDelivery()
    {
        using var context = new DeliveryContext();
        context.TreatmentModel.SetupSequence(value => value.SaveTreatmentData())
            .ThrowsAsync(new IOException("Store unavailable"))
            .ReturnsAsync(context.Treatment);
        context.PopUpService.Setup(value => value.YesCancelDialog(
            It.IsAny<string>(), It.IsAny<string>(), "Yes", "Cancel", DialogBoxIconType.Choice))
            .Returns(DialogBoxResult.Yes);
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 2);
            context.Publish(GcbStateNew.Emission, 2);
            context.Publish(GcbStateNew.Emission, 5);
            context.Publish(GcbStateNew.Emission, 5);
            context.Publish(GcbStateNew.Cold);
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();
        context.Publish(GcbStateNew.Emission, 5);
        context.Publish(GcbStateNew.Cold);

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.That(context.UserAudits[0], Does.Contain("observed delivered duration seconds=5"));
    }

    [Test]
    public async Task ActorAndPatientChangedDuringPersistence_RetainsInitiatingIdentity()
    {
        using var context = new DeliveryContext();
        context.TreatmentModel.Setup(value => value.SaveTreatmentData()).Returns(async () =>
        {
            await Task.Yield();
            context.User.SetupGet(value => value.Id).Returns(99);
            context.User.SetupGet(value => value.Username).Returns("replacement");
            context.UserStore.SetupGet(value => value.AuthorizedUser).Returns((IUser?)null);
            context.Diagnosis.PatientId = 222;
            context.Plan.Id = 333;
            context.Field.Id = 555;
            return context.Treatment;
        });
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 5);
            context.Publish(GcbStateNew.Cold);
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(context.UserAudits[0], Does.Contain("by clinician (user id=11)"));
            Assert.That(context.UserAudits[0], Does.Contain("patient id=22; plan id=33"));
            Assert.That(context.UserAudits[0], Does.Contain("field id=55"));
            Assert.That(context.UserAudits[0], Does.Not.Contain("replacement"));
        });
    }

    [Test]
    public async Task HardwareFaultCancelsDelivery_RecordsFailedRatherThanUserInterruption()
    {
        using var context = new DeliveryContext();
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 2);
            context.Publish(GcbStateNew.Fault);
            return Task.FromException(new TaskCanceledException());
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.That(context.UserAudits[0], Does.Contain("outcome=failed"));
    }

    [Test]
    public async Task ConnectionLostAfterEmission_RecordsLastObservedDurationWithoutInventingCompletion()
    {
        using var context = new DeliveryContext();
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 2);
            context.CurrentEmission = null;
            context.Events.GetEvent<SystemTelemetryChangedEvent>().Publish(null);
            return Task.FromException(new IOException("Board unavailable"));
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(context.UserAudits[0], Does.Contain("outcome=failed"));
            Assert.That(context.UserAudits[0], Does.Contain("observed delivered duration seconds=2"));
        });
    }

    [Test]
    public async Task ShortDeliveryWithoutIntermediateTelemetry_UsesFinalHardwareDuration()
    {
        using var context = new DeliveryContext();
        context.Deliver = () =>
        {
            context.CurrentEmission = new GcbOperationalPoint { TotalPointTime = 5, RemainingPointTime = 0 };
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.That(context.UserAudits[0], Does.Contain("observed delivered duration seconds=5"));
    }

    [Test]
    public async Task AuditWriterFails_HardwareCleanupStillFinishes()
    {
        using var context = new DeliveryContext();
        context.ConfirmCleanup = true;
        context.LogWriter.Setup(value => value.LogAsync(
            It.IsAny<string>(), LogRecordSeverity.Info, LogRecordType.User))
            .ThrowsAsync(new IOException("Audit unavailable"));
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 5);
            context.Publish(GcbStateNew.Cold);
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();

        Assert.Multiple(() =>
        {
            Assert.That(context.HardwareCalls, Is.EqualTo(new[] { "BeamOn", "ResetTimers", "ClearPlan" }));
            Assert.That(context.UiState, Is.EqualTo(UIMacroState.StandBy));
            Assert.That(context.CurrentEmission, Is.Null);
            Assert.That(context.SystemErrors, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task CleanupFailsAfterCompleteDelivery_DoesNotRewriteDeliveryAsFailure()
    {
        using var context = new DeliveryContext();
        context.ConfirmCleanup = true;
        context.MainBoard.Setup(value => value.ClearPlan()).ThrowsAsync(new InvalidOperationException("Clear failed"));
        context.Deliver = () =>
        {
            context.Publish(GcbStateNew.Emission, 5);
            context.Publish(GcbStateNew.Cold);
            return Task.CompletedTask;
        };

        await context.ViewModel.Deliver();

        Assert.That(context.UserAudits, Has.Count.EqualTo(1));
        Assert.That(context.UserAudits[0], Does.Contain("outcome=completed"));
    }

    private static string SegmentId(string message) =>
        message.Split("segment id=")[1].Split(';')[0];

    private sealed class DeliveryContext : IDisposable
    {
        private readonly CancellationTokenSource _lifetime = new();

        public DeliveryContext()
        {
            Events = new EventAggregator();
            var dataStore = new Mock<IGCBDataStore>();
            dataStore.SetupGet(value => value.SystemTelemetry).Returns(Mock.Of<ISystemTelemetry>());
            var planModel = new Mock<OutdoorPlanModel>();
            planModel.SetupGet(value => value.Plan).Returns(Plan);
            planModel.SetupGet(value => value.Diagnosis).Returns(Diagnosis);
            planModel.SetupGet(value => value.TreatmentFields)
                .Returns(new TreatmentFieldEntryObservableCollection([Field]));
            planModel.SetupGet(value => value.TotalDuration).Returns(5);
            planModel.Setup(value => value.UpdateActualTime(It.IsAny<GcbOperationalPoint>()))
                .Callback<GcbOperationalPoint>(point => Field.Actual = point.ActualDuration);
            TreatmentModel.SetupGet(value => value.Treatment).Returns(Treatment);
            TreatmentModel.Setup(value => value.SaveTreatmentData()).ReturnsAsync(Treatment);
            User.SetupGet(value => value.Id).Returns(11);
            User.SetupGet(value => value.Username).Returns("clinician");
            UserStore.SetupGet(value => value.AuthorizedUser).Returns(User.Object);
            LogWriter.Setup(value => value.LogAsync(
                    It.IsAny<string>(), It.IsAny<LogRecordSeverity>(), It.IsAny<LogRecordType>()))
                .Callback<string, LogRecordSeverity, LogRecordType>((message, severity, type) =>
                {
                    if (type == LogRecordType.User)
                        UserAudits.Add(message);
                    if (type == LogRecordType.System && severity == LogRecordSeverity.Error)
                        SystemErrors.Add(message);
                }).Returns(Task.CompletedTask);
            var ui = new Mock<IUIStateMachine>();
            ui.SetupGet(value => value.State).Returns(() => UiState);
            ui.SetupGet(value => value.LeftButton).Returns(new LeftButtonInfo());
            ui.SetupGet(value => value.CentralButton).Returns(new CentralButtonInfo());
            ui.SetupGet(value => value.RightButton).Returns(new RightButtonInfo());
            ui.Setup(value => value.RequestStateSwitch(It.IsAny<UIMacroState>()))
                .Callback<UIMacroState>(state => UiState = state);
            MainBoard.SetupGet(value => value.CurrentEmission).Returns(() => CurrentEmission);
            MainBoard.Setup(value => value.BeamOn()).Returns(() =>
            {
                HardwareCalls.Add("BeamOn");
                if (CurrentEmission is { } point)
                {
                    point.InitialRemainingPointTime = point.RemainingPointTime;
                    CurrentEmission = point;
                }
                return Deliver();
            });
            MainBoard.Setup(value => value.ResetTimers()).Callback(() => HardwareCalls.Add("ResetTimers"))
                .Returns(Task.CompletedTask);
            MainBoard.Setup(value => value.ClearPlan()).Callback(() =>
            {
                HardwareCalls.Add("ClearPlan");
                CurrentEmission = null;
            }).Returns(Task.CompletedTask);
            var dialogs = new Mock<IDialogService>();
            dialogs.Setup(value => value.ShowDialog(It.IsAny<string>(), It.IsAny<IDialogParameters>(),
                    It.IsAny<Action<IDialogResult>>()))
                .Callback<string, IDialogParameters, Action<IDialogResult>>((_, _, callback) =>
                {
                    OnConfirmation?.Invoke();
                    callback(new DialogResult(ConfirmCleanup ? ButtonResult.OK : ButtonResult.Cancel));
                });
            var globals = Mock.Of<IAppGlobals>(value => value.AppCancellationTokenSource == _lifetime);
            ViewModel = new DeliveryHarness(Events, globals, dataStore.Object, planModel.Object,
                TreatmentModel.Object, UserStore.Object, LogWriter.Object, ui.Object,
                MainBoard.Object, PopUpService.Object, dialogs.Object);
        }

        public Mock<IMainBoardModel> MainBoard { get; } = new();
        public Mock<ITreatmentModel> TreatmentModel { get; } = new();
        public Mock<IAuthorizedUserStore> UserStore { get; } = new();
        public Mock<IUser> User { get; } = new();
        public Mock<ILogWriter> LogWriter { get; } = new();
        public Mock<IPopUpService> PopUpService { get; } = new();
        public Plan Plan { get; } = new() { Id = 33 };
        public Diagnosis Diagnosis { get; } = new() { Id = 66, PatientId = 22 };
        public Treatment Treatment { get; } = new() { Id = 44, PlanId = 33 };
        public TreatmentFieldEntry Field { get; } = new(new TreatmentField
        {
            Id = 55, PlanId = 33, Name = TreatmentFieldName.PlusC, DwellTime = 5, Energy = Energy.Energy_100
        }, 0);
        public IEventAggregator Events { get; }
        public DeliveryHarness ViewModel { get; }
        public Func<Task> Deliver { get; set; } = () => Task.CompletedTask;
        public GcbOperationalPoint? CurrentEmission { get; set; } = new GcbOperationalPoint
        {
            TotalPointTime = 5, RemainingPointTime = 5, InitialRemainingPointTime = 5
        };
        public UIMacroState UiState { get; private set; } = UIMacroState.Preparation;
        public bool ConfirmCleanup { get; set; }
        public Action? OnConfirmation { get; set; }
        public List<string> HardwareCalls { get; } = [];
        public List<string> UserAudits { get; } = [];
        public List<string> SystemErrors { get; } = [];

        public void Publish(GcbStateNew state, float elapsed = 0)
        {
            if (state is GcbStateNew.Emission or GcbStateNew.Termination && CurrentEmission is { } point)
            {
                point.RemainingPointTime = point.InitialRemainingPointTime - elapsed;
                CurrentEmission = point;
            }
            Events.GetEvent<SystemTelemetryChangedEvent>().Publish(Mock.Of<ISystemTelemetry>(
                value => value.ControlBoardState == state && value.PrimaryTimerValue == elapsed));
        }

        public void Dispose()
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        }
    }


    private sealed class DeliveryHarness : TreatmentViewModel
    {
        public DeliveryHarness(IEventAggregator events, IAppGlobals globals, IGCBDataStore dataStore,
            OutdoorPlanModel plan, ITreatmentModel treatment, IAuthorizedUserStore userStore,
            ILogWriter log, IUIStateMachine ui, IMainBoardModel board, IPopUpService popup,
            IDialogService dialogs)
            : base(Mock.Of<IRegionManager>(), dialogs, Mock.Of<IHeraclesExternalSettings>(), globals,
                events, log, dataStore, plan,
                new LoadForTreatmentEventSource(Mock.Of<ILoadForTreatmentEventStream>(), CancellationToken.None),
                new PlanEventSource(Mock.Of<IPlanEventStream>(), CancellationToken.None),
                board, Mock.Of<IGcbIndicators>(), Mock.Of<ICollimatorConfigurationStore>(), treatment,
                Mock.Of<IActualTreatmentFieldModel>(), Mock.Of<ITreatmentDoseCalculation>(),
                null!, null!, new ApplicatorCompatibilityService(Mock.Of<ICollimatorModel>()),
                Mock.Of<IWarmupService>(), ui, Mock.Of<ICollimatorModel>(), popup,
                Mock.Of<ISafetyCheckModel>(), Mock.Of<IBearerTokenUserSessionManager>(), userStore)
        {
        }

        public Task Deliver() => OnBeamOnClicked();

        // The board mock supplies settled hardware evidence; keep dispatch and clinical writes out of the fixture.
        protected override Task UpdateAfterEmission(CancellationToken token) => Task.CompletedTask;
        protected override Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry) => Task.CompletedTask;
        protected override Task SetPlanUnloadTaskAsync() => Task.CompletedTask;
        protected override void CheckForGcbStateChanged(GcbStateNew? state)
        {
            PreviousGcbState = GcbState;
            GcbState = state;
        }
    }
}
