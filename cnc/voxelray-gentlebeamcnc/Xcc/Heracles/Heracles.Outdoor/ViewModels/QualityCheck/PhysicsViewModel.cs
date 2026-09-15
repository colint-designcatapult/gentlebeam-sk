using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.AppLayer.Patient.Planning;
using Heracles.Application.AppLayer.QualityAssurance.QualityCheck;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Enums;
using Heracles.Application.Helpers;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.RDBMS;
using Heracles.External.Models;
using Heracles.External.Models.CollimatorConfiguration;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Warmup;
using Xcc.Application.Common;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Domain.QualityAssurance;
using Xcc.Application.Domain.System;
using Xcc.Application.Forms;
using Xcc.Application.Helpers;
using Xcc.Core.Common;
using Xcc.Core.Constants;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.External.ViewModels.QualityCheck
{
    public class PhysicsViewModel : OperatePlanViewModelBase
    {
        private const double DefaultCalibrationDurationSec = 60;
        private const double MinCalibrationDurationSec = 0.1;
        private const double MaxCalibrationDurationSec = 180;

        #region Contructors
        public PhysicsViewModel() : base()
        { }

        public PhysicsViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IHeraclesExternalSettings heraclesExternalSettings,
            IGCBDataStore gcbDataStore,
            ILogWriter logWriter,
            IUIStateMachine uiStateMachine,
            IDialogService dialogService,
            IWarmupService warmUpService,
            ICollimatorModel collimatorModel,
            CollimatorService collimatorService,
            IPopUpService popUpService,
            IMainBoardModel mainBoardModel,
            IGcbIndicators gcbIndicators,
            IAuthorizedUserStore userStore,
            IDispatcherService dispatcherService,
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
            CollimatorService = collimatorService;
            UserStore = userStore;
            CollimatorCalibrationModel = collimatorCalibrationModel;
            ApplicatorCompatibilityService = applicatorCompatibilityService;
            PhysicsPlan = new QualityCheckPlan(dispatcherService, DefaultCalibrationDurationSec);
            
            CollimatorModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(ICollimatorModel.CollimatorConfigurations))
                {
                    GetAvailableTargetTypesAndEnergyLevels(forceRefresh: true);
                }
            };


            GetAvailableTargetTypesAndEnergyLevels();

            SelectedConfiguration = new EmissionConfiguration();
            SelectedConfiguration.IsValidChanged += (s, e) => PrepareCommand.RaiseCanExecuteChanged();
            SelectedConfiguration.IsModifiedChanged += (s, e) => PrepareCommand.RaiseCanExecuteChanged();
            SelectedConfiguration.PropertyChanged += SelectedConfigurationOnPropertyChanged;
        }

        #endregion Contructors
        
        #region Properties

        public CollimatorService CollimatorService { get; }
        public IAuthorizedUserStore UserStore { get; }
        public QualityCheckPlan PhysicsPlan { get; }
        public ICollimatorCalibrationModel CollimatorCalibrationModel { get; }
        public ApplicatorCompatibilityService ApplicatorCompatibilityService { get; }

        private Energy? _energy = null!;
        public Energy? Energy
        {
            get => _energy;
            set
            {
                if (SetProperty(ref _energy, value))
                {
                    OnEnergyChanged(_energy);
                }
            }
        }

        private bool _prepareButtonIsEnabled;
        public bool PrepareButtonIsEnabled
        {
            get { return _prepareButtonIsEnabled; }
            set { SetProperty(ref _prepareButtonIsEnabled, value); }
        }

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

                    OnApplicatorSizeChanged();

                    //FieldsSelectionModel?.OnSelectedCollimatorTypeChanged(value);
                }
            }
        }

        private IEnumerable<Energy> _availableEnergyLevels;
        public IEnumerable<Energy> AvailableEnergyLevels
        {
            get => _availableEnergyLevels;
            private set => SetProperty(ref _availableEnergyLevels, value);
        }

        private IEnumerable<TargetType> _availableTargetTypeValues;
        public IEnumerable<TargetType> AvailableTargetTypeValues
        {
            get => _availableTargetTypeValues;
            private set
            {
                if (SetProperty(ref _availableTargetTypeValues, value))
                {
                    CollimatorType = _availableTargetTypeValues.FirstOrDefault();

                    CheckForApplicatorCompatibility();
                }
            }
        }

        private bool _fullCollection = true;
        public bool FullCollection
        {
            get => _fullCollection;
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

        private EmissionConfiguration? _selectedConfiguration;
        public EmissionConfiguration? SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (SetProperty(ref _selectedConfiguration, value))
                {
                    value?.DurationForm.GetProperty<double>(nameof(PhysicsForm.Duration)).Subscribe((_, duration) =>
                    {
                        OnDurationChanged(duration);
                        PrepareCommand.RaiseCanExecuteChanged();
                    });
                }
            }
        }
        #endregion Properties

        #region Commands
        //Left buttons and progress bars
        private DelegateCommand? _fetchPlanCommand;
        public DelegateCommand FetchCommand => _fetchPlanCommand ??= new DelegateCommand(
            async () =>
            {
                await OnFetchClicked();
            }, 
            canExecuteMethod: ()=> true); // todo: set proper CanExecute


        #endregion Commands


        #region Private methods   

        private void SelectedConfigurationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EmissionConfiguration.CoilConfiguration))
            {
                PrepareCommand.RaiseCanExecuteChanged();
            }
        }

        private async Task OnFetchClicked()
        {
            try
            {
                await CollimatorService.UpdateCollimatorModelAsync();

                _selectedConfiguration?.ResetValues();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.ApplicatorCoilConfigurationLoadError,
                    ex);
            }
            finally
            {
                ValidateCanExecuteCommands();
            }
        }

        protected override void LogUserRequest(string actionMessage)
        {
            actionMessage = $"QualityCheck: {actionMessage}";

            base.LogUserRequest(actionMessage);
        }
        
        protected override bool CanPrepare()
        {
            if (UIStateMachine.LeftButton == null)
            {
                PrepareButtonIsEnabled = false;
                return false;
            }

            PrepareButtonIsEnabled = UIStateMachine.LeftButton.IsEnabled || HasValidPlanForTreatment();

            var telemetry = GCBDataStore.SystemTelemetry;

            if (telemetry is null)
                return false;

            var stateResult = telemetry.ControlBoardState is
                GcbStateNew.Cold or
                GcbStateNew.Primed or
                GcbStateNew.Startup;
            
            return PhysicsPlan.Fields is {Count: > 0}
                   && !CanResetTimers()
                   && !IsPreparing
                   && ApplicatorCompatibilityStatus.IsCompatible
                   && stateResult
                   && SelectedConfiguration is { IsSet: true}
                   && SelectedConfiguration.GetIsValid();
        }

        protected override async Task PrepareAsync(bool tryKeepPrevPlan)
        {
            var tabName = ExternalTabName.QA;

            try
            {
                _preparationEmissionConfigurationState = SelectedConfiguration;

                await base.PrepareAsync(tryKeepPrevPlan: false); // don't keep any prev plan in GCB

                UIStateMachine.IsPlanLoadedForTreatment = true;
                UIStateMachine.TabName = tabName;
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
                SwitchExternalTab(tabName);
                EventAggregator?.GetEvent<RequestQaTabChangeEvent>().Publish(QaTabName.Physics);
            }
        }


        protected override void CheckForApplicatorCompatibility()
        {
            var requiredParameters = ApplicatorParameters.FromValues(CollimatorType, Energy);

            ApplicatorCompatibilityStatus =
                (requiredParameters is null)
                ? ApplicatorCompatibilityStatus.Compatible
                : ApplicatorCompatibilityService.Check(requiredParameters.Value);
            
            HasMatchingPlanForTreatment = HasValidPlanForTreatment();

            ValidateCanExecuteCommands();
        }

        protected override async Task UpdateEmissionTreatmentField(ISystemTelemetry telemetry)
        {
            try
            {
                await Semaphore.WaitAsync();
                if (ActiveEmissionIndex < 0
                    || ActiveEmissionIndex >= PhysicsPlan.Fields.Count)
                {
                    return;
                }

                var field = PhysicsPlan.Fields[ActiveEmissionIndex];
                if (GcbState == GcbStateNew.Emission)
                {
                    float timerValue = telemetry.PrimaryTimerValue;
                    field.Actual = Convert.ToSingle(XrayPointStartTime + timerValue);
                    UpdateBeamOnProgress(
                        PhysicsPlan.TotalDuration,
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
                        UpdateBeamOnProgress(PhysicsPlan.TotalDuration, XrayTime);
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

        protected override async Task OnBeamOnClicked()
        {
            IsCurrentViewModelRunning = true;

            using var tokenSource = new CancellationTokenSource();
            Task updateAfterEmissionTask = Task.CompletedTask;

            try
            {
                if (ActiveEmissionIndex < 0
                    || ActiveEmissionIndex >= PhysicsPlan.Fields.Count)
                {
                    throw new InvalidOperationException(
                        "No physics emission is prepared.");
                }

                _ = GCBDataStore.SystemTelemetry
                    ?? throw new Exception("GCB telemetry connection lost.");
                var field = PhysicsPlan.Fields[ActiveEmissionIndex];
                RecalculateInitialXrayTime();
                XrayPointStartTime = field.Actual;
                UpdateBeamOnProgress(PhysicsPlan.TotalDuration, XrayTime);

                UIStateMachine.RequestStateSwitch(UIMacroState.Emission);
                _ = LogWriter.LogAsync(
                    $"Run Physics emission {ActiveEmissionIndex + 1} by {UserStore.AuthorizedUser!.EmailAddress}",
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                updateAfterEmissionTask = Task.Run(
                    () => UpdateAfterEmission(tokenSource.Token),
                    tokenSource.Token);
                await MainBoardModel.BeamOn();
                await updateAfterEmissionTask;
                await MainBoardModel.UpdateCurrentEmissionFromGCB();

                if (MainBoardModel.CurrentEmission is not { } completedEmission)
                {
                    throw new InvalidOperationException(
                        "Main-control returned no completed physics emission.");
                }

                field.Actual = completedEmission.ActualDuration;
                field.IsDone =
                    completedEmission.RemainingPointTime < PlanCompletedThreshold;
                RecalculateInitialXrayTime();

                int nextIndex = FindNextPendingEmissionIndex(ActiveEmissionIndex + 1);
                if (nextIndex >= 0)
                {
                    await WarmUpAsync();
                    UIStateMachine.RequestStateSwitch(UIMacroState.Preparation);
                    await PrepareNextEmissionAsync(nextIndex);
                    UIStateMachine.IsPlanStaged = MainBoardModel.IsPlanStaged;
                    return;
                }

                if (!PhysicsPlan.Fields.All(item => item.IsDone))
                {
                    throw new InvalidOperationException(
                        "Physics emissions did not complete.");
                }

                PopUpService.LogAndShowMessage(
                    Application.Common.StringConstants.TreatmentConsole.PhysicsNotificationTitle,
                    Application.Common.StringConstants.TreatmentConsole.PhysicsExecutionCompletionNotification,
                    ReportType.Info,
                    LogRecordSeverity.Info,
                    LogRecordType.System);

                await MainBoardModel.ResetTimers();
                await MainBoardModel.ClearPlan();
                UIStateMachine.IsPlanStaged = false;
                UIStateMachine.RequestStateSwitch(UIMacroState.StandBy);
                await SetPlanUnloadTaskAsync();
            }
            catch (TaskCanceledException ex)
            {
                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);
                PopUpService.ShowMessage(
                    Application.Common.StringConstants.TreatmentConsole.PhysicsErrorTitle,
                    StringConstants.TreatmentConsole.EmissionInterruptedError,
                    ReportType.Error);
                _ = LogWriter.LogAsync(
                    $"Physics emission was cancelled: {ex.Message}",
                    LogRecordSeverity.Info,
                    LogRecordType.System);
            }
            catch (InvalidOperationException ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.PhysicsErrorTitle,
                    ex.Message);
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    Application.Common.StringConstants.TreatmentConsole.PhysicsErrorTitle,
                    StringConstants.TreatmentConsole.EmissionInterruptedError,
                    ex);
            }
            finally
            {
                await tokenSource.CancelAsync();
                await WaitAndIgnoreTaskExceptionsAsync(updateAfterEmissionTask);
                IsCurrentViewModelRunning = false;
            }
        }
        
        protected override Task SetPlanUnloadTaskAsync()
        {
            UIStateMachine.IsPlanLoadedForTreatment = false;

            PhysicsPlan.ResetEntryCollectionActualTime();

            return Task.CompletedTask;
        }

        protected override void RecalculateInitialXrayTime()
        {
            var fields = PhysicsPlan.Fields;
            if (fields is null || fields.Count == 0)
                XrayTime = 0.0;
            else
                XrayTime = fields.Sum(tf => tf.Actual);
        }

        protected override GcbOperationalPoint BuildGcbOperationalPoint(int fieldIndex)
        {
            if (fieldIndex < 0 || fieldIndex >= PhysicsPlan.Fields.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(fieldIndex));
            }

            var field = PhysicsPlan.Fields[fieldIndex];
            var coilValues =
                _preparationEmissionConfigurationState.CoilConfiguration.GetValue();
            float totalTime = (float)field.Duration;

            return new GcbOperationalPoint
            {
                TotalPointTime = totalTime,
                RemainingPointTime = totalTime - (float)field.Actual,
                SetpointKv = EnergyConverter.Convert(field.Energy),
                TargetMA = Convert.ToSingle(field.Current),
                FilamentSetpoint =
                    (float)_preparationEmissionConfigurationState.HeaterCurrent.HeaterCurrent!,
                XCoilSetpoint = (float)coilValues.XDeflectionCurrent,
                YCoilSetpoint = (float)coilValues.YDeflectionCurrent,
                FocusCoilSetpoint = (float)coilValues.FocusCurrent
            };
        }

        protected override int FindNextPendingEmissionIndex(int startIndex)
        {
            for (int index = Math.Max(0, startIndex);
                 index < PhysicsPlan.Fields.Count;
                 index++)
            {
                var field = PhysicsPlan.Fields[index];
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
            return DialogService.Confirmation(
                Application.Common.StringConstants.TreatmentConsole.QualityCheckDiscardChangesConfirmationTitle,
                Application.Common.StringConstants.TreatmentConsole.QualityCheckDiscardChangesConfirmationMessage);
        }

        private void CreateEntryCollection()
        {
            if (!Energy.HasValue)
                return;

            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    SelectedConfiguration.Reset();

                    PhysicsPlan.ResetEntries();

                    var matchingConfiguration = _collimatorConfigurationsWithCalibInfo.FirstOrDefault(
                        c => c.Key.Energy == Energy.Value && c.Key.Type == CollimatorType);

                    if (matchingConfiguration.Key != null)
                    {
                        var (configuration, configInfo) = matchingConfiguration;
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
                                PhysicsPlan.AddField(qcSampleFieldEntry);
                            }

                            var fieldName = fieldNameMapping.Values.First();
                            var calibrationConfig = configInfo.GetCoilConfiguration(fieldName);
                            if (calibrationConfig != null)
                            {
                                SelectedConfiguration.Set(
                                    calibrationConfig.Value.XDeflectionCurrent,
                                    calibrationConfig.Value.YDeflectionCurrent,
                                    calibrationConfig.Value.FocusCurrent,
                                    configInfo.HeaterCurrent,
                                    configInfo.CollimatorConfiguration.ReferencedDoseRate);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.PlanCreateCollectionError,
                    ex);
            }
            finally
            {
                CheckForApplicatorCompatibility();
            }
        }

        private void OnDurationChanged(double duration)
        {
            try
            {
                if (duration is >= MinCalibrationDurationSec and <= MaxCalibrationDurationSec)
                {
                    PhysicsPlan.SetDuration(duration);
                    if (PhysicsPlan.IsEmpty)
                    {
                        CreateEntryCollection();
                    }
                    else
                    {
                        CheckForApplicatorCompatibility();
                    }
                }
                //else
                //{
                //    QcPlan.ResetEntries();
                //    CheckForTargetMismatch();
                //}
            }
            catch(Exception)
            {
                PhysicsPlan.ResetEntries();
                CheckForApplicatorCompatibility();
            }
        }

        private void OnEnergyChanged(Energy? energy)
        {
            CreateEntryCollection();

            ValidateCanExecuteCommands();
        }

        private bool HasValidPlanForTreatment()
        {
            return PhysicsPlan?.Fields?.Any() ?? false;
        }

        private void OnApplicatorSizeChanged()
        {
            AvailableEnergyLevels = _collimatorConfigurationsWithCalibInfo.Keys
                .Where(c => CollimatorType == c.Type)
                .Select(c => c.Energy).Distinct().Order();

            PhysicsPlan.ResetEntries(); 
            CreateEntryCollection();
        }

        private void GetAvailableTargetTypesAndEnergyLevels(bool forceRefresh = false)
        {
            try
            {
                // We first empty both lists to prevent user from selecting anything actually unavailable
                AvailableTargetTypeValues = new List<TargetType>();
                AvailableEnergyLevels = new List<Energy>();

                CurrentTask = new ObservableTask(PrepareAvailableOptionsToSelect(forceRefresh));
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(
                    StringConstants.Common.ErrorTitle,
                    Application.Common.StringConstants.TreatmentConsole.ApplicatorCoilConfigurationLoadError,
                    ex);
            }
        }

        private Task PrepareAvailableOptionsToSelect(bool forceRefresh)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (CollimatorModel.CollimatorConfigurations is null)
                        return;

                    var collimatorConfigurations = CollimatorModel.CollimatorConfigurations.Where(c => c.Type != TargetType.TargetType_QC_Collimator);

                    var calibDataStore = await CollimatorCalibrationModel.FetchCalibrationDataAsync(forceRefresh);

                    // We select only those configurations that we have calibration data for (coil currents, heater current and mangnetometer refs)
                    // TODO: there are some concerns on persistency of these calib. data in future,
                    // maybe we need to get a copy of the list of calib info here
                    _collimatorConfigurationsWithCalibInfo =
                        collimatorConfigurations.Select(c => (c, calibDataStore[c.Id])).Where(value => value.Item2 != null).ToDictionary();

                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        AvailableTargetTypeValues = _collimatorConfigurationsWithCalibInfo.Keys.Select(c => c.Type).Distinct().Order();
                    });
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync($"PrepareAvailableOptionsToSelect failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                }
            });
        }
        
        #endregion Private methods

        /// <summary>
        /// It's used to store configuration that will be used for preparation of the plan
        /// </summary>
        private EmissionConfiguration? _preparationEmissionConfigurationState; // todo: we can avoid using this field, if block corresponding UI controls

        public class EmissionConfiguration : DirtyFlaggedBindableBase
        {
            public EmissionConfiguration()
            {
                CoilConfiguration = new CoilConfigurationForm();
                DurationForm = new PhysicsForm(null);
            }

            private CoilConfigurationForm _coilConfiguration = new CoilConfigurationForm();
            public CoilConfigurationForm CoilConfiguration
            {
                get => _coilConfiguration;
                set
                {
                    if (SetPropertyWithDirtyFlag(ref _coilConfiguration, value))
                    {
                        Validate(value);
                    }
                }
            }

            private HeaterCurrentBindable? _heaterCurrent;
            public HeaterCurrentBindable? HeaterCurrent
            {
                get => _heaterCurrent;
                set
                {
                    if (_heaterCurrent is not null)
                    {
                        _heaterCurrent.IsValidChanged -= OnIsValidChanged;
                        _heaterCurrent.IsModifiedChanged -= OnIsModifiedChanged;
                    }

                    if (SetPropertyWithDirtyFlag(ref _heaterCurrent, value))
                    {
                        if (_heaterCurrent is not null)
                        {
                            _heaterCurrent.IsValidChanged += OnIsValidChanged;
                            _heaterCurrent.IsModifiedChanged += OnIsModifiedChanged;
                        }
                    }
                }
            }

            private PhysicsForm _durationForm;
            public PhysicsForm DurationForm
            {
                get => _durationForm;
                set => SetPropertyWithDirtyFlag(ref _durationForm, value);
            }

            public bool IsSet =>
                HeaterCurrent is {IsSet: true} &&
                CoilConfiguration is { IsValid: true };

            public bool GetIsValid()
            {
                return IsValid &&                       
                       HeaterCurrent?.IsValid == true &&
                       CoilConfiguration?.IsValid == true;
            }

            public void Set(
                double? xDeflectionCurrent,
                double? yDeflectionCurrent,
                double? focusCurrent,
                double? heaterCurrent,
                double doseRate)
            {
                var coilConfig = new CoilConfigurationForm();
                coilConfig.XDeflectionCurrent = xDeflectionCurrent;
                coilConfig.YDeflectionCurrent = yDeflectionCurrent;
                coilConfig.FocusCurrent = focusCurrent;

                CoilConfiguration = coilConfig;

                HeaterCurrent = new HeaterCurrentBindable
                {
                    HeaterCurrent = heaterCurrent
                };
                DurationForm.DoseRate = doseRate;
            }

            public void Reset()
            {
                HeaterCurrent = null;
                CoilConfiguration = null;
                DurationForm.Duration = DefaultCalibrationDurationSec;
                DurationForm.DoseRate = DurationForm.DefaultDoseRate;
            }

            public void ResetValues()
            {
                HeaterCurrent = null;
                
                CoilConfiguration.ResetValues();

                DurationForm.Duration = DefaultCalibrationDurationSec;
                DurationForm.DoseRate = null;
            }
        }

        private IDictionary<ICollimatorConfiguration, ICollimatorCalibrationInfo> _collimatorConfigurationsWithCalibInfo =
            new Dictionary<ICollimatorConfiguration, ICollimatorCalibrationInfo>();
    }

    public class PhysicsForm(double? doseRate) : Form
    {
        private const double MinCalibrationDurationSec = 0;
        private const double MaxCalibrationDurationSec = 180;

        private FormField<double> _duration = new(60.0);
        private FormField<double> _doseRate = new(doseRate);

        public double? DefaultDoseRate => doseRate;

        [Required(ErrorMessage = "Duration is required")]
        [Double]
        [NumericRange(MinCalibrationDurationSec, MaxCalibrationDurationSec)]
        [DeniedDoubleValues(0d, ErrorMessage = "Duration cannot be 0")]
        [FieldReference(nameof(_duration))]
        public object? Duration
        {
            get => GetFieldValue(); // Would be nice to return just _duration
            set => SetFieldValue(value);
        }

        //[Required(ErrorMessage = "Dose rate is required")]
        [Double]
        [NumericRange(0, 10000)]
        [DeniedDoubleValues(0d, ErrorMessage = "Dose rate cannot be 0")]
        [FieldReference(nameof(_doseRate))]
        public object? DoseRate
        {
            get => GetFieldValue(); // Would be nice to return just _duration
            set => SetFieldValue(value);
        }
    }
}
