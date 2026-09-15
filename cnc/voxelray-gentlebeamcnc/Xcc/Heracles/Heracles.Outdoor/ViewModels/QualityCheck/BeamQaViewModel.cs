using System.Diagnostics;
using System.Windows.Data;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck.Events;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Domain.DataManagement.System.QualityCheck;
using Heracles.Application.Enums;
using Heracles.Application.Helpers;
using Heracles.Application.Models;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class BeamQaViewModel : OperatePlanViewModelBase
    {

        #region Contructors
        public BeamQaViewModel()
        { }

        public BeamQaViewModel(
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
            QcReportService qcReportService,
            IDispatcherService dispatcherService,
            IQcbService qcbService,
            ICollimatorCalibrationModel collimatorCalibrationModel,
            ICollimatorConfigurationStore collimatorConfigurationStore,
            ApplicatorCompatibilityService applicatorCompatibilityService,
            ISafetyCheckModel safetyCheckModel,
            IBearerTokenUserSessionManager userSessionManager)
            : base(regionManager, eventAggregator, heraclesExternalSettings,
                gcbDataStore, uiStateMachine, logWriter, warmUpService, popUpService,
                dialogService, mainBoardModel, gcbIndicators,
                collimatorModel, collimatorConfigurationStore,
                safetyCheckModel, userSessionManager)
        {
            UserStore = userStore;
            QcReportService = qcReportService;
            QcbService = qcbService;
            CollimatorCalibrationModel = collimatorCalibrationModel;
            ApplicatorCompatibilityService = applicatorCompatibilityService;
            QcPlan = new QualityCheckPlan(dispatcherService, heraclesExternalSettings.QcFieldDuration);

            QcPlan.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(QualityCheckPlan.Fields))
                {
                    SelectedEmission = null;
                    FieldsViewSource.Source = QcPlan.Fields;
                }
            };

            FieldsSelectionModel = new TreatmentFieldSelectionModel(QcPlan);
            FieldsSelectionModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TreatmentFieldSelectionModel.TreatmentFieldListSelection) ||
                    e.PropertyName == nameof(TreatmentFieldSelectionModel.HoneycombSelection))
                    OnTreatmentFieldSelectionChanged();
                else if (e.PropertyName == nameof(TreatmentFieldSelectionModel.SelectedCollimatorType))
                {
                    CollimatorType = FieldsSelectionModel.SelectedCollimatorType;
                }
            };

            CollimatorModel.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ICollimatorModel.CollimatorConfigurations))
                {
                    GetAvailableTargetTypesAndEnergyLevels();
                }
            };

            GetAvailableTargetTypesAndEnergyLevels();
        }

        #endregion Contructors

        private IDictionary<ICollimatorConfiguration, ICollimatorCalibrationInfo> _collimatorConfigurationsWithCalibInfo =
            new Dictionary<ICollimatorConfiguration, ICollimatorCalibrationInfo>();

        #region Properties
        public IAuthorizedUserStore UserStore { get; }
        public QcReportService QcReportService { get; }
        public QualityCheckPlan QcPlan { get; }
        public IQcbService QcbService { get; }
        public ICollimatorCalibrationModel CollimatorCalibrationModel { get; }
        public ApplicatorCompatibilityService ApplicatorCompatibilityService { get; }

        private IQcSampleFieldEntry _selectedEmission;
        public IQcSampleFieldEntry SelectedEmission
        {
            get => _selectedEmission;
            set
            {
                if (SetProperty(ref _selectedEmission, value))
                {
                    ValidateCanExecuteCommands();
                }
            }
        }

        private CollectionViewSource _fieldsViewSource = new();
        public CollectionViewSource FieldsViewSource
        {
            get => _fieldsViewSource;
            set => SetProperty(ref _fieldsViewSource, value);
        }

        private Energy? _energy = null!;
        public Energy? Energy
        {
            get => _energy;
            set
            {
                if (!IsModified || GetUserConfirmation())
                {
                    if (SetProperty(ref _energy, value))
                    {
                        IsModified = false;
                        OnEnergyChanged(_energy);
                    }
                }
            }
        }

        private bool _prepareButtonIsEnabled;
        public bool PrepareButtonIsEnabled
        {
            get { return _prepareButtonIsEnabled; }
            set { SetProperty(ref _prepareButtonIsEnabled, value); }
        }

        public TreatmentFieldSelectionModel FieldsSelectionModel { get; }

        private SsdType? _ssdType;
        public SsdType? SsdType
        {
            get { return _ssdType; }
            set
            {
                SetProperty(ref _ssdType, value);
            }
        }

        private TargetType _type;
        public TargetType CollimatorType
        {
            get => _type;
            set
            {
                if (SetProperty(ref _type, value))
                {
                    SsdType = (value == TargetType.TargetType_30mm_SSD_7_Fields)
                        ? Core.Enums.SsdType.SsdType30mm : Core.Enums.SsdType.SsdType50mm;
                    
                    //FieldsSelectionModel?.OnSelectedCollimatorTypeChanged(value);
                }
            }
        }

        private IEnumerable<Energy> _availableEnergyLevels;
        public IEnumerable<Energy> AvailableEnergyLevels
        {
            get => _availableEnergyLevels;
            private set
            {
                if (SetProperty(ref _availableEnergyLevels, value))
                {
                    CheckForApplicatorCompatibility();
                }
            }
        }


        private IEnumerable<TargetType> _availableTargetTypeValues;
        public IEnumerable<TargetType> AvailableTargetTypeValues
        {
            get => _availableTargetTypeValues;
            private set
            {
                if (SetProperty(ref _availableTargetTypeValues, value))
                {
                    FieldsSelectionModel.SelectedCollimatorType = _availableTargetTypeValues.FirstOrDefault();
                }
            }
        }

        private bool _fullCollection = true;
        public bool FullCollection
        {
            get { return _fullCollection; }
            set
            {
                if (!Energy.HasValue)
                {
                    SetProperty(ref _fullCollection, value);
                    return;
                }

                string message;

                if (value) // 'Full' selected
                {
                    message = Application.Common.StringConstants.TreatmentConsole.QualityCheckFullModeConfirmationMessage;
                }
                else
                {
                    message = Application.Common.StringConstants.TreatmentConsole.QualityCheckCustomModeConfirmationMessage;
                }

                if (DialogService.Confirmation(StringConstants.Common.ConfirmationDialogTitle, message))
                {
                    if (SetProperty(ref _fullCollection, value))
                    {
                        CreateEntryCollection();
                    }
                }
            }
        }

        public bool IsModified;

        #endregion Properties

        #region Commands
        private DelegateCommand? _addCommand;
        public DelegateCommand AddCommand => _addCommand ??= new DelegateCommand(
            OnAddClicked,
            canExecuteMethod: CanAdd);

        private DelegateCommand? _removeCommand;
        public DelegateCommand RemoveCommand => _removeCommand ??= new DelegateCommand(
            () =>
            {
                if (DialogService.Confirmation(
                    StringConstants.Common.DeleteDialogTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanDeleteFieldConfirmationMessage))
                {
                    CurrentTask = new Xcc.Application.Helpers.ObservableTask(Task.Run(OnRemoveClicked));
                }
            },
            canExecuteMethod: CanRemove);


        #endregion Commands

        #region Public methods

        #endregion

        #region Private methods   

        public override void CheckForBoardRestart(GcbStateNew gcbState)
        {
            base.CheckForBoardRestart(gcbState);
            
            // we can't rely on UIStateMachine.State here, because it can be changed from other ViewModels
            if (gcbState == GcbStateNew.Startup) 
            {
                if ((IsCurrentViewModelRunning || 
                     UIStateMachine.TabName == ExternalTabName.QA) && // todo: this condition will allow to reset a QC plan even if SafetyCheck or Physics was ran 
                    UIStateMachine.IsPlanStaged)
                {
                    // unblock all the tabs, because GCB was rebooted, and we don't need to keep the QC plan
                    UIStateMachine.IsPlanStaged = false;

                    //reset the plan
                    _ = SetPlanUnloadTaskAsync();
                }
            }
        }

        private void LogInfoSystem(string message)
        {
            _ = LogWriter.LogAsync(message, LogRecordSeverity.Info, LogRecordType.System);
        }

        protected override void LogUserRequest(string actionMessage)
        {
            actionMessage = $"QualityCheck: {actionMessage}";

            base.LogUserRequest(actionMessage);
        }
        private bool CanAdd()
        {
            return
                !FullCollection &&
                FieldsSelectionModel.HoneycombSelection != null && Energy != null &&
                !QcPlan.ContainsField(CollimatorType, Energy.Value, FieldsSelectionModel.HoneycombSelection);
        }

        //protected override bool CanPrepare()
        //{
        //    if (UIStateMachine.LeftButton == null)
        //    {
        //        PrepareButtonIsEnabled = false;
        //        return false;
        //    }

        //    PrepareButtonIsEnabled = UIStateMachine.LeftButton.IsEnabled || HasValidPlanForTreatment();

        //    return QcModel.Fields != null
        //        && QcModel.Fields.Count > 0
        //        && base.CanPrepare();
        //}

        protected override bool CanPrepare()
        {
            if (UIStateMachine.LeftButton == null)
            {
                PrepareButtonIsEnabled = false;
                return false;
            }

            int selectedEmissionIndex = GetSelectedEmissionIndex();
            PrepareButtonIsEnabled =
                (UIStateMachine.LeftButton.IsEnabled || HasValidPlanForTreatment())
                && selectedEmissionIndex >= 0;

            var telemetry = GCBDataStore.SystemTelemetry;

            if (telemetry is null)
                return false;

            var stateResult = telemetry.ControlBoardState is GcbStateNew.Cold or
                                                                GcbStateNew.Primed or
                                                                GcbStateNew.Startup or
                                                                GcbStateNew.StandBy;

            return QcPlan.Fields is {Count: > 0}
                   && selectedEmissionIndex >= 0
                   && !CanResetTimers()
                   && !IsPreparing
                   && ApplicatorCompatibilityStatus.IsCompatible
                   && stateResult;
        }

        private bool CanRemove()
        {
            return !FullCollection &&
                (FieldsSelectionModel.TreatmentFieldListSelection != null && FieldsSelectionModel.TreatmentFieldListSelection.Count > 0);
        }

        // We cannot resume a QC plan, as it requires to redo all the measurements in a consistent way
        protected override bool CanResume()
        {
            return false;
        }


        protected override async Task PrepareAsync(bool tryKeepPrevPlan)
        {
            try
            {
                await CheckQCBoardStatusAsync();
            }
            catch (Exception ex)
            {
                // To prevent error loop on preparation callback, go to StandBy
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                    StringConstants.TreatmentConsole.PlanPreparationForQcBoardPingErrorMessage,
                    ex);
                IsCurrentViewModelRunning = false;
                ValidateCanExecuteCommands();
                return;
            }

            var tabName = ExternalTabName.QA;

            try
            {
                await base.PrepareAsync(tryKeepPrevPlan: false); // don't keep any prev plan in GCB

                UIStateMachine.IsPlanLoadedForTreatment = true;
                UIStateMachine.TabName = tabName;
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.TreatmentConsole.PlanPreparationErrorTitle,
                    StringConstants.TreatmentConsole.PlanPreparationForQcErrorMessage,
                    ex);
                IsCurrentViewModelRunning = false;
                ValidateCanExecuteCommands();
            }
            finally
            {
                SwitchExternalTab(tabName);
                EventAggregator?.GetEvent<RequestQaTabChangeEvent>().Publish(QaTabName.QualityChecks);
            }
        }


        protected override void CheckForApplicatorCompatibility()
        {
            var requiredParameters = ApplicatorParameters.FromValues(
                TargetType.TargetType_QC_Collimator,
                Core.Enums.Energy.Energy_50);
            
            ApplicatorCompatibilityStatus =
                (requiredParameters is null)
                ? ApplicatorCompatibilityStatus.Compatible
                : ApplicatorCompatibilityService.Check(requiredParameters.Value);
            HasMatchingPlanForTreatment = HasValidPlanForTreatment();

            ValidateCanExecuteCommands();
        }

        protected override void ValidateCanExecuteCommands()
        {
            base.ValidateCanExecuteCommands();

            AddCommand.RaiseCanExecuteChanged();
            RemoveCommand.RaiseCanExecuteChanged();
        }

        protected override async Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry)
        {
            try
            {
                await Semaphore.WaitAsync();
                if (ActiveEmissionIndex < 0 || ActiveEmissionIndex >= QcPlan.Fields.Count)
                {
                    return;
                }

                var field = QcPlan.Fields[ActiveEmissionIndex];
                if (GcbState == GcbStateNew.Emission)
                {
                    float timerValue = telemetry.PrimaryTimerValue;
                    field.Actual = Convert.ToSingle(XrayPointStartTime + timerValue);
                    UpdateBeamOnProgress(
                        field.Duration,
                        Convert.ToSingle(XrayTime + timerValue));
                    Debug.WriteLine(
                        $"Update treatment field {field.DisplayValue} with actual = {field.Actual}");
                }
                else if (PreviousGcbState == GcbStateNew.Emission)
                {
                    await MainBoardModel.UpdateCurrentEmissionFromGCB();
                    if (MainBoardModel.CurrentEmission is { } emission)
                    {
                        field.Actual = emission.ActualDuration;
                        field.IsDone =
                            emission.RemainingPointTime < PlanCompletedThreshold;
                        RecalculateInitialXrayTime();
                        UpdateBeamOnProgress(field.Duration, XrayTime);
                        _ = LogWriter.LogAsync(
                            $"Query emission response: TotalPointTime={emission.TotalPointTime} RemainingPointTime={emission.RemainingPointTime} Actual={field.Actual}",
                            LogRecordSeverity.Info,
                            LogRecordType.System);
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
            SelectedEmission = null;
            FieldsSelectionModel.SelectField(null);

            await base.OnClearPlanClicked();
        }

        protected override async Task OnBeamOnClicked()
        {
            IsCurrentViewModelRunning = true;

            using var tokenSource = new CancellationTokenSource();
            Task updateAfterEmissionTask = Task.CompletedTask;
            bool startAttempted = false;
            bool stopCompleted = false;
            try
            {
                if (ActiveEmissionIndex < 0 || ActiveEmissionIndex >= QcPlan.Fields.Count)
                {
                    throw new InvalidOperationException("No QC emission is prepared.");
                }

                await CheckQCBoardStatusAsync();
                _ = GCBDataStore.SystemTelemetry
                    ?? throw new Exception("GCB telemetry connection lost.");

                var field = QcPlan.Fields[ActiveEmissionIndex];
                RecalculateInitialXrayTime();
                XrayPointStartTime = field.Actual;
                UpdateBeamOnProgress(field.Duration, XrayTime);

                startAttempted = true;
                var startStatus = await QcbService.StartQCReadingsAsync();
                if (startStatus != QcbCommandResponseStatus.StartConfirmed)
                {
                    throw new InvalidOperationException(
                        "Main-control rejected the QC reading start command.");
                }

                UIStateMachine.RequestStateSwitch(UIMacroState.Emission);
                _ = LogWriter.LogAsync(
                    $"Run QC emission {ActiveEmissionIndex + 1} by {UserStore.AuthorizedUser.EmailAddress}",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                updateAfterEmissionTask = Task.Run(
                    () => UpdateAfterEmission(tokenSource.Token),
                    tokenSource.Token);
                Task beamOn = MainBoardModel.BeamOn();
                await beamOn;
                await updateAfterEmissionTask;

                QcReadings readings = await QcbService.StopQCReadingsAsync();
                stopCompleted = true;

                await MainBoardModel.UpdateCurrentEmissionFromGCB();
                if (MainBoardModel.CurrentEmission is not { } completedEmission)
                {
                    throw new InvalidOperationException(
                        "Main-control returned no completed QC emission.");
                }

                field.Actual = completedEmission.ActualDuration;
                field.IsDone =
                    completedEmission.RemainingPointTime < PlanCompletedThreshold;
                field.Intensities = readings;
                RecalculateInitialXrayTime();

                if (!field.IsDone)
                {
                    throw new InvalidOperationException(
                        Application.Common.StringConstants.TreatmentConsole.QualityCheckIncompleteEmissionErrorMessage);
                }

                PopUpService.LogAndShowMessage(
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckNotificationTitle,
                    $"{Application.Common.StringConstants.TreatmentConsole.QualityCheckCompletionNotification}{Environment.NewLine}{Application.Common.StringConstants.TreatmentConsole.SwitchToReportsSuggestionMessage}",
                    ReportType.Info,
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                await MainBoardModel.ResetTimers();
                await MainBoardModel.ClearPlan();
                UIStateMachine.IsPlanStaged = false;
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                await QcReportService.SaveQcSampleReportAsync(field);
                await SetPlanUnloadTaskAsync();
                EventAggregator!.GetEvent<QualityCheckFinishedEvent>().Publish();
            }
            catch (TaskCanceledException ex)
            {
                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);
                PopUpService.ShowMessage(
                    StringConstants.TreatmentConsole.EmissionTitle,
                    StringConstants.TreatmentConsole.EmissionInterruptedError,
                    ReportType.Error);
                _ = LogWriter.LogAsync(
                    $"QC emission was cancelled: {ex.Message}",
                    LogRecordSeverity.Info,
                    LogRecordType.System);
            }
            catch (DataServiceException ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.SaveErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckSaveErrorMessage,
                    ex);
            }
            catch (InvalidOperationException ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckTitle,
                    ex.Message);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckTitle,
                    Application.Common.StringConstants.TreatmentConsole.QualityCheckStartErrorMessage,
                    ex);
            }
            finally
            {
                await tokenSource.CancelAsync();
                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);
                if (startAttempted && !stopCompleted)
                {
                    try
                    {
                        _ = await QcbService.StopQCReadingsAsync();
                    }
                    catch (Exception cleanupException)
                    {
                        _ = LogWriter.LogAsync(
                            $"QC cleanup stop failed: {cleanupException.Message}",
                            LogRecordSeverity.Error,
                            LogRecordType.System);
                    }
                }
                SelectedEmission = null;
                FieldsSelectionModel.SelectField(null);
                IsCurrentViewModelRunning = false;
            }
        }

        private async Task CheckQCBoardStatusAsync()
        {
            bool qcBoardIsAlive = await QcbService.PingBoardAsync();
            if (!qcBoardIsAlive)
            {
                throw new Exception("QCBoard does not respond");
            }
        }

        protected override Task SetPlanUnloadTaskAsync()
        {
            UIStateMachine.IsPlanLoadedForTreatment = false;
            ActiveEmissionIndex = -1;

            foreach (var field in QcPlan.Fields)
            {
                field.Actual = 0.0f;
                field.IsDone = false;
                field.Intensities = null;
            }

            return Task.CompletedTask;
        }

        protected override void RecalculateInitialXrayTime()
        {
            XrayTime = ActiveEmissionIndex >= 0
                       && ActiveEmissionIndex < QcPlan.Fields.Count
                ? QcPlan.Fields[ActiveEmissionIndex].Actual
                : 0.0;
        }


        protected override GcbOperationalPoint BuildGcbOperationalPoint(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= QcPlan.Fields.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldIndex));
            }

            var field = QcPlan.Fields[fieldIndex];
            var collimatorCalibConfig =
                _collimatorConfigurationsWithCalibInfo[field.Configuration];
            var fieldCalibConfig =
                collimatorCalibConfig.GetCoilConfiguration(field.Name).Value;
            float totalTime = (float)field.Duration;

            return new GcbOperationalPoint
            {
                TotalPointTime = totalTime,
                RemainingPointTime = totalTime - (float)field.Actual,
                SetpointKv = EnergyConverter.Convert(field.Energy),
                TargetMA = Convert.ToSingle(field.Current),
                FilamentSetpoint = (float)collimatorCalibConfig.HeaterCurrent,
                XCoilSetpoint = (float)fieldCalibConfig.XDeflectionCurrent,
                YCoilSetpoint = (float)fieldCalibConfig.YDeflectionCurrent,
                FocusCoilSetpoint = (float)fieldCalibConfig.FocusCurrent
            };
        }

        protected override int GetInitialEmissionIndex() =>
            GetSelectedEmissionIndex();

        private int GetSelectedEmissionIndex()
        {
            int selectedIndex = QcPlan.Fields.IndexOf(SelectedEmission);
            if (selectedIndex < 0)
            {
                return -1;
            }

            var selectedField = QcPlan.Fields[selectedIndex];
            return !selectedField.IsDone
                   && selectedField.Duration - selectedField.Actual >= PlanCompletedThreshold
                ? selectedIndex
                : -1;
        }

        protected override int FindNextPendingEmissionIndex(int startIndex)
        {
            for (int index = Math.Max(0, startIndex); index < QcPlan.Fields.Count; index++)
            {
                var field = QcPlan.Fields[index];
                if (!field.IsDone
                    && field.Duration - field.Actual >= PlanCompletedThreshold)
                {
                    return index;
                }
            }

            return -1;
        }

        private bool GetUserConfirmation()
        {
            bool dialogResult = false;

            DialogService.Report(
                Application.Common.StringConstants.TreatmentConsole.QualityCheckDiscardChangesConfirmationTitle,
                Application.Common.StringConstants.TreatmentConsole.QualityCheckDiscardChangesConfirmationMessage,
                ReportType.Confirmation,
                result =>
                {
                    dialogResult = (result.Result == ButtonResult.OK);
                });

            return dialogResult;
        }

        private void CreateEntryCollection()
        {
            if (!Energy.HasValue)
                return;

            try
            {
                QcPlan.ResetEntries();

                var collection = new List<IQcSampleFieldEntry>();

                foreach (var (configuration, configInfo) in _collimatorConfigurationsWithCalibInfo)
                {
                    if (configuration.Energy != Energy.Value)
                        continue; // skip all other energy configurations 

                    var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(configuration.Type);

                    if (FullCollection)
                    {
                        foreach (var field in fieldNameMapping)
                        {
                            var qcSampleFieldEntry = new QcSampleFieldEntry(configuration, configInfo.HeaterCurrent)
                            {
                                Name = field.Value,
                                DisplayValue = field.Key
                            };
                            QcPlan.AddField(qcSampleFieldEntry);
                        }
                    }
                    else
                    {
                        var centralCellIndex = TargetTypeConverter.GetCentralCellIndex(configuration.Type);
                        var centralFieldName = fieldNameMapping[centralCellIndex];

                        var qcSampleFieldEntry = new QcSampleFieldEntry(configuration, configInfo.HeaterCurrent)
                        {
                            Name = centralFieldName,
                            DisplayValue = centralCellIndex
                        };
                        QcPlan.AddField(qcSampleFieldEntry);
                    }
                }
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanCreateCollectionError,
                    ex);
            }
        }

        private void OnEnergyChanged(Energy? energy)
        {
            if (!energy.HasValue)
                return;

            try
            {
                AvailableTargetTypeValues = _collimatorConfigurationsWithCalibInfo.Keys
                    .Where(c => energy == c.Energy)
                    .Select(c => c.Type).Distinct().Order();

                CreateEntryCollection();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanCreateCollectionError,
                    ex);
            }

            CheckForApplicatorCompatibility(); 

            ValidateCanExecuteCommands();
        }

        private bool HasValidPlanForTreatment()
        {
            return ApplicatorCompatibilityStatus.IsCompatible;
        }

        private void OnTreatmentFieldSelectionChanged()
        {
            AddCommand?.RaiseCanExecuteChanged();
            RemoveCommand?.RaiseCanExecuteChanged();
        }

        private void OnRemoveClicked()
        {
            try
            {
                foreach (object selectedEntry in FieldsSelectionModel.TreatmentFieldListSelection)
                {
                    IQcSampleFieldEntry qcSampleFieldEntry = selectedEntry as IQcSampleFieldEntry;

                    if (qcSampleFieldEntry != null)
                    {
                        QcPlan.RemoveField(qcSampleFieldEntry);
                        IsModified = true;
                    }
                    else
                    {
                        ITreatmentFieldEntry treatmentFieldEntry = selectedEntry as ITreatmentFieldEntry;
                        if (treatmentFieldEntry != null)
                        {
                            QcPlan.RemoveField(CollimatorType, Energy.Value, treatmentFieldEntry);
                            IsModified = true;
                        }
                    }
                }

                ValidateCanExecuteCommands();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.PlanOperationErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanRemoveFieldErrorMessage,
                    ex);
            }
        }

        private void GetAvailableTargetTypesAndEnergyLevels()
        {
            try
            {
                // We first empty both lists to prevent user from selecting anything actually unavailable
                AvailableTargetTypeValues = new List<TargetType>();
                AvailableEnergyLevels = new List<Energy>();

                CurrentTask = new ObservableTask(PrepareAvailableOptionsToSelect());
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.ApplicatorCoilConfigurationLoadError,
                    ex);
            }
        }

        private Task PrepareAvailableOptionsToSelect()
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (CollimatorModel.CollimatorConfigurations is null)
                        return;

                    var collimatorConfigurations = CollimatorModel.CollimatorConfigurations.Where(c => c.Type != TargetType.TargetType_QC_Collimator);

                    var calibDataStore = await CollimatorCalibrationModel.FetchCalibrationDataAsync();

                    // filter out any configurations that do not have approved default preset
                    calibDataStore = calibDataStore.Filter(x => x.CollimatorConfiguration?.DefaultPreset?.IsApproved ?? false);
                    
                    // We select only those configurations that we have calibration data for (coil currents, heater current and mangnetometer refs)
                    // TODO: there are some concerns on persistency of these calib. data in future,
                    // maybe we need to get a copy of the list of calib info here
                    _collimatorConfigurationsWithCalibInfo =
                        collimatorConfigurations.Select(c => (c, calibDataStore[c.Id])).Where(value => value.Item2 != null).ToDictionary();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableTargetTypeValues = _collimatorConfigurationsWithCalibInfo.Keys.Select(c => c.Type).Distinct().Order();
                        AvailableEnergyLevels = _collimatorConfigurationsWithCalibInfo.Keys.Select(c => c.Energy).Distinct().Order();
                    });
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync($"PrepareAvailableOptionsToSelect failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                }
            });
        }

        private void OnAddClicked()
        {
            try
            {
                // TODO: need to refactor this coupled dependency over filamentSetpoint & configuration:
                FieldsSelectionModel.HoneycombSelection.Energy = Energy.Value;
                var (collimatorConfig, configInfo) = _collimatorConfigurationsWithCalibInfo.FirstOrDefault( kv =>
                    kv.Key.Type == CollimatorType &&
                    kv.Key.Energy == Energy.Value);

                var selectedTreatmentField = FieldsSelectionModel.HoneycombSelection;
                if (!QcPlan.ContainsField(CollimatorType, Energy.Value, selectedTreatmentField))
                {
                    double filamentSetpoint = configInfo.HeaterCurrent;
                    var fieldNameMapping = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(CollimatorType);

                    FieldsSelectionModel.SelectField(
                        QcPlan.AddField(
                            new QcSampleFieldEntry(collimatorConfig, filamentSetpoint)
                            {
                                Name = selectedTreatmentField.Name,
                                DisplayValue = TargetTypeConverter.GetBackwardFieldNameMapping(
                                    fieldNameMapping, selectedTreatmentField.Name)
                            })
                        );
                    IsModified = true;
                }

                ValidateCanExecuteCommands();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.PlanOperationErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanAddFieldErrorMessage,
                    ex);
            }
        }

        #endregion Private methods
    }

}
