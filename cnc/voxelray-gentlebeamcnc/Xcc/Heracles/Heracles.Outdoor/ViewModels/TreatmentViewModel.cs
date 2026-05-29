using Empyrean.Common.Infra.Threading;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Enums;
using Heracles.Application.Helpers;
using Heracles.Application.Models;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.EMR;
using Heracles.Core.Commands;
using Heracles.Core.Constants;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Heracles.External.AppLayer.Treatment;
using Heracles.External.AppServices;
using Heracles.External.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Events;
using Xcc.Application.Helpers;
using Xcc.Application.Helpers.Threading;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.External.ViewModels
{
    public class SafeTaskExecutor(IAppGlobals appGlobals, ILogWriter logWriter)
    {
        private readonly TaskSequenceExecutor _executor = new TaskSequenceExecutor();

        public void Run()
        {
            _executor.Execute(appGlobals.AppCancellationTokenSource.Token);
        }

        public void Stop()
        {
            _executor.Complete();
        }

        public void ScheduleSafeTask(Func<Task> task)
        {
            _executor.Enqueue(async () =>
            {
                try 
                {
                    await task(); 
                }
                catch (Exception ex)
                {
                    _ = logWriter.LogAsync(ex.Message, LogRecordSeverity.Error, LogRecordType.System);
                }
            });
        }
    }

    public class TreatmentViewModel : OperatePlanViewModelBase
    {
        public static string ApprovePhysicsDataMessage => Application.Common.StringConstants.TreatmentConsole.ApprovePhysicsDataMessage;
        
        public TreatmentViewModel(
            IRegionManager regionManager,
            IDialogService dialogService,
            IHeraclesExternalSettings heraclesExternalSettings,
            IAppGlobals appGlobals,
            IEventAggregator eventAggregator,
            ILogWriter logWriter,
            IGCBDataStore gcbDataStore,
            Models.IPlanModel planForTreatmentModel,
            LoadForTreatmentEventSource loadForTreatmentEventSource,
            PlanEventSource planEventSource,
            IMainBoardModel mainBoardModel,
            IGcbIndicators gcbIndicators,
            ICollimatorConfigurationStore collimatorConfigurationStore,
            ITreatmentModel treatmentModel,
            IActualTreatmentFieldModel actualTreatmentFieldModel,
            ITreatmentDoseCalculation treatmentDoseCalculation,
            TreatmentPreparationService treatmentPreparationService,
            QcTreatmentAcceptanceService qcTreatmentAcceptanceService,
            ApplicatorCompatibilityService applicatorCompatibilityService,
            IWarmupService warmupService,
            IUIStateMachine uiStateMachine,
            ICollimatorModel collimatorModel,
            IPopUpService popUpService,
            IActionAuditService actionAuditService,
            ISafetyCheckModel safetyCheckModel,
            IBearerTokenUserSessionManager userSessionManager,
            IAuthorizedUserStore authorizedUserStore)
            : base(regionManager, eventAggregator, heraclesExternalSettings, gcbDataStore,
                  uiStateMachine, logWriter, warmupService, popUpService, dialogService,
                  mainBoardModel, gcbIndicators, collimatorModel,
                  collimatorConfigurationStore, actionAuditService, 
                  safetyCheckModel, userSessionManager)
        {
            CameraUriSource = heraclesExternalSettings.CameraUriSource;
            AppGlobals = appGlobals;
            PlanModel = planForTreatmentModel;
            TreatmentModel = treatmentModel;
            ActualTreatmentFieldModel = actualTreatmentFieldModel;
            TreatmentDoseCalculation = treatmentDoseCalculation;
            QcTreatmentAcceptanceService = qcTreatmentAcceptanceService;
            ApplicatorCompatibilityService = applicatorCompatibilityService;
            AuthorizedUserStore = authorizedUserStore;
            TreatmentPreparationService = treatmentPreparationService;
            eventAggregator.GetEvent<ExitApplicationEvent>().Subscribe(OnExit);

            //TreatmentFieldsViewSource.SortDescriptions.Add(new SortDescription("Data.Energy", ListSortDirection.Ascending));
            TreatmentFieldsViewSource.Filter += (s, e) =>
            {
                if (e.Item is not ITreatmentFieldEntry treatmentField)
                {
                    e.Accepted = false;
                    return;
                }

                e.Accepted = treatmentField.Energy > 0;
            };

            _safeTaskExecutor = new(appGlobals, logWriter);
            _safeTaskExecutor.Run();

            _noCommWatchdog = new(mainBoardModel, uiStateMachine);

            loadForTreatmentEventSource.LoadForTreatmentEvent += (s, e) => OnLoadForTreatmentEvent(e);
            planEventSource.PlanChangedEvent += (_, e) => OnPlanEvent(e);

            PlanModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Models.IPlanModel.TreatmentFields))
                {
                    TreatmentFieldsViewSource.Source = PlanModel.TreatmentFields;
                }
            };

            CommonProperties = new CommonProperties(eventAggregator);

            _ = FetchSafetyChecksAsync();
        }

        #region Constants
        private readonly string ACTUAL_FIELDS_DUMP_FILENAME = "ActualFieldsDump.csv";

        #endregion Constants

        #region Properties
        public CommonProperties CommonProperties { get; }

        public string CameraUriSource { get; set; }

        private CollectionViewSource _treatmentFieldViewSource = new();
        public CollectionViewSource TreatmentFieldsViewSource
        {
            get => _treatmentFieldViewSource;
            set => SetProperty(ref _treatmentFieldViewSource, value);
        }

        private bool _loadBusy = false;
        public bool LoadBusy
        {
            get { return _loadBusy; }
            set
            {
                SetProperty(ref _loadBusy, value);
                ValidateCanExecuteCommands();
            }
        }

        private bool _runBusy = false;
        public bool RunBusy
        {
            get { return _runBusy; }
            set
            {
                SetProperty(ref _runBusy, value);
                ValidateCanExecuteCommands();
            }
        }

        private bool _warmupFastBusy;
        public bool WarmupFastBusy
        {
            get { return _warmupFastBusy; }
            set
            {
                SetProperty(ref _warmupFastBusy, value);
                ValidateCanExecuteCommands();
            }
        }

        private bool _warmupFullBusy;
        public bool WarmupFullBusy
        {
            get { return _warmupFullBusy; }
            set
            {
                SetProperty(ref _warmupFullBusy, value);
                ValidateCanExecuteCommands();
            }
        }
        public IAppGlobals AppGlobals { get; }

        private ITreatmentField _honeycombSelection;
        public ITreatmentField HoneycombSelection { 
            get => _honeycombSelection; 
            set => SetProperty(ref _honeycombSelection, value); 
        }



        #endregion Properties

        #region Read-only properties
        public Models.IPlanModel PlanModel { get; }
        private ITreatmentModel TreatmentModel { get; }
        private IActualTreatmentFieldModel ActualTreatmentFieldModel { get; }
        public ITreatmentDoseCalculation TreatmentDoseCalculation { get; }
        public QcTreatmentAcceptanceService QcTreatmentAcceptanceService { get; }
        public ApplicatorCompatibilityService ApplicatorCompatibilityService { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public TreatmentPreparationService TreatmentPreparationService { get; }
        private TaskQueue ActualTreatmentFieldTaskQueue { get; set; }

        // TODO: remove this if not needed.
        // Now we must fail on an attempt to load a plan + its unapproved config
        //private bool _approveRequired;
        //public bool ApproveRequired
        //{
        //    get => _approveRequired;
        //    set
        //    {
        //        SetProperty(ref _approveRequired, value);

        //        //do not show message if applicator is not compatible, because the text will be overlapped
        //        DisplayApproveRequiredMessage = ApproveRequired && ApplicatorCompatibilityStatus.IsCompatible;
        //    }
        //}
        //private bool _displayApproveRequiredMessage;
        //public bool DisplayApproveRequiredMessage
        //{
        //    get => _displayApproveRequiredMessage;
        //    set => SetProperty(ref _displayApproveRequiredMessage, value);
        //}

        #endregion Read-only properties

        #region Fields
        private SafeTaskExecutor _safeTaskExecutor;
        private NoCommWatchdog _noCommWatchdog;

        #endregion Fields


        #region Commands        

        private DelegateCommand? _resetTimersDebug;
        public DelegateCommand ResetTimersDebugCommand => _resetTimersDebug ??= new DelegateCommand(
            async () =>
            {
                await MainBoardModel.ResetTimers();
            });

        //private DelegateCommand? _loadEmissionPlanCommand;
        //public DelegateCommand LoadEmissionPlanCommand => _loadEmissionPlanCommand ??= new DelegateCommand(
        //    async () =>
        //    {
        //        try
        //        {
        //            await MainBoardModel.Load();

        //            RecalculateInitialXrayTime();
        //        }
        //        catch (Exception ex)
        //        {
        //            PopUpService.LogAndShowError("Load", $"Failed to load the Plan", ex);
        //        }
        //    },
        //    canExecuteMethod: () => MainBoardModel.CanLoad());

        #endregion Commands        

        #region private methods  

        protected override void CheckForGcbStateChanged(GcbStateNew? state)
        {
            base.CheckForGcbStateChanged(state);

            if (PreviousGcbState != GcbState)
            {
                // log state change here to avoid duplicate logging from the base class
                _ = LogWriter.LogAsync($"OnGCBStateChanged: {state}", LogRecordSeverity.Info, LogRecordType.System);

                if (PreviousGcbState == GcbStateNew.Emission && state is null && !_noCommWatchdog.IsRunning)
                {
                    _noCommWatchdog.Start();
                }
                else if (state is not null && _noCommWatchdog.IsRunning)
                {
                    _noCommWatchdog.Stop();
                }
            }
        }
        
        /// <summary>
        /// calculates initial XRay time for the beamOn progress bar
        /// </summary>
        protected override void RecalculateInitialXrayTime()
        {
            var treatmentFields = PlanModel.TreatmentFields;
            if (treatmentFields is null || treatmentFields.Count == 0)
                //if (ActualTreatmentFieldModel.Collection == null || ActualTreatmentFieldModel.Collection.Count == 0)
                XrayTime = 0.0;
            else
                //_xrayTime = ActualTreatmentFieldModel.Collection.Sum(atf => atf.DwellTime);
                XrayTime = treatmentFields.Sum(tf => tf.Actual);
        }


        private void UpdatePlanActualTimeAndXrayTime()
        {
            // First get actual/remaining times from the board's current plan
            PlanModel.UpdateActualTime(MainBoardModel.CurrentPlan);
            RecalculateInitialXrayTime();
        }
        
        protected override void CheckForApplicatorCompatibility()
        {
            var requiredParameters = ApplicatorParameters.FromValues(
                PlanModel?.Plan?.CollimatorType, 
                PlanModel?.Prescription?.Energy);

            ApplicatorCompatibilityStatus = 
                (requiredParameters is null) 
                ? ApplicatorCompatibilityStatus.Compatible 
                : ApplicatorCompatibilityService.Check(requiredParameters.Value);

            HasMatchingPlanForTreatment = PlanModel?.Plan != null && ApplicatorCompatibilityStatus.IsCompatible;

            // TODO: we must fail earlier now, when we fetch and verify applicato data in the TreatmentPreparationService
            //CheckForApprovedCalibrationData();

            ValidateCanExecuteCommands();
        }

        // TODO: Remove if not needed
        // we must fail earlier now, when we fetch and verify applicato data in the TreatmentPreparationService
        //private void CheckForApprovedCalibrationData()
        //{
        //    var planCollimatorType = PlanModel?.Plan?.CollimatorType;
        //    var planEnergy = PlanModel?.Prescription?.Energy;
        //    var approvedConfig = CollimatorModel.CollimatorConfigurations?
        //        .FirstOrDefault(c =>
        //            c.Type == planCollimatorType &&
        //            c.Energy == planEnergy &&
        //            (c.DefaultPreset?.IsApproved ?? false));
        //    ApproveRequired = approvedConfig is null;
        //}

        protected override bool CanPrepare()
        {
            bool userHasPermission =
                AuthorizedUserStore.AuthorizedUser is not null &&
                AuthorizedUserStore.AuthorizedUser.Role.Permissions.Treatment;

            return PlanModel.Plan != null
                   && PlanModel.TreatmentFields is {Count: > 0}
                   && userHasPermission
                   && base.CanPrepare();
        }

        protected override async Task PrepareAsync(bool tryKeepPrevPlan)
        {
            try
            {
                UserActionAudit($"User triggered preparation Plan {PlanModel.Plan?.Id} for treatment");

                await base.PrepareAsync(
                    tryKeepPrevPlan: true); // We need to keep prev plan for treatment if there's one in GCB
                UpdatePlanActualTimeAndXrayTime();

                // We lock the plan as "Loaded for Treatment" anyway:
                // either it was loaded from scratch, or verified against what is on the board already.
                // TODO: it would be better to lock the plan as Loaded first
                // to avoid a race condition from Indoor making "Cancel Loading" simultaneously
                CurrentTaskCommand = new(
                    () => CurrentTask = new ObservableTask(
                        AcknowledgePlanForTreatment(), 
                        Application.Common.StringConstants.TreatmentConsole.AcknowledgePlanStatusErrorMessage));
                CurrentTaskCommand.Execute();
            }
            catch (Exception ex)
            {
                // This duplicates the catch in the base class, probably need to be removed,
                // but first we need to make sure that the Task Cancellation is handled properly there
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                    StringConstants.TreatmentConsole.PlanPreparationErrorMessage,
                    ex);
            }
            finally
            {
                SwitchExternalTab(ExternalTabName.Treatment);
            }
        }

        protected override Task SetPlanUnloadTaskAsync()
        {
            // TODO: we need to unload the plan from treatment only if it was the current one on the board.
            // Now we unload it unconditionally, this is wrong behavior for the Preparation state
            //if (UIStateMachine.State != UIMacroState.Preparation)
            CurrentTaskCommand = new(() =>
            {
                _safeTaskExecutor.ScheduleSafeTask(() =>
                { 
                    return System.Windows.Application.Current.Dispatcher.Invoke(async () => {
                        var unloadPlanTask = UnloadPlanFromTreatment();
                        CurrentTask = new ObservableTask(
                            unloadPlanTask,
                            Application.Common.StringConstants.TreatmentConsole.UnloadPlanErrorMessage);
                        try
                        {
                            await unloadPlanTask;
                        }
                        catch
                        {
                            // we don't want the exception to be logged second time from the executor
                        }
                    });
                });
            });
            CurrentTaskCommand.Execute();

            return Task.CompletedTask;
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
                UpdatePlanActualTimeAndXrayTime();

                XrayPointStartTime = (PreviousOperationPointIndex < PlanModel.TreatmentFields.Count)
                    ? PlanModel.TreatmentFields[PreviousOperationPointIndex].Actual
                    : 0.0;
                UpdateBeamOnProgress(Convert.ToSingle(PlanModel.TotalDuration), Convert.ToSingle(XrayTime));

                UIStateMachine.RequestStateSwitch(UIMacroState.Emission);
                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled}), " +
                                $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                // Make a queue of treatment field update requests and perform initial treatment setup:
                ActualTreatmentFieldTaskQueue = new TaskQueue();

                // TODO: save treatment should go to an observable task, to provide retry/cancel options
                while (true)
                {
                    try
                    {
                        await SaveTreatmentAsync();
                        break;
                    }
                    catch
                    {
                        if (PopUpService.YesCancelDialog(
                            StringConstants.Common.ErrorTitle,
                            StringConstants.TreatmentConsole.Treatment.TreatmentRetryUiMessage) == DialogBoxResult.Cancel)
                        {
                            // User decided to stop attempts to save anything to Moses
                            UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                            throw;
                        } // Otherwise we'll try again on the next iteration
                    }
                }

                ActualTreatmentFieldModel.StartCalculatingAverageEnergy();

                updateAfterEmissionTask = Task.Run(() => UpdateAfterEmission(tokenSource.Token), tokenSource.Token);

                // Run emission and wait until it gets done or gets stopped:
                await MainBoardModel.BeamOn();

                await updateAfterEmissionTask;

                // Check if plan is actually complete:
                if (IsPlanCompleted())
                {
                    await FinalizePlanOnUserConfirmation();
                }
                else
                {
                    PopUpService.LogAndShowError(
                        Application.Common.StringConstants.TreatmentConsole.PlanExecutionConsistencyErrorTitle,
                        Application.Common.StringConstants.TreatmentConsole.PlanExecutionConsistencyErrorMessage);
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
                _ = LogWriter.LogAsync($"Plan execution was cancelled: {ex.Message}", LogRecordSeverity.Info, LogRecordType.System);
            }
            catch (InvalidOperationException ex) // probably we catch ClearPlan exception here
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.TreatmentTitle,
                    ex.Message);
            }
            catch (Exception ex)
            {
                IsCurrentViewModelRunning = false;
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.EmissionTitle,
                    StringConstants.TreatmentConsole.EmissionInterruptedError,
                    ex);
            }
            finally
            {
                await tokenSource.CancelAsync();

                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);
            }
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
          
            foreach (var field in PlanModel.TreatmentFields)
            {
                var coilSetpoints = coilConfigurations.FirstOrDefault(c => c.FieldName == field.Name);
                if (coilSetpoints is null)
                {
                    throw new NullReferenceException("Invalid applicator configuration, coil configuration data is missing");
                }

                float totalTime = (float)field.DwellTime;
                float remainingTime = totalTime - (float)field.Actual;

                GcbOperationalPoint op = new GcbOperationalPoint
                {
                    PointIndex = plan.TotalPoints,
                    TotalPointTime = totalTime,
                    RemainingPointTime = remainingTime,
                    SetpointKv = EnergyConverter.Convert(field.Energy),
                    TargetMA = Convert.ToSingle(field.Current),

                    // TODO: we don't apply magnetometer now, just get calibrated coilX/Y
                    FocusCoilSetpoint = Convert.ToSingle(coilSetpoints.FocusCurrent),
                    FilamentSetpoint = (float)heaterCurrent.Value,
                    XCoilSetpoint = Convert.ToSingle(coilSetpoints.XDeflectionCurrent),
                    YCoilSetpoint = Convert.ToSingle(coilSetpoints.YDeflectionCurrent),
                    AutoExecution = GetPlanAutoExecutionFlag()
                };

                plan.AddPoint(op);
            }

            return plan;
        }

        /// <summary>
        /// Tests for conditions preventing from plan preparation
        /// </summary>
        /// <returns>true if decision is to prevent the Prepare action</returns>
        protected override async Task<bool> PreventUserFromPrepare()
        {
            if (PlanModel.TreatmentFields is not null 
                && PlanModel.TreatmentFields.Any(tf => tf.DwellTime >= ClinicalDataConstants.DwellTimeLimit))
            {
                PopUpService.LogAndShowMessage(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.EMR.PlanDwellTimeLimitExceededErrorMessage, 
                    ReportType.Error, LogRecordSeverity.Error, LogRecordType.System);
                return true;
            }

            if (await base.PreventUserFromPrepare())
                return true;

            var qcStatus = await QcTreatmentAcceptanceService.QcDeviationAcceptanceTestAsync(PlanModel.CollimatorConfiguration.Id);
            if (qcStatus != QcAcceptanceStatus.Accepted)
            {
                var qcErrorMessage = qcStatus switch
                {
                    QcAcceptanceStatus.Missing => StringConstants.TreatmentConsole.Treatment.MissingQcErrorMessage,
                    QcAcceptanceStatus.Failed => StringConstants.TreatmentConsole.Treatment.FailedQcErrorMessage,
                    QcAcceptanceStatus.NoReference => StringConstants.TreatmentConsole.Treatment.MissingQcReferenceErrorMessage,
                    _ => throw new Exception("Quality check acceptance test error: unexpected test result")
                };
                DialogService.ReportError(
                        StringConstants.TreatmentConsole.QualityCheckRequiredErrorTitle,
                        qcErrorMessage);
                return true;
            }

            return false;
        }

        protected override void UserActionAudit(string actionMessage)
        {
            actionMessage = $"Treatment: {actionMessage}";

            base.UserActionAudit(actionMessage);
        }

        private async Task FinalizePlanOnUserConfirmation()
        {
            if (GetUserCleanupConfirmation(
                StringConstants.TreatmentConsole.TreatmentPlanCompletionConfirmationTitle,
                StringConstants.TreatmentConsole.TreatmentPlanCompletionConfirmationMessage))
            {
                await MainBoardModel.ResetTimers();
                await MainBoardModel.ClearPlan();
                UIStateMachine.IsPlanStaged = false;
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled})" +
                    $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                await SetPlanUnloadTaskAsync();
            }
        }

        private async Task GetVersionInfo()
        {
            try
            {
                var versionInfo = await MainBoardModel.GetVersionInfo();

                _ = LogWriter.LogAsync(versionInfo.ToString(), LogRecordSeverity.Info, LogRecordType.System);
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"Failed to GetVersionInfo: {ex.Message}", LogRecordSeverity.Info, LogRecordType.System);
            }
        }

        /// <summary>
        /// Block UI tabs, allowing to be on Treatment tab only
        /// </summary>
        private void BlockUiTabs()
        {
            UIStateMachine.IsPlanLoadedForTreatment = true;
            UIStateMachine.TabName = ExternalTabName.Treatment;
        }

        #endregion private methods


        #region Database interaction        
        private async Task FetchSafetyChecksAsync() // todo: refactor this (copy from SafetyCheckReportsViewModel)
        {
            try
            {
                await SafetyCheckModel.FetchSafetyCheckListAsync();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle, 
                    Application.Common.StringConstants.TreatmentConsole.SafetyCheck.ListLoadError,
                    ex);
            }
        }

        /// <summary>
        /// Starts watching LoadForTreatmentEvent stream and looks for a pending plan in the DB
        /// </summary>
        /// <returns></returns>
        private async Task LookForPlanAsync()
        {
            try
            {
                IPlan plan = await PlanModel.FindPendingPlanAsync();

                if (plan == null)
                {
                    plan = await PlanModel.FindLoadedPlanAsync();

                    if (plan == null && HeraclesExternalSettings.DebugLoadedPlanId > 0)
                    {
                        await PlanModel.LoadPlanForTreatment(
                            HeraclesExternalSettings.DebugLoadedPlanId,
                            isPartial: false);
                        return; // The debug plan will be delivered through the LoadForTreatmentEvent stream
                    }
                }

                var treatmentPlan = await SetPlanAsync(plan);

                // plan recovery from DB and board:
                if (plan is not null && plan.TreatmentLoadingState == TreatmentLoadingState.Loaded && treatmentPlan != null)
                {
                    //BlockUiTabs();
                    UIStateMachine.TabName = ExternalTabName.Treatment;

                    // Plan was not unloaded last time, need to recover it
                    try
                    {
                        // load last plan treatment from DB and set TreatmentFields' actual values from there:
                        var treatment = treatmentPlan.Treatment; //await TreatmentModel.FetchLastTreatmentByPlan(plan.Id);
                        if (BaseEntry.IsNullOrBlankEntry(treatment))
                        {
                            MakeLoadedPlanPending();
                            return; // last treatment was done more than 12 hours ago, so don't need to continue here
                        }

                        IList<IActualTreatmentField> actualFields = treatment.ActualTreatmentFields.ToList();// new List<IActualTreatmentField>(await ActualTreatmentFieldModel.FetchCollection(treatment.Id));

                        while (GCBDataStore.SystemTelemetry == null)
                        {
                            await Task.Delay(50, AppGlobals.AppCancellationTokenSource.Token);
                        }

                        var telemetry = GCBDataStore?.SystemTelemetry;

                        var totalOperationalPoints = telemetry?.TotalOperationalPoints ?? 0;

                        //read plan from the board and check whether it was started or not
                        var gcbPlan = await MainBoardModel.QueryPlanFromGCB();

                        if (gcbPlan.TotalPoints == 0)
                        {
                            MakeLoadedPlanPending();
                            return;
                        }

                        if (telemetry.CurrentOperationalPoint >= gcbPlan.TotalPoints)
                        {
                            _ = LogWriter.LogAsync($"Failed to recover the Plan: telemetry.CurrentOperationalPoint ({telemetry.CurrentOperationalPoint}) >= gcbPlan.TotalPoints ({gcbPlan.TotalPoints})", LogRecordSeverity.Warn, LogRecordType.System);
                            MakeLoadedPlanPending();
                            return;
                        }

                        var currentPoint = gcbPlan[telemetry.CurrentOperationalPoint];

                        if (telemetry.CurrentOperationalPoint == 0 &&
                            currentPoint.TotalPointTime == currentPoint.RemainingPointTime)
                        {
                            _ = LogWriter.LogAsync("GCB has a Plan already, but it was not started yet", LogRecordSeverity.Info, LogRecordType.System);
                            MakeLoadedPlanPending();
                            return;
                        }

                        //PlanModel.TreatmentFields.Last().Actual--; // todo: for debug

                        if (totalOperationalPoints != PlanModel.TreatmentFields.Count)
                        {
                            _ = LogWriter.LogAsync("Previous Plan was not unloaded correctly", LogRecordSeverity.Warn, LogRecordType.System);
                        }
                        else
                        {
                            // Next, check if board plan remaining times more or less match the DB state

                            GcbEmissionPlan emissionPlan = BuildGcbEmissionPlan();
                            MainBoardModel.SetCurrentPlan(emissionPlan);

                            try
                            {
                                if (emissionPlan.IsSameAs(gcbPlan))
                                {
                                    MainBoardModel.SetCurrentPlan(gcbPlan);

                                    var previousPointIndex = telemetry.CurrentOperationalPoint;
                                    // check current and previous point
                                    var startIndex = Math.Max(previousPointIndex - 1, 0);
                                    var stopIndex = Math.Min(previousPointIndex, totalOperationalPoints - 1);

                                    double actualEnergy;

                                    for (var i = startIndex; i <= stopIndex; i++)
                                    {
                                        var actualFromGcb = gcbPlan[i].TotalPointTime - gcbPlan[i].RemainingPointTime;

                                        var currentTreatmentField = PlanModel.TreatmentFields[i];
                                        if (actualFromGcb - currentTreatmentField.Actual > PlanCompletedThreshold)
                                        {
                                            currentTreatmentField.Actual = actualFromGcb;
                                            _ = LogWriter.LogAsync($"TreatmentField[{i + 1}].Actual was taken from GCB ({actualFromGcb} sec)", LogRecordSeverity.Info, LogRecordType.System);

                                            if (i < actualFields.Count)
                                                actualEnergy = actualFields[i].ActualEnergy;
                                            else
                                                actualEnergy = 0.0;

                                            await UpdateActualTreatmentField(currentTreatmentField, actualEnergy); // do not overwrite ActualEnergy value 
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _ = LogWriter.LogAsync($"Plan recovery: failed to update the Plan from GCB. {ex.Message}", LogRecordSeverity.Warn, LogRecordType.System);
                            }
                        }

                        if (IsPlanCompleted())
                        {
                            // Go to emission state and call FinalizePlanOnUserConfirmation
                            UIStateMachine.RequestStateSwitch(UIMacroState.Preparation);
                            UIStateMachine.RequestStateSwitch(UIMacroState.Emission);

                            await FinalizePlanOnUserConfirmation();
                        }
                        else
                        {
                            // Show 'Recover. Plan was not completed.'
                            PopUpService.LogAndShowMessage(
                                Application.Common.StringConstants.TreatmentConsole.PlanRecoveryInfoTitle,
                                Application.Common.StringConstants.TreatmentConsole.PlanRecoveryIncompletePlanExecutionInfo, 
                                ReportType.Info, LogRecordSeverity.Warn, LogRecordType.System);

                            // Go to Resume state
                            UIStateMachine.RequestStateSwitch(UIMacroState.Preparation);
                            UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                        }
                    }
                    catch (Exception ex)
                    {
                        PopUpService.LogAndShowError(
                            Application.Common.StringConstants.TreatmentConsole.PlanRecoveryErrorTitle,
                            Application.Common.StringConstants.TreatmentConsole.PlanRecoveryErrorMessage,
                            ex);
                        MakeLoadedPlanPending();
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                DialogService.ReportError(
                    Application.Common.StringConstants.TreatmentConsole.InvalidPlanApplicatorConfigurationErrorTitle,
                    ex.Message);
                _ = LogWriter.LogAsync($"OnCurrentPlanChanged: {ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
                // we don't throw here, as we can't handle this exception with just reloading the plan: it occurs when we have incorrect heater current config value
            }
            catch (Exception ex)
            {
                await LogWriter.LogAsync(
                    $"{Application.Common.StringConstants.TreatmentConsole.LoadForTreatmentInitLogMessage}. {ex?.Message}. {ex?.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw; // This method supposed to be called without direct user action. Exception rethrows, so ObservableTask can get information about error. 
            }
        }

        private void MakeLoadedPlanPending()
        {
            _safeTaskExecutor.ScheduleSafeTask(() =>
            {
                return System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    // Unblock the tabs:
                    UIStateMachine.IsPlanLoadedForTreatment = false;
                    // Move plan to pending for treatment
                    // Now it's a workaround: we rely on event stream to deliver us a reloaded plan
                    if (await PlanModel.UnloadFromTreatmentAsync())
                    {
                        await PlanModel.LoadPlanForTreatment(PlanModel.Plan.Id, isPartial: true);
                        // Set plan status to Loaded to prevent from plan update 
                        // which may discard our treatment choice made at plan lookup
                        PlanModel.Plan.TreatmentLoadingState = TreatmentLoadingState.Loaded;
                    }
                });
            });
        }


        /// <summary>
        /// Updates plan status in the database after loading into GCB.
        /// </summary>
        private async Task AcknowledgePlanForTreatment()
        {
            try
            {
                BlockUiTabs();
                await PlanModel.TreatmentLoadAcknowledgeAsync();
            }
            catch (Exception ex)
            {
                //UIStateMachine.IsPlanLoadedForTreatment = false;
                await LogWriter.LogAsync(
                    $"{Application.Common.StringConstants.TreatmentConsole.AcknowledgePlanStatusLogMessage}. {ex?.Message}. {ex?.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw; // This method supposed to be called without direct user action. Exception rethrows, so ObservableTask can get information about error. 
            }
        }


        /// <summary>
        /// Unloads plan from UI and database after clearing from GCB.
        /// </summary>
        private async Task UnloadPlanFromTreatment()
        {
            try
            {
                UIStateMachine.IsPlanLoadedForTreatment = false;

                if (await PlanModel.UnloadFromTreatmentAsync())
                {
                    await SetPlanAsync(null); //PlanModel.SetPlanAsync(null);
                    IsPreparing = false;
                    TreatmentModel.CloseTreatment();
                }
            }
            catch (Exception ex)
            {
                await LogWriter.LogAsync(
                    $"{Application.Common.StringConstants.TreatmentConsole.UnloadPlanLogMessage}. {ex?.Message}. {ex?.InnerException?.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw; // This method supposed to be called without direct user action. Exception rethrows, so ObservableTask can get information about error. 
            }
        }

        private async Task<TreatmentPlan?> SetPlanAsync(IPlan? plan)
        {
            var treatmentPlan = await TreatmentPreparationService.PrepareTreatmentAsync(plan);

            if (treatmentPlan != null)
            {
                // TODO: we fetch all necessary data to treatmentPlan now,
                // so that we could use it instead of addressing CollimatorConfigurationStore from somewhere else
                await UpdateCollimatorPreset(PlanModel.Prescription.Energy, PlanModel.Plan.CollimatorType);

                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled})" +
                        $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");
            }
            CheckForApplicatorCompatibility();

            ValidateCanExecuteCommands();

            return treatmentPlan;
        }

        private void OnLoadForTreatmentEvent(LoadForTreatmentEventsStreamArgs args)
        {
            try
            {
                _safeTaskExecutor.ScheduleSafeTask(() =>
                {
                    return System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    {
                        if (PlanModel.Plan == null || PlanModel.Plan.TreatmentLoadingState != TreatmentLoadingState.Loaded)
                        {
                            // plan is not loaded yet, so we can replace it with a new one
                            await SetPlanAsync(args?.Plan);
                        }
                        else
                        {
                            _ = LogWriter.LogAsync(
                                $"OnCurrentPlanChanged: new load for treatment event with plan id={args?.Plan?.Id} while previous plan is already loaded",
                                LogRecordSeverity.Warn, LogRecordType.System);
                        }
                    });
                });
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"OnCurrentPlanChanged: {ex.Message}. {ex.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
        }
        #endregion Database interaction


        #region Callbacks

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
                        var fields = PlanModel.TreatmentFields;
                        if (PreviousOperationPointIndex >= 0 && fields is not null && fields.Count > PreviousOperationPointIndex)
                        {
                            var previousTf = fields[PreviousOperationPointIndex];
                            
                            previousTf.Actual = previousTf.DwellTime;
                            RecalculateInitialXrayTime();
                            XrayPointStartTime = 0;

                            await MainBoardModel.UpdatePlanPointFromGCB(PreviousOperationPointIndex);
                            var previousPoint = MainBoardModel.CurrentPlan[PreviousOperationPointIndex];

                            previousTf.Actual = previousPoint.TotalPointTime - previousPoint.RemainingPointTime;
                            if (previousTf.DwellTime - previousTf.Actual < PlanCompletedThreshold)
                            {
                                previousTf.IsDone = true;
                            }

                            _ = LogWriter.LogAsync($"Query point response: TotalPointTime={previousPoint.TotalPointTime} RemainingPointTime={previousPoint.RemainingPointTime} Actual={previousTf.Actual}", LogRecordSeverity.Info, LogRecordType.System);
                            await UpdateActualTreatmentField(previousTf, ActualTreatmentFieldModel.AverageEnergy);
                            //_ = LogService.LogAsync($"UpdateEmissionTreatmentField: operationalPointIndex={operationalPointIndex}, timerValue {timerValue} _xrayTime {_xrayTime} TotalDuration {PlanModel.TotalDuration}", LogRecordSeverity.Info, LogRecordType.System);
                        }
                    }

                    if (operationalPointIndex < PlanModel.TreatmentFields?.Count)
                    {
                        var tf = PlanModel.TreatmentFields[operationalPointIndex];

                        if (operationalPointIndex != PreviousOperationPointIndex)
                        {
                            PreviousOperationPointIndex = operationalPointIndex;
                            XrayPointStartTime = tf.Actual;
                        }

                        UpdateBeamOnProgress(Convert.ToSingle(PlanModel.TotalDuration), Convert.ToSingle(XrayTime + timerValue));

                        tf.Actual = XrayPointStartTime + timerValue;
                        if (tf.DwellTime - tf.Actual < PlanCompletedThreshold)
                        {
                            tf.IsDone = true;
                        }

                        UpdateTreatmentFieldSelection(tf);

                        ActualTreatmentFieldModel.AddEnergyValue(telemetry.KvFeedback);

                        await UpdateActualTreatmentField(tf, ActualTreatmentFieldModel.AverageEnergy);
                    }
                    else if (PlanModel.TreatmentFields == null
                             || PlanModel.TreatmentFields.Count == 0
                             || telemetry.CurrentOperationalPoint >= PlanModel.TreatmentFields.Count)
                    {
                        UpdateTreatmentFieldSelection(null);
                    }
                }
                else
                {
                    if (PlanModel.SelectedTreatmentField != null)
                        UpdateTreatmentFieldSelection(null);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private void UpdateTreatmentFieldSelection(ITreatmentFieldEntry tf)
        {
            PlanModel.SelectedTreatmentField = tf;
            var collection = Application.Models.RDBMS.EMR.TreatmentField.GetTreatmentFieldCollection(PlanModel.Plan?.CollimatorType);
            HoneycombSelection = (tf is null) ? null : collection?.FirstOrDefault(item => item.Name.Equals(tf.Name));
        }

        private async Task UpdateActualTreatmentField(ITreatmentFieldEntry tf, double actualEnergy)
        {
            try
            {
                _ = DumpActualFieldsToTextFileAsync();
                if (BaseEntry.IsNullOrBlankEntry(TreatmentModel.Treatment))     // nothing to update                   
                    return;

                IActualTreatmentField atf = new Application.Models.RDBMS.EMR.ActualTreatmentField
                {
                    TreatmentId = TreatmentModel.Treatment.Id,
                    CreationDate = DateTime.Now,
                    Name = tf.Name,
                    ActualDuration = tf.Actual,
                    ActualCurrent = tf.Current,
                    ActualEnergy = actualEnergy,
                    Completed = tf.IsDone ? 1 : 0,
                    ActualDose = TreatmentDoseCalculation.CalculateDose(tf.Name, PlanModel.CollimatorConfiguration, tf.Actual)
                };

                // TODO: need to guarantee that we'll write the final fields state,
                // as we may fail on any of these now.
                // Need to add some retriable observable task at the end of treatment or on it interruption
                _safeTaskExecutor.ScheduleSafeTask(() => ActualTreatmentFieldModel.SaveActualTreatmentField(atf));
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"Failed to update ActualTreatmentField: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                // todo: show error dialog?
            }
        }

        private Task DumpActualFieldsToTextFileAsync()
        {
            var dataToDump = new List<ITreatmentFieldEntry>(PlanModel.TreatmentFields);
            return Task.Run(() =>
            {
                try
                {
                    lock (PlanModel)
                    {
                        using StreamWriter writer = File.CreateText(ACTUAL_FIELDS_DUMP_FILENAME);
                        writer.WriteLine("PlanId,FieldId,FieldName,DwellTime,ActualDose");
                        foreach (var field in dataToDump)
                        {
                            var actualDose = TreatmentDoseCalculation.CalculateDose(field.Name, PlanModel.CollimatorConfiguration, field.Actual);
                            writer.WriteLine($"{field.PlanId},{field.Id},{field.Name},{field.DwellTime},{actualDose}");
                        }
                        writer.Close();
                    }
                }
                catch
                {

                }
            });
        }

        private async Task SaveTreatmentAsync()
        {
            var hasTreatment = !BaseEntry.IsNullOrBlankEntry(TreatmentModel.Treatment);
            await TreatmentModel.SaveTreatmentData();

            // TODO: what are we trying to fetch here? It is called on a stored blank treatment only:
            if (!hasTreatment)
                await ActualTreatmentFieldModel.FetchCollection(TreatmentModel.Treatment.Id);
        }

        private void OnPlanEvent(IPlan plan)
        {
            _safeTaskExecutor.ScheduleSafeTask(() =>
            {
                return System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    if (plan is null || PlanModel.Plan is null || plan.Id != PlanModel.Plan.Id)
                        return; // nothing to do here

                    // Ok, plan is matching, we need to check if it changed its state from Pending to Unload.
                    // In this case, we need to clear the plan from here:
                    if (IsPreparing == false
                        && (PlanModel.Plan.TreatmentLoadingState is TreatmentLoadingState.PendingLoad
                                or TreatmentLoadingState.PartialPendingLoad)
                        && plan.TreatmentLoadingState == TreatmentLoadingState.Unloaded)
                    {
                        try
                        {
                            await StopAsync();

                            UIStateMachine.RequestStateSwitch(UIMacroState.StandBy); // restore the initial state
                            await SetPlanAsync(null);
                        }
                        catch (Exception ex)
                        {
                            // TODO: do we need to notify user with a dialog here?
                            _ = LogWriter.LogAsync($"Remote plan unload request failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                        }
                    }
                });
            });
        }
        #endregion


        #region RegionViewModelBase
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            Task.Run(GetVersionInfo);

            CurrentTaskCommand = new(() => 
            {
                // To prevent race conditions on SetPlanAsync in the LookForPlanAsync against any LoadForTreatment events
                _safeTaskExecutor.ScheduleSafeTask(() =>
                {
                    return System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                    {
                        try
                        {
                            var lookupTask = LookForPlanAsync();
                            CurrentTask = new ObservableTask(
                                lookupTask,
                                Application.Common.StringConstants.TreatmentConsole.LoadForTreatmentInitErrorMessage);
                            await lookupTask;
                        }
                        catch
                        {
                            // we don't want to log this task's error second time from the executor, so we skip it
                        }
                    });
                });
            });

            CancelCurrentTaskCommand = new DelegateCommand(() => {
                CurrentTask = null;
                CancelCurrentTaskCommand = null;
                });
            CurrentTaskCommand.Execute();

            ValidateCanExecuteCommands();
        }

        protected override void OnExit()
        {
            //Task.Run(() => ClearPlanAsync());// todo: check whether a loaded plan is really can be cleared before closing the app
            //Task.Run(() => System.Windows.Application.Current.Dispatcher.BeginInvoke(async () => await UnloadPlanFromTreatment())).GetAwaiter().GetResult();
            System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {
                try
                {
                    // TODO: need to make sure it doesn't cause race conditions with other plan state operations
                    await UnloadPlanFromTreatment();
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        Application.Common.StringConstants.TreatmentConsole.PlanUnloadFromTreatmentErrorTitle,
                        Application.Common.StringConstants.TreatmentConsole.PlanUnloadFromTreatmentErrorMessage,
                        ex);
                }
            });


            base.OnExit();
        }
        #endregion RegionViewModelBase

    }

    public class NoCommWatchdog(IMainBoardAPI mainBoardAPI, IUIStateMachine stateMachine)
    {
        private const int StopDelayIntervalMs = 5000;
        System.Timers.Timer _stopEmissionTimer = CreateTimer(mainBoardAPI, stateMachine);

        private static System.Timers.Timer CreateTimer(IMainBoardAPI mainBoardAPI, IUIStateMachine uIStateMachine)
        {
            System.Timers.Timer timer = new(StopDelayIntervalMs);
            timer.Elapsed += (s, e) =>
            {
                try
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        uIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                    });
                    mainBoardAPI.Stop();
                }
                catch (Exception)
                {
                    // no matter what, we just end the task, as we did our best stopping it
                }
            };
            timer.AutoReset = false;
            return timer;
        }

        public bool IsRunning => _stopEmissionTimer.Enabled;

        public void Start()
        {
            _stopEmissionTimer.Start();
        }

        public void Stop()
        {
            _stopEmissionTimer.Stop();
        }
    }
}
