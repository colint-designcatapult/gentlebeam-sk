using System.Diagnostics;
using Heracles.Application.Enums;
using Heracles.Application.Events;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.External.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Helpers;
using Xcc.Application.Models;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;
using StringConstants = Xcc.Core.Constants.StringConstants;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Domain.GryphonBoard.Model.OperationGuards;
using Xcc.Core.Logging;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.External.ViewModels
{
    public abstract class OperatePlanViewModelBase : RegionViewModelBase, ILoadAware
    {
        #region Constructors
        public OperatePlanViewModelBase() : base(null)
        {
        }
        public OperatePlanViewModelBase(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IHeraclesExternalSettings heraclesExternalSettings,
            IGCBDataStore gcbDataStore,
            IUIStateMachine uiStateMachine,
            ILogWriter logWriter,
            IWarmupService warmupService,
            IPopUpService popUpService,
            IDialogService dialogService,
            IMainBoardModel mainBoardModel,
            IGcbIndicators gcbIndicators,
            ICollimatorModel collimatorModel,
            ICollimatorConfigurationStore collimatorConfigurationStore,
            IActionAuditService actionAuditService,
            ISafetyCheckModel safetyCheckModel,
            IBearerTokenUserSessionManager userSessionManager)
            : base(regionManager, eventAggregator, dialogService)
        {
            HeraclesExternalSettings = heraclesExternalSettings;
            GCBDataStore = gcbDataStore;
            UIStateMachine = uiStateMachine;
            LogWriter = logWriter;
            WarmupService = warmupService;
            PopUpService = popUpService;
            MainBoardModel = mainBoardModel;
            GcbIndicators = gcbIndicators;
            CollimatorModel = collimatorModel;
            CollimatorConfigurationStore = collimatorConfigurationStore;
            ActionAuditService = actionAuditService;
            SafetyCheckModel = safetyCheckModel;
            UserSessionManager = userSessionManager;
            ClearFaultsGuard = new();

            eventAggregator.GetEvent<SystemTelemetryChangedEvent>().Subscribe(OnSystemTelemetryChanged, ThreadOption.UIThread);
            
            UIStateMachine.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(UIStateMachine.State))
                {
                    OnUiStateChanged(UIStateMachine.State);
                }
            };

            CollimatorModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ICollimatorModel.ActiveCollimator))
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ActualCollimatorConfiguration = CollimatorModel.ActiveCollimator?.Configuration;
                        //CheckForTargetMismatch();
                    });

                }
            };
        }
        #endregion

        #region properties
        const int MinSessionLifetimeOnPrepareInMinutes = 30;

        private ICollimatorConfiguration? _actualCollimatorConfiguration;
        public ICollimatorConfiguration? ActualCollimatorConfiguration
        {
            get => _actualCollimatorConfiguration;
            set
            {
                if (SetProperty(ref _actualCollimatorConfiguration, value) || value is null)
                {
                    try
                    {
                        CheckForApplicatorCompatibility();
                    }
                    catch (Exception ex)
                    {
                        _ = LogWriter.LogAsync($"Check for applicator mismatch failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                    }
                }
            }
        }
        public IHeraclesExternalSettings HeraclesExternalSettings { get; }
        public IGCBDataStore GCBDataStore { get; }
        public IUIStateMachine UIStateMachine { get; }
        public ILogWriter LogWriter { get; }
        public IWarmupService WarmupService { get; }
        public IPopUpService PopUpService { get; }
        public IMainBoardModel MainBoardModel { get; }
        public IGcbIndicators GcbIndicators { get; }
        public ICollimatorModel CollimatorModel { get; }
        public ICollimatorConfigurationStore CollimatorConfigurationStore { get; }
        public IActionAuditService ActionAuditService { get; }
        public ISafetyCheckModel SafetyCheckModel { get; }
        public IBearerTokenUserSessionManager UserSessionManager { get; }

        private int _beamOnProgress;
        public int BeamOnProgress
        {
            get => _beamOnProgress;
            set => SetProperty(ref _beamOnProgress, value);
        }

        // Signals that there's a plan we can stage for treatment
        private bool _hasPlanForTreatment = false;
        public bool HasMatchingPlanForTreatment
        {
            get => _hasPlanForTreatment;
            set => SetProperty(ref _hasPlanForTreatment, value);
        }

        private GcbStateNew? _gcbState;
        [Obsolete]
        public GcbStateNew? GcbState
        {
            get => _gcbState;
            set
            {
                SetProperty(ref _gcbState, value);
            }
        }
        protected GcbStateNew? PreviousGcbState { get; set; } = null;
        
        protected SemaphoreSlim Semaphore { get; } = new SemaphoreSlim(1, 1);

        private ObservableTask _currentTask;
        public ObservableTask CurrentTask { get => _currentTask; protected set => SetProperty(ref _currentTask, value); }

        public float PlanCompletedThreshold { get; } = 0.10f;

        private bool _isCurrentViewModelRunning;
        /// <summary>
        /// Can be used for controlling of telemetry updates
        /// </summary>
        public bool IsCurrentViewModelRunning { get => _isCurrentViewModelRunning; set => SetProperty(ref _isCurrentViewModelRunning, value); }
        protected double XrayTime { get; set; } = 0.0;
        protected double XrayPointStartTime { get; set; } = 0.0;
        protected int PreviousOperationPointIndex { get; set; } = 0;
        protected bool IsPreparing { get; set; }

        private bool _isResuming;

        protected ClearFaultsEnergyGuard ClearFaultsGuard { get; }

        private ApplicatorCompatibilityStatus _applicatorCompatibilityStatus =
            new ApplicatorCompatibilityStatus(false, "Applicator compatibility is undefined");

        public ApplicatorCompatibilityStatus ApplicatorCompatibilityStatus
        {
            get => _applicatorCompatibilityStatus;
            set => SetProperty(ref _applicatorCompatibilityStatus, value);
        }
        #endregion

        #region commands

        private DelegateCommand? _getFaultsCommand;
        public DelegateCommand GetFaultsCommand => _getFaultsCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("FaultsView");
            });

        private DelegateCommand? _showInterlocks;
        public DelegateCommand ShowInterlocksCommand => _showInterlocks ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("InterlocksDialogView");
            });

        private DelegateCommand? _showDetailedTelemetryCommand;
        public DelegateCommand ShowDetailedTelemetryCommand => _showDetailedTelemetryCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("TelemetryDialogView");
            });

        //Left buttons and progress bars
        private DelegateCommand? _clearPlanCommand;
        public DelegateCommand ClearPlanCommand => _clearPlanCommand ??= new DelegateCommand(
            async () =>
            {
                await OnClearPlanClicked();
                ValidateCanExecuteCommands();                
            },
            canExecuteMethod: CanClearPlan);

        private DelegateCommand? _prepareCommand;
        public DelegateCommand PrepareCommand => _prepareCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    if (IsPreparing)
                        return;

                    IsPreparing = true;

                    if (StopOnUserSessionExpiration())
                        return;

                    if (await PreventUserFromPrepare()) // todo: check for refactoring
                        return;

                    IsCurrentViewModelRunning = true;

                    await WarmUpAsync();

                    UIStateMachine.RequestStateSwitch(UIMacroState.Preparation);
                    // Is warm already, can prepare. Otherwise, we'll do it on callback when it finishes warming up
                    await PrepareAsync();
                }
                catch (TaskCanceledException)
                { }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                        StringConstants.TreatmentConsole.PlanPreparationErrorMessage,
                        ex);
                }
                finally
                {
                    IsPreparing = false;
                    ValidateCanExecuteCommands();
                }
            },
            canExecuteMethod: CanPrepare);


        //Central buttons and progress bars
        private DelegateCommand? _resetTimersCommand;
        public DelegateCommand ResetTimersCommand => _resetTimersCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    IsCurrentViewModelRunning = false;
                    await MainBoardModel.ResetTimers();

                    if (UIStateMachine.State == UIMacroState.Preparation
                     || UIStateMachine.State == UIMacroState.Emission)
                    {
                        if (UIStateMachine.IsPlanStaged)
                        {
                            // Go to state with resume button
                            UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                        }
                    }
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        StringConstants.TreatmentConsole.ResetTimersTitle,
                        StringConstants.TreatmentConsole.ResetTimersErrorMessage,
                        ex);
                }
                finally
                {
                    Debug.WriteLine($"UIStateMachine.State: {UIStateMachine.State}");

                    IsPreparing = false;
                    ValidateCanExecuteCommands();
                }
            },
            canExecuteMethod: () => CanResetTimers());


        private DelegateCommand? _clearErrorsCommand;
        public DelegateCommand ClearErrorsCommand => _clearErrorsCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    IsCurrentViewModelRunning = false;
                    await MainBoardModel.ClearFaults();

                    try
                    {
                        RequestUiStateSwitchOnClearErrors();
                    }
                    catch (Exception ex)
                    {
                        PopUpService.LogAndShowError(
                            StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                            StringConstants.TreatmentConsole.PlanPreparationAfterFaultErrorMessage,
                            ex);
                    }
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        StringConstants.TreatmentConsole.ClearErrorsTitle,
                        StringConstants.TreatmentConsole.ClearErrorsErrorMessage,
                        ex);

                }
                finally
                {
                    Debug.WriteLine($"UIStateMachine.State: {UIStateMachine.State}");

                    IsPreparing = false;
                    ValidateCanExecuteCommands();
                }
            }).ObservesCanExecute(() => ClearFaultsGuard.CanClearErrors);


        private DelegateCommand? _beamOnCommand;
        public DelegateCommand BeamOnCommand => _beamOnCommand ??= new DelegateCommand(
            async () =>
            {
                UserActionAudit("User triggered Emission delivery");

                await OnBeamOnClicked();

                ValidateCanExecuteCommands();
            },
            canExecuteMethod: CanBeamOn);


        //Right buttons
        private DelegateCommand? _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    if (UIStateMachine.State == UIMacroState.Emission)
                    {
                        UserActionAudit("User triggered emission stop");
                    }

                    IsCurrentViewModelRunning = false;

                    await StopAsync();
                }
                catch (Exception ex)
                {
                    PopUpService.LogAndShowError(
                        StringConstants.TreatmentConsole.StopTitle,
                        StringConstants.TreatmentConsole.StopErrorMessage,
                        ex);
                }
                finally
                {
                    ValidateCanExecuteCommands();
                }
            },
            canExecuteMethod: MainBoardModel.CanStop);

        private DelegateCommand? _resumeCommand;
        public DelegateCommand ResumeCommand => _resumeCommand ??= new DelegateCommand(
            async () =>
            {
                if (_isResuming)
                    return;

                _isResuming = true;
                IsCurrentViewModelRunning = true;

                try
                {
                    UIStateMachine.RequestStateSwitch(UIMacroState.Preparation);

                    UserActionAudit("User triggered Resume");

                    await MainBoardModel.ResetTimers();
                    await WarmUpAsync();

                    if (UIStateMachine.IsPlanLoadedForTreatment == false)
                        UIStateMachine.IsPlanLoadedForTreatment = true;

                    await MainBoardModel.ResumePlan();
                }
                catch (Exception ex)
                {
                    // We could fail on BeamOn and went to Resume, so we won't do a transition in this case
                    if (UIStateMachine.State == UIMacroState.Preparation || UIStateMachine.State == UIMacroState.Emission)
                    {
                        UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                    }
                    else if (UIStateMachine.State == UIMacroState.StandBy)
                    {
                        _ = LogWriter.LogAsync(
                            "ResumeCommand error: went into unexpected StandBy state",
                            LogRecordSeverity.Error, LogRecordType.System);
                    }
                    PopUpService.LogAndShowError(
                        StringConstants.TreatmentConsole.ResumeErrorTitle,
                        StringConstants.TreatmentConsole.ResumeErrorMessage,
                        ex);
                }
                finally
                {
                    _isResuming = false;

                    ValidateCanExecuteCommands();
                }
            },
            canExecuteMethod: CanResume);

        private DelegateCommand? _currentTaskCommand;

        public DelegateCommand CurrentTaskCommand
        {
            get => _currentTaskCommand;
            set => SetProperty(ref _currentTaskCommand, value);
        }

        private DelegateCommand? _cancelCurrentTaskCommand  ;
        public DelegateCommand? CancelCurrentTaskCommand
        {
            get => _cancelCurrentTaskCommand;
            set => SetProperty(ref _cancelCurrentTaskCommand, value);
        }


        #endregion


        #region public methods
        #endregion

        #region abstract methods

        protected abstract Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry);
        protected abstract void CheckForApplicatorCompatibility();
        protected abstract Task OnBeamOnClicked();

        /// <summary>                                                 
        /// calculates initial XRay time for the beamOn progress bar                                                 
        /// </summary>
        protected abstract void RecalculateInitialXrayTime();

        protected abstract GcbEmissionPlan BuildGcbEmissionPlan();
        protected abstract Task SetPlanUnloadTaskAsync();
        #endregion

        #region private methods

        protected async Task WaitAndIgnoreTaskExceptionsAsync(Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                // ignored
            }
        }

        protected async Task UpdateAfterEmission(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var systemTelemetry = GCBDataStore.SystemTelemetry ?? throw new Exception("Failed to wait for the end of emission: GCB telemetry connection lost.");

                if (systemTelemetry.ControlBoardState != GcbStateNew.Emission &&
                    PreviousGcbState == GcbStateNew.Emission)
                {
                    break;
                }

                await Task.Delay(50, token);
            }

            await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
            {
                await UpdateEmissionTreatmentField(GCBDataStore.SystemTelemetry); 
            });
        }

        protected bool IsPlanCompleted()
        {
            return MainBoardModel.CurrentPlan?.IsCompleted(PlanCompletedThreshold) ?? false;
        }

        protected virtual void SwitchExternalTab(ExternalTabName tabName)
        {
            EventAggregator?.GetEvent<RequestExternalTabChangeEvent>().Publish(tabName);
        }

        protected virtual Task<bool> PreventUserFromPrepare()
        {
            if (!IsSafetyCheckDone())
            {
                PopUpService.LogAndShowMessage(
                    Application.Common.StringConstants.TreatmentConsole.PlanPreparationEventDialogTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanPreparationSafetyCheckRequest,
                    ReportType.Info, LogRecordSeverity.Warn, LogRecordType.User);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }


        protected virtual void UserActionAudit(string actionMessage)
        {
            ActionAuditService.RegisterAction(actionMessage);
        }
        
        protected virtual async Task OnClearPlanClicked()
        {
            try
            {
                await MainBoardModel.ClearPlan();

                UIStateMachine.IsPlanStaged = MainBoardModel.IsPlanStaged;
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled})" +
                    $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                await SetPlanUnloadTaskAsync();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.ClearPlanTitle,
                    StringConstants.TreatmentConsole.ClearPlanErrorMessage,
                    ex);
                return;
            }
        }

        /// <summary>
        /// Prepares a new plan for execution
        /// </summary>
        /// <param name="tryKeepPrevPlan">If the flag is set to true, it will try to preserve the matching plan values from GCB</param>
        /// <returns></returns>
        protected virtual async Task PrepareAsync(bool tryKeepPrevPlan = false)
        {
            try
            {
                UIStateMachine.IsPlanStaged = MainBoardModel.IsPlanStaged;

                // TODO: what if Prepare fails? We probably need to switch back to StandBy state?
                UIStateMachine.RequestStateSwitch(UIMacroState.Preparation);
                Debug.WriteLine($"Update UI state machine: State={UIStateMachine.State}, LB=({UIStateMachine.LeftButton.State}, {UIStateMachine.LeftButton.IsEnabled})" +
                    $"CB=({UIStateMachine.CentralButton.State}, {UIStateMachine.CentralButton.IsEnabled}), Stop={UIStateMachine.RightButton.IsEnabled}");

                GcbEmissionPlan plan = BuildGcbEmissionPlan();

                bool loadedPlanFromScratch = await MainBoardModel.PreparePlan(plan, tryKeepPrevPlan);
                RecalculateInitialXrayTime();
                GcbIndicators.BeamOnProgress.Reset();

                if (!loadedPlanFromScratch)
                {
                    // We went into staged state and found that this exact plan is on the board already,
                    // so we want to try to resume it manually or just clear it
                    UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                }
            }
            // TODO: add custom exception for plan mismatch (board vs loaded for treatment)
            // Show warning and clear the plan from the board only
            catch
            {
                // To prevent error loop on preparation callback, go to StandBy
                if (MainBoardModel.IsPlanStaged)
                {
                    UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                }
                else
                {
                    UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                }

                throw;
            }
            finally
            {
                // In case if PreparePlan failed after plan was staged:
                UIStateMachine.IsPlanStaged = MainBoardModel.IsPlanStaged;
            }
        }

        /// <summary>
        /// Returns flag value indicating if plan auto-execution should be applied
        /// </summary>
        /// <returns></returns>
        protected virtual bool GetPlanAutoExecutionFlag()
        {
            return true;
        }

        protected async Task UpdateCollimatorPreset(Energy energy, TargetType collimatorType)
        {
            try
            {
                await CollimatorConfigurationStore.FetchConfigurationAsync(energy, collimatorType);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.HeaterCurrentErrorTitle,
                    $"{Application.Common.StringConstants.TreatmentConsole.HeaterCurrentMissingValueErrorMessage}: Energy = {energy}, Collimator = {collimatorType}", ex);
            }
        }

        protected bool GetUserCleanupConfirmation(string reportHeader, string reportMessage)
        {
            return DialogService.Confirmation(reportHeader, reportMessage);
        }

        protected void UpdateBeamOnProgress(double totalDuration, double value)
        {
            if (totalDuration < double.Epsilon)
            {
                BeamOnProgress = 0;
                return;
            }

            var progress = Convert.ToInt32(value / totalDuration * 100.0);

            BeamOnProgress = BoundProgress(progress);
        }

        protected int BoundProgress(int progress)
        {
            if (progress < 0) progress = 0;
            if (progress > 100) progress = 100;

            return progress;
        }

        protected virtual void ValidateCanExecuteCommands()
        {
            BeamOnCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
            ClearErrorsCommand.RaiseCanExecuteChanged();
            ClearPlanCommand.RaiseCanExecuteChanged();
            ResetTimersCommand.RaiseCanExecuteChanged();
            PrepareCommand.RaiseCanExecuteChanged();
            ResumeCommand.RaiseCanExecuteChanged();
        }

        protected async Task WarmUpAsync()
        {
            try
            {
                await WarmupService.RunSafeWarmupAsync(
                    WarmupParameters.FastWarmup(HeraclesExternalSettings.WarmupSetpoint, CollimatorModel.ActiveHead?.Id ?? 0));
            }
            catch (TaskCanceledException ex)
            {
                _ = LogWriter.LogAsync($"Warmup: interrupted. {ex?.Message}", LogRecordSeverity.Info, LogRecordType.System);
                throw;
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.WarmupErrorTitle,
                    StringConstants.TreatmentConsole.WarmupFailureError,
                    ex);
                throw;
            }
        }

        protected virtual bool CanPrepare()
        {
            //bool cannotResetTimers = !CanResetTimers();
            //bool boardCanPrepare = MainBoardModel.CanPrepare();
            //Debug.WriteLine($"{cannotResetTimers} - {!IsPreparing} - {HasMatchingPlanForTreatment} - {boardCanPrepare}");
            return !CanResetTimers()
                && !IsPreparing
                && HasMatchingPlanForTreatment
                && CollimatorConfigurationStore.CoilConfigurations != null
                && CollimatorConfigurationStore.HeaterCurrent != null
                && MainBoardModel.CanPrepare();
        }

        protected bool CanResetTimers()
        {
            if (UIStateMachine.State == UIMacroState.Preparation)
                return false;

            return MainBoardModel.CanResetTimers();
        }

        private bool IsSafetyCheckDone()
        {
            var latestCheck = SafetyCheckModel.SafetyChecks?.FirstOrDefault();  // should be already sorted by descending id
            if (latestCheck == null)
                return false;

            return latestCheck.CreationDate.Date == DateTime.Today;
        }

        private bool CanClearPlan()
        {
            return !CanResetTimers() && MainBoardModel.CanClearPlan();
        }

        protected virtual bool CanResume()
        {
            // Generally, we can resume any staged plan,
            // so we rely on the UIStateMachine with the state of the button and just allow it from here
            // (but in practice there's some exceptions like QC - that's why the method is virtual)
            //return (UIStateMachine.State == UIMacroState.ResumePlan) && UIStateMachine.IsPlanStaged;
            return true;
        }

        private bool CanBeamOn()
        {
            return MainBoardModel.CanBeamOn();
        }

        protected async Task StopAsync()
        {
            var inputUIState = UIStateMachine.State;
            try
            {
                // If we reached at least Staged state with current plan, we need to go to Resume track:
                if ((UIStateMachine.State == UIMacroState.Preparation && UIStateMachine.IsPlanStaged)
                        || UIStateMachine.State == UIMacroState.Emission)
                {
                    UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                }

                await MainBoardModel.Stop();
            }
            catch (Exception)
            {
                UIStateMachine.RequestStateSwitch(inputUIState); // restore the initial state
                throw;
            }
            finally
            {
                IsPreparing = false;

                ValidateCanExecuteCommands();
            }
        }

        public virtual void CheckForBoardRestart(GcbStateNew gcbState)
        {
            if (gcbState == GcbStateNew.Startup
                && UIStateMachine.State != UIMacroState.StandBy)
            {
                UIStateMachine.OnReboot();
                MainBoardModel.CancelCurrentTask();

                PopUpService.ShowMessage(
                    "Control Board Reboot",
                    "The board was rebooted. Please start the operation again.",
                    ReportType.Info);
            }
        }

        // TODO: refactor this method, as it's the same for both external apps.
        private bool StopOnUserSessionExpiration()
        {
            var userSession = UserSessionManager.UserSession;
            var sessionExpirationTimespan =
                TimeSpan.FromMinutes(MinSessionLifetimeOnPrepareInMinutes);
            if (userSession is null)
            {
                DialogService.ReportError("User Session Error", "User session is missing. Please log in again.");
                return true;
            }
            else if (userSession.ExpiresIn(sessionExpirationTimespan))
            {
                if (PopUpService.YesCancelDialog(
                    "User Session",
                    $"Warning: user session is about to expire in less than {MinSessionLifetimeOnPrepareInMinutes} minutes." +
                    $"{Environment.NewLine}In order to proceed, do you want to refresh it?",
                    iconType: DialogBoxIconType.Warning
                    ) == DialogBoxResult.Yes)
                {
                    UserSessionManager.ExpireUserSession();
                }
                return true;
            }
            return false;
        }
        #endregion private methods

        #region callbacks
        protected void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry)
        {
            ClearFaultsGuard.OnSystemTelemetryChanged(systemTelemetry);

            CheckForGcbStateChanged(systemTelemetry?.ControlBoardState);

            if (!IsCurrentViewModelRunning)
                return; // Don't update progress on current view

            _ = UpdateEmissionTreatmentField(systemTelemetry);
        }

        protected virtual void CheckForGcbStateChanged(GcbStateNew? state)
        {
            try
            {
                PreviousGcbState = GcbState;

                GcbState = state;
                
                if (PreviousGcbState != GcbState)
                {
                    if (GcbState != null)
                    {
                        UIStateMachine.OnGcbStateChange(GcbState.Value);
                        CheckForBoardRestart(GcbState.Value);
                    }
                    else
                    {
                        UIStateMachine.OnGcbStateChange(GcbStateNew.NoComm);
                    }

                    ValidateCanExecuteCommands();

                    if (SystemTelemetry.IsFaultState(state))
                    {
                        MainBoardModel.CancelCurrentTask();
                    }
                }
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync($"OnGCBStateChanged: {ex.Message}", LogRecordSeverity.Error, LogRecordType.Error);
            }
        }

        protected virtual void RequestUiStateSwitchOnClearErrors()
        {
            if (UIStateMachine.State == UIMacroState.Preparation
                                        || UIStateMachine.State == UIMacroState.Emission)
            {
                if (UIStateMachine.IsPlanStaged)
                {
                    //await WarmUpAsync();
                    //await MainBoardModel.WaitForWarmupCompleted(new List<GcbStateNew>
                    //{
                    //    GcbStateNew.Staged
                    //});

                    // Go to state with resume button
                    UIStateMachine.RequestStateSwitch(UIMacroState.ResumePlan);
                }
                else
                {
                    // No plan should be on board, try to proceed with plan reload
                    UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                    // or we can just call PreparePlanAsync to force plan reload
                }
            }
        }
        private void OnUiStateChanged(UIMacroState state)
        {
            ValidateCanExecuteCommands();
        }
        #endregion


        #region RegionViewModelBase
        protected override void OnExit()
        {
            UIStateMachine.IsPlanLoadedForTreatment = false;

            base.OnExit();
        }
        #endregion RegionViewModelBase

        public virtual void VisiblyLoaded()
        {
            ActualCollimatorConfiguration = CollimatorModel.ActiveCollimator?.Configuration;
        }

        public virtual void Unloaded()
        {
        }
    }
}
