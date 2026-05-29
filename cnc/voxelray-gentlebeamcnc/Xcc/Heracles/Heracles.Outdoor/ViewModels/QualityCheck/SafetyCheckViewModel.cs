using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Enums;
using Heracles.Application.Helpers;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.QualityCheck;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.External.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;
using SafetyCheckConstants = Heracles.Application.Common.StringConstants.TreatmentConsole.SafetyCheck;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class SafetyCheckViewModel : OperatePlanViewModelBase, ILoadAware
    {
        public static string ApprovePhysicsDataMessage => Application.Common.StringConstants.TreatmentConsole.ApprovePhysicsDataMessage;

        #region Contructors
        public SafetyCheckViewModel() : base()
        { }

        public SafetyCheckViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IHeraclesExternalSettings heraclesExternalSettings,
            IGCBDataStore gcbDataStore,
            ILogWriter logWriter,
            IUIStateMachine uiStateMachine,
            IDialogService dialogService,
            IWarmupService warmUpService,
            ICollimatorModel collimatorModel,
            IPopUpService popUpService,
            IMainBoardModel mainBoardModel,
            IGcbIndicators gcbIndicators,
            IAuthorizedUserStore userStore,
            ISafetyCheckModel safetyCheckModel,
            ICollimatorConfigurationStore collimatorConfigurationStore,
            ApplicatorCompatibilityService applicatorCompatibilityService,
            IActionAuditService actionAuditService,
            IBearerTokenUserSessionManager userSessionManager)
            : base(regionManager, eventAggregator, heraclesExternalSettings,
                gcbDataStore, uiStateMachine, logWriter, warmUpService,
                popUpService, dialogService, mainBoardModel, gcbIndicators,
                collimatorModel, collimatorConfigurationStore, 
                actionAuditService, safetyCheckModel, userSessionManager)
        {
            UserStore = userStore;
            ApplicatorCompatibilityService = applicatorCompatibilityService;
            CommonProperties = new CommonProperties(eventAggregator);

            SafetyCheckModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Models.ISafetyCheckModel.Fields))
                {
                    FieldsViewSource.Source = SafetyCheckModel.Fields;
                }
            };

            this.PropertyChanged += (s,e) =>
            {
                if (e.PropertyName == nameof(ActualCollimatorConfiguration))
                {
                    OnActualCollimatorConfigurationChanged(ActualCollimatorConfiguration);
                }
            };

        }
        #endregion Contructors


        #region Properties        

        public CommonProperties CommonProperties { get; }

        public IAuthorizedUserStore UserStore { get; }
        public ApplicatorCompatibilityService ApplicatorCompatibilityService { get; }

        private CollectionViewSource _fieldsViewSource = new();
        public CollectionViewSource FieldsViewSource
        {
            get => _fieldsViewSource;
            set => SetProperty(ref _fieldsViewSource, value);
        }

        private Core.Models.EMR.ITreatmentField _honeycombSelection;
        public Core.Models.EMR.ITreatmentField HoneycombSelection
        {
            get => _honeycombSelection;
            set => SetProperty(ref _honeycombSelection, value);
        }

        private Energy _energy;
        public Energy Energy
        {
            get => _energy;
            set
            {
                if (SetProperty(ref _energy, value))
                {
                    _ = OnEnergyChanged(value);

                    //TODO: calculate dose

                }
            }
        }

        private double _dwellTime;
        public double DwellTime
        {
            get => _dwellTime;
            set
            {
                if (SetProperty(ref _dwellTime, value))
                {
                    //TODO: calculate dose
                }
            }
        }

        private double _calculatedDose;
        public double CalculatedDose
        {
            get => _calculatedDose;
            set => SetProperty(ref _calculatedDose, value);
        }

        private bool _prepareButtonIsEnabled = false;
        public bool PrepareButtonIsEnabled
        {
            get { return _prepareButtonIsEnabled; }
            set { SetProperty(ref _prepareButtonIsEnabled, value); }
        }

        private bool? _xrayLight = null!;
        public bool? XrayLight
        {
            get { return _xrayLight; }
            set
            {
                if (SetProperty(ref _xrayLight, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _xraySound = null!;
        public bool? XraySound
        {
            get { return _xraySound; }
            set
            {
                if (SetProperty(ref _xraySound, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _doorInterlock = null!;
        public bool? DoorInterlock
        {
            get { return _doorInterlock; }
            set
            {
                if (SetProperty(ref _doorInterlock, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _eStopInterlock = null!;
        public bool? EStopInterlock
        {
            get { return _eStopInterlock; }
            set
            {
                if (SetProperty(ref _eStopInterlock, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _stopEmission = null!;
        public bool? StopEmission
        {
            get { return _stopEmission; }
            set
            {
                if (SetProperty(ref _stopEmission, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _monitoringCamera = null!;
        public bool? MonitoringCamera
        {
            get { return _monitoringCamera; }
            set
            {
                if (SetProperty(ref _monitoringCamera, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool? _liveAudio = null!;
        public bool? LiveAudio
        {
            get { return _liveAudio; }
            set
            {
                if (SetProperty(ref _liveAudio, value))
                    SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _approveRequired;
        public bool ApproveRequired
        {
            get => _approveRequired;
            private set => SetProperty(ref _approveRequired, value);
        }
        #endregion Properties

        #region Commands
        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            () =>
            {
                _ = OnSave();
            }, canExecuteMethod: CanSave);

        #endregion Commands

        #region private methods

        protected override void UserActionAudit(string actionMessage)
        {
            actionMessage = $"SafetyCheck: {actionMessage}";

            base.UserActionAudit(actionMessage);
        }

        private void OnCollimatorModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ICollimatorModel.ActiveCollimator))
            {
                OnActiveCollimatorChanged();
            }
        }

        private void OnActiveCollimatorChanged()
        {
            _ = Task.Run(async () =>
            {
                await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    var activeCollimator = CollimatorModel.ActiveCollimator;
                    if (activeCollimator == null || ActualCollimatorConfiguration == null)
                    {
                        ClearEntryCollection();
                    }
                    else
                    {
                        await FetchCoilConfigurationsAsync();
                        CreateEntryCollection();
                    }

                    CheckForApplicatorCompatibility();
                });
            });
        }

        private void ClearEntryCollection()
        {
            SafetyCheckModel.Fields?.Clear();
        }

        private async Task FetchCoilConfigurationsAsync()
        {
            var configuration = CollimatorModel.ActiveCollimator?.Configuration;

            if (configuration == null)
                return;

            try
            {
                await CollimatorConfigurationStore.FetchConfigurationAsync(configuration.Energy, configuration.Type);

                if (CollimatorConfigurationStore.HeaterCurrent == null)
                    await UpdateCollimatorPreset(configuration.Energy, configuration.Type);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.ApplicatorCoilConfigurationLoadError,
                    ex);
            }

            ValidateCanExecuteCommands();
        }

        protected override bool CanPrepare()
        {
            if (UIStateMachine.LeftButton == null)
            {
                PrepareButtonIsEnabled = false;
                return false;
            }

            PrepareButtonIsEnabled = UIStateMachine.LeftButton.IsEnabled || HasValidPlanForTreatment();

            return SafetyCheckModel.SafetyCheck != null
                && SafetyCheckModel.Fields != null
                && SafetyCheckModel.Fields.Count > 0
                && base.CanPrepare();
        }

        protected override async Task PrepareAsync(bool tryKeepPrevPlan)
        {
            var externalTabName = ExternalTabName.QA;

            try
            {
                await base.PrepareAsync(tryKeepPrevPlan: false); // don't keep any prev plan in GCB

                UIStateMachine.IsPlanLoadedForTreatment = true;
                UIStateMachine.TabName = externalTabName;
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                    StringConstants.TreatmentConsole.PlanPreparationErrorMessage,
                    ex);
            }
            finally
            {
                SwitchExternalTab(externalTabName);
                EventAggregator?.GetEvent<RequestQaTabChangeEvent>().Publish(QaTabName.SafetyChecks);
            }
        }

        protected override Task<bool> PreventUserFromPrepare()
        {
            return Task.FromResult(false);
        }

        protected override GcbEmissionPlan BuildGcbEmissionPlan()
        {
            var coilConfigurations = CollimatorConfigurationStore.CoilConfigurations;
            double? heaterCurrent = CollimatorConfigurationStore.HeaterCurrent.HeaterCurrent;
            if (heaterCurrent is null)
            {
                throw new NullReferenceException("Invalid applicator configuration, heater current value is missing");
            }

            GcbEmissionPlan plan = new();

            foreach (var field in SafetyCheckModel.Fields)
            {
                var coilSetpoints = coilConfigurations.FirstOrDefault(c => c.FieldName == field.Name);
                if (coilSetpoints is null)
                {
                    throw new NullReferenceException("Invalid applicator configuration, coil configuration data is missing");
                }

                float totalTime = (float)field.Duration;
                float remainingTime = totalTime - (float)field.Actual;

                GcbOperationalPoint op = new GcbOperationalPoint
                {
                    PointIndex = plan.TotalPoints,
                    TotalPointTime = totalTime,
                    RemainingPointTime = remainingTime,
                    SetpointKv = EnergyConverter.Convert(field.Energy),
                    TargetMA = Convert.ToSingle(field.Current),

                    XCoilSetpoint = Convert.ToSingle(coilSetpoints.XDeflectionCurrent),
                    YCoilSetpoint = Convert.ToSingle(coilSetpoints.YDeflectionCurrent),
                    FocusCoilSetpoint = Convert.ToSingle(coilSetpoints.FocusCurrent),
                    FilamentSetpoint = (float)heaterCurrent.Value,
                    AutoExecution = GetPlanAutoExecutionFlag()
                };

                plan.AddPoint(op);
            }
            return plan;
        }

        protected override void RecalculateInitialXrayTime()
        {
            var fields = SafetyCheckModel.Fields;
            if (fields is null || fields.Count == 0)
                XrayTime = 0.0;
            else
                XrayTime = fields.Sum(tf => tf.Actual);
        }

        protected override void CheckForApplicatorCompatibility()
        {
            ApplicatorCompatibilityStatus = ApplicatorCompatibilityService.Check(SafetyCheck.SupportedEnergy);

            HasMatchingPlanForTreatment = HasValidPlanForTreatment();

            CheckForApprovedCalibrationData();

            ValidateCanExecuteCommands();
        }

        protected override async Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry)
        {
            try
            {
                await Semaphore.WaitAsync();

                if (GcbState == GcbStateNew.Emission ||
                    PreviousGcbState == GcbStateNew.Emission)
                {
                    int operationalPointIndex = telemetry.CurrentOperationalPoint;
                    float timerValue = telemetry.PrimaryTimerValue;

                    if (operationalPointIndex != PreviousOperationPointIndex)
                    {
                        var fields = SafetyCheckModel.Fields;
                        if (PreviousOperationPointIndex >= 0 && fields is not null && fields.Count > PreviousOperationPointIndex)
                        {
                            var previousTf = fields[PreviousOperationPointIndex];
                            previousTf.Actual = Convert.ToSingle(previousTf.Duration);
                            RecalculateInitialXrayTime();
                            XrayPointStartTime = 0;

                            var previousPoint = await MainBoardModel.QueryPointFromGCB(PreviousOperationPointIndex);

                            if (previousPoint.RemainingPointTime > 0)
                            {
                                previousTf.Actual = previousPoint.TotalPointTime - previousPoint.RemainingPointTime;
                                _ = LogWriter.LogAsync($"Query point response: TotalPointTime={previousPoint.TotalPointTime} RemainingPointTime={previousPoint.RemainingPointTime} Actual={previousTf.Actual}", LogRecordSeverity.Info, LogRecordType.System);
                            }
                        }
                    }

                    if (operationalPointIndex < SafetyCheckModel.Fields?.Count)
                    {
                        var tf = SafetyCheckModel.Fields[operationalPointIndex];

                        if (operationalPointIndex != PreviousOperationPointIndex)
                        {
                            PreviousOperationPointIndex = operationalPointIndex;
                            XrayPointStartTime = tf.Actual;
                        }

                        UpdateBeamOnProgress(SafetyCheckModel.TotalDuration, Convert.ToSingle(XrayTime + timerValue));

                        tf.Actual = Convert.ToSingle(XrayPointStartTime + timerValue);
                    }
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        protected override async Task OnClearPlanClicked()
        {
            SelectCurrentField(null);
            await base.OnClearPlanClicked();

            ValidateCanExecuteCommands();
        }

        protected override async Task OnBeamOnClicked()
        {
            IsCurrentViewModelRunning = true;

            using var tokenSource = new CancellationTokenSource();
            Task updateAfterEmissionTask = Task.CompletedTask;
            try
            {
                var telemetry = GCBDataStore.SystemTelemetry ?? throw new Exception("GCB telemetry connection lost.");

                // Store what was the current point before emission
                PreviousOperationPointIndex = telemetry.CurrentOperationalPoint;

                // Store initial emission time of the current point to calc progress over the plan
                RecalculateInitialXrayTime();

                XrayPointStartTime = (PreviousOperationPointIndex < SafetyCheckModel.Fields.Count)
                    ? SafetyCheckModel.Fields[PreviousOperationPointIndex].Actual
                            : 0.0;

                UpdateBeamOnProgress(SafetyCheckModel.TotalDuration, Convert.ToSingle(XrayTime));

                UIStateMachine.RequestStateSwitch(UIMacroState.Emission);
                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled}), " +
                    $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                _ = LogWriter.LogAsync($"Run Safety check by {UserStore.AuthorizedUser.EmailAddress}", LogRecordSeverity.Info, LogRecordType.User);

                // Highlight emitting cell
                SelectCurrentField(SafetyCheckModel.Fields?.FirstOrDefault());

                updateAfterEmissionTask = Task.Run(() => UpdateAfterEmission(tokenSource.Token), tokenSource.Token);

                // Run emission and wait until it gets done or gets stopped:
                await MainBoardModel.BeamOn();

                await updateAfterEmissionTask;

                if (GetUserCleanupConfirmation(
                    SafetyCheckConstants.CompletionConfirmationTitle,
                    SafetyCheckConstants.CompletionConfirmationMessage))
                {
                    await MainBoardModel.ResetTimers();
                    await MainBoardModel.ClearPlan();
                    UIStateMachine.IsPlanStaged = false;
                    UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                    Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled})" +
                        $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                    await SetPlanUnloadTaskAsync();
                }

                IsCurrentViewModelRunning = false;
            }
            catch (TaskCanceledException ex)
            {
                IsCurrentViewModelRunning = false;

                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);

                PopUpService.ShowMessage(
                    StringConstants.TreatmentConsole.EmissionTitle,
                    StringConstants.TreatmentConsole.EmissionInterruptedError,
                    ReportType.Error);

                _ = LogWriter.LogAsync($"Safety check execution was cancelled: {ex.Message}", LogRecordSeverity.Info, LogRecordType.System);
            }
            catch (InvalidOperationException ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.SafetyCheck.ErrorTitle,
                    ex.Message);
            }
            catch (Exception ex)
            {
                IsCurrentViewModelRunning = false;
                PopUpService.LogAndShowError(
                    SafetyCheckConstants.ErrorTitle,
                    SafetyCheckConstants.StartErrorMessage,
                    ex);
            }
            finally
            {
                await tokenSource.CancelAsync();

                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);

                SelectCurrentField(null);
            }
        }

        private void ResetAllIndicators()
        {
            XrayLight = null;
            XraySound = null;
            DoorInterlock = null;
            EStopInterlock = null;
            StopEmission = null;
            MonitoringCamera = null;
            LiveAudio = null;
        }

        protected override Task SetPlanUnloadTaskAsync()
        {
            UIStateMachine.IsPlanLoadedForTreatment = false;

            CreateEntryCollection();
            return Task.CompletedTask;
        }

        protected override void ValidateCanExecuteCommands()
        {
            base.ValidateCanExecuteCommands();

            SaveCommand.RaiseCanExecuteChanged();
        }

        private async Task OnEnergyChanged(Energy energy)
        {
            try
            {
                if (CollimatorConfigurationStore.HeaterCurrent == null)
                    await UpdateCollimatorPreset(energy, CollimatorModel.ActiveCollimator.Configuration.Type);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.ApplicatorCoilConfigurationLoadError,
                    ex);
            }
        }

        private bool HasValidPlanForTreatment()
        {
            return SafetyCheckModel.SafetyCheck != null &&
                   ApplicatorCompatibilityStatus.IsCompatible;
        }

        private void CreateEntryCollection()
        {
            try
            {
                if (ActualCollimatorConfiguration?.Energy != SafetyCheck.SupportedEnergy ||
                    ApproveRequired)
                {
                    return;
                }

                if (SafetyCheckModel.SafetyCheck == null)
                {
                    SafetyCheckModel.CreateBlank();
                }

                SafetyCheckModel.CreateEntryCollection();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    SafetyCheckConstants.CreatePlanErrorMessage,
                    ex);
            }

            ValidateCanExecuteCommands();
        }

        private bool AreIndicatorsSet()
        {
            return XrayLight != null
                && XraySound != null
                && DoorInterlock != null
                && EStopInterlock != null
                && StopEmission != null
                && MonitoringCamera != null
                && LiveAudio != null;
        }

        private bool CanSave()
        {
            bool inEmission = UIStateMachine.State != UIMacroState.StandBy;
            bool blockedInEmission = (inEmission && MainBoardModel.State != GcbStateNew.Ready && MainBoardModel.State != GcbStateNew.Staged);
            return !IsPreparing && !blockedInEmission && AreIndicatorsSet();
        }

        private async Task OnSave()
        {
            try
            {
                SafetyCheckModel.SafetyCheck.XRayLight = XrayLight.Value;
                SafetyCheckModel.SafetyCheck.XRaySound = XraySound.Value;
                SafetyCheckModel.SafetyCheck.DoorInterlock = DoorInterlock.Value;
                SafetyCheckModel.SafetyCheck.EStop = EStopInterlock.Value;
                SafetyCheckModel.SafetyCheck.SStop = StopEmission.Value;
                SafetyCheckModel.SafetyCheck.LiveVideo = MonitoringCamera.Value;
                SafetyCheckModel.SafetyCheck.LiveAudio = LiveAudio.Value;

                await SafetyCheckModel.SaveAsync();
                ResetAllIndicators();

                // TODO: do we want to clear the plan after saving the report?
                await OnClearPlanClicked();

                EventAggregator.GetEvent<SafetyCheckSavedEvent>().Publish();
            }
            catch (Exception ex) when (ex.InnerException is SessionAuthorizationException exInner)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    exInner.Message);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    SafetyCheckConstants.SaveDataErrorMessage,
                    ex);
            }
        }

        // TODO: almost the same as in TreatmentViewModel, consider refactoring this
        private void SelectCurrentField(IFieldEntryBase field)
        {
            SafetyCheckModel.SelectedField = field;

            var collection = Application.Models.RDBMS.EMR.TreatmentField.GetTreatmentFieldCollection(SafetyCheckModel.CollimatorType);
            HoneycombSelection = (field is null) ? null : collection?.FirstOrDefault(item => item.Name.Equals(field.Name));
        }

        private void OnActualCollimatorConfigurationChanged(ICollimatorConfiguration? actualCollimatorConfiguration)
        {
            CheckForApprovedCalibrationData();
        }

        private void CheckForApprovedCalibrationData()
        {
            var defaultPreset = ActualCollimatorConfiguration?.DefaultPreset;
            if (defaultPreset is null ||
                defaultPreset.IsApproved)
            {
                ApproveRequired = false;
            }
            else
                ApproveRequired = true;
        }

        #endregion

        #region ILoadAware
        public override void VisiblyLoaded()
        {
            base.VisiblyLoaded();

            CollimatorModel.PropertyChanged += OnCollimatorModelChanged;

            OnActiveCollimatorChanged();
            
            //// This should be called on ActualCollimatorConfiguration,
            //// but if it was and still is null, it won't, so we need to call the check manually
            //CheckForApplicatorCompatibility();
        }

        public override void Unloaded()
        { 
            base.Unloaded();

            CollimatorModel.PropertyChanged -= OnCollimatorModelChanged;
        }
        #endregion
    }
}
