using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.CollimatorConfiguration.CSV;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Core.Enums;

using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Application.UI.UserControls;
using Xcc.Core.Common;
using Xcc.Core.Constants;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Physics
{
    public class ConfigurationViewModel : BindableBase
    {
        #region Constructors
        public ConfigurationViewModel(
            IMagnetometerCorrectionsStore magnetometerCorrectionsStore,
            IHeaterCurrentStore heaterCurrentStore,
            ICoilConfigurationStore coilConfigurationStore,
            IOutputFactorConfigurationStore outputFactorConfigurationStore,
            IPresetConfigurationCommands presetConfigurationCommands,
            ICollimatorModel collimatorModel,
            IDialogService dialogService,
            ILogWriter logWriter,
            IPopUpService popUpService
            )
        {
            MagnetometerCorrectionsStore = magnetometerCorrectionsStore;
            HeaterCurrentStore = heaterCurrentStore;
            CoilConfigurationStore = coilConfigurationStore;
            OutputFactorConfigurationStore = outputFactorConfigurationStore;
            PresetConfigurationCommands = presetConfigurationCommands;
            CollimatorModel = collimatorModel;
            DialogService = dialogService;
            LogWriter = logWriter;
            PopUpService = popUpService;

            CollimatorModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ICollimatorModel.CollimatorConfigurations))
                {
                    SelectMatchingConfiguration();
                }
            };

            MagnetometerCorrectionsStore.IsValidChanged += (s, e) => CommandsRaiseCanExecute();
            MagnetometerCorrectionsStore.IsModifiedChanged += (s, e) => CommandsRaiseCanExecute();
            HeaterCurrentStore.IsValidChanged += (s, e) => CommandsRaiseCanExecute();
            HeaterCurrentStore.IsModifiedChanged += (s, e) => CommandsRaiseCanExecute();
            CoilConfigurationStore.IsValidChanged += (s, e) => CommandsRaiseCanExecute();
            CoilConfigurationStore.IsModifiedChanged += (s, e) => CommandsRaiseCanExecute();
            OutputFactorConfigurationStore.IsValidChanged += (s, e) => CommandsRaiseCanExecute();
            OutputFactorConfigurationStore.IsModifiedChanged += (s, e) => CommandsRaiseCanExecute();
        }

        #endregion Constructors


        #region Injected Dependencies
        public IMagnetometerCorrectionsStore MagnetometerCorrectionsStore { get; }
        public IHeaterCurrentStore HeaterCurrentStore { get; }
        public IOutputFactorConfigurationStore OutputFactorConfigurationStore { get; }
        public IPresetConfigurationCommands PresetConfigurationCommands { get; }
        public ICoilConfigurationStore CoilConfigurationStore { get; }
        public ICollimatorModel CollimatorModel { get; }
        IDialogService DialogService { get; }
        public ILogWriter LogWriter { get; }
        public IPopUpService PopUpService { get; }
        #endregion Injected Dependencies



        #region Properties
        private ICollimatorConfiguration? _selectedConfiguration;
        public ICollimatorConfiguration? SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                if (SetProperty(ref _selectedConfiguration, value))
                {
                    OnCollimatorParameterChanged();
                }
            }
        }

        private TargetType _selectedCollimatorType;
        public TargetType SelectedCollimatorType
        {
            get => _selectedCollimatorType;
            set
            {
                if (SetProperty(ref _selectedCollimatorType, value))
                    SelectMatchingConfiguration();
            }
        }

        private Energy _selectedEnergy;
        public Energy SelectedEnergy
        {
            get => _selectedEnergy;
            set
            {
                if (SetProperty(ref _selectedEnergy, value))
                    SelectMatchingConfiguration();
            }
        }

        private string _csvConfigurationFilepath = string.Empty;
        public string CsvConfigurationFilepath
        {
            get => _csvConfigurationFilepath;
            set => SetProperty(ref _csvConfigurationFilepath, value);
        }

        public Task? FetchConfigurationTask { get; set; }


        private string? _calibrationDataWarningMessage;
        public string? CalibrationDataWarningMessage
        {
            get => _calibrationDataWarningMessage;
            set => SetProperty(ref _calibrationDataWarningMessage, value);
        }
        #endregion Properties


        #region Tabs-related properties
        TabItem? _selectedTab;
        public TabItem? SelectedTab
        {
            get => _selectedTab;
            set
            {
                SetProperty(ref _selectedTab, value);
            }
        }

        private DelegateCommand<Tuple<object, object>>? _tabChangePreventedCommand;
        public DelegateCommand<Tuple<object, object>>? TabChangePreventedCommand => _tabChangePreventedCommand ??= new DelegateCommand<Tuple<object, object>>(
            (tabs) =>
            {
                var lockedTab = (XccTabItem)tabs.Item1;
                var desiredTab = (TabItem)tabs.Item2;

                string message = StringConstants.Physics.LeaveConfigurationTabConfirmationMessage;
                if (lockedTab != null && DialogService.Confirmation(lockedTab.Tag.ToString()!, message))
                {
                    lockedTab.SetPreventTabChangeCurrent(false);
                    SelectedTab = desiredTab;

                    lockedTab.SetPreventTabChangeCurrent(true); // restore state of the locked tab after tab changing.
                }
            });
        #endregion Tabs-related properties


        #region Commands
        private DelegateCommand? _csvImportCommand;
        public DelegateCommand? CsvImportCommand => _csvImportCommand ??= new DelegateCommand(ImportCsv);

        private DelegateCommand? _reloadCommand;
        public DelegateCommand? ReloadCommand => _reloadCommand ??= new DelegateCommand(
            () => {
                if (DialogService.Confirmation(
                    StringConstants.Physics.ReloadConfigurationConfirmationTitle,
                    StringConstants.Physics.ReloadConfigurationConfirmationMessage))
                {
                    OnCollimatorParameterChanged();
                }
            }
            ).ObservesCanExecute(() => CanSave);

        private DelegateCommand? _approveCommand;
        public DelegateCommand? ApproveCommand => _approveCommand ??= new DelegateCommand(
            () =>
            {
                var preset = SelectedConfiguration?.DefaultPreset;

                if (preset != null)
                {
                    DialogService.ApprovalDialog(new PresetConfigurationApprovalAction(PresetConfigurationCommands, preset));

                    if (preset.IsApproved)
                    {
                        PopUpService.ShowMessage(
                            StringConstants.Common.SettingsDialogTitle,
                            StringConstants.Common.RestartExternalOnSaveNotification,
                            ReportType.Info);

                        //SelectConfiguration();

                        //StoreHeadConfigurationToCsv(SelectedSource!, actualConfiguration);
                        ApproveCommand?.RaiseCanExecuteChanged();
                    }
                }
            }, CanApprove);

        private DelegateCommand? _saveCommand;
        public DelegateCommand? SaveCommand => _saveCommand ??= new DelegateCommand(SaveConfiguration)
            .ObservesCanExecute(() => CanSave);

        public bool CanSave => IsValid && 
                               IsModified;
        
        public bool IsValid => HeaterCurrentStore.IsValid &&
                               OutputFactorConfigurationStore.IsValid &&
                               MagnetometerCorrectionsStore.IsValid &&
                               CoilConfigurationStore.IsValid;

        public bool IsModified => (HeaterCurrentStore.IsModified || OutputFactorConfigurationStore.IsModified ||
                                   MagnetometerCorrectionsStore.IsModified || CoilConfigurationStore.IsModified);

        #endregion Commands



        private void SelectMatchingConfiguration()
        {
            try
            {
                var configuration = CollimatorModel.FindConfigurationByType(SelectedCollimatorType, SelectedEnergy);
                if (configuration is null)
                {
                    SelectedConfiguration = null;
                }
                else
                {
                    if (configuration.DefaultPreset is null)
                    {
                        IPresetConfiguration preset = new PresetConfiguration
                        {
                            CollimatorConfigurationId = configuration.Id,
                            IsActive = true,
                            IsDefault = true,
                            PresetName = "Default"
                        };

                        configuration.AddPreset(preset);
                    }

                    SelectedConfiguration = configuration;
                }
            }
            catch (Exception)
            {
                DialogService.ReportError(
                    StringConstants.Common.ErrorTitle,
                    "Configuration for this applicator is not supported");
                SelectedConfiguration = null;
            }
            finally
            {
                ApproveCommand?.RaiseCanExecuteChanged();
                SaveCommand?.RaiseCanExecuteChanged();
            }
        }

        private void OnCollimatorParameterChanged()
        {
            CoilConfigurationStore.CollimatorConfiguration = SelectedConfiguration;
            MagnetometerCorrectionsStore.CollimatorConfiguration = SelectedConfiguration;
            HeaterCurrentStore.CollimatorConfiguration = SelectedConfiguration;
            OutputFactorConfigurationStore.CollimatorConfiguration = SelectedConfiguration;

            if (SelectedConfiguration?.DefaultPreset != null)
            {
                FetchConfigurationTask = FetchConfigurationAsync();
            }
        }

        private async Task FetchConfigurationAsync()
        {
            try
            {
                await CoilConfigurationStore.FetchCollimatorConfigurationAsync();
                await MagnetometerCorrectionsStore.FetchMagnetometerParametersAsync();
                await HeaterCurrentStore.FetchHeaterCurrentAsync();
                await OutputFactorConfigurationStore.FetchOutputFactorsAsync();
            }
            catch (Exception ex)
            {
                _= LogWriter.LogAsync(
                    $"Failed to fetch applicator configuration. {ex.Message}. {ex.InnerException?.Message}", 
                    LogRecordSeverity.Error, 
                    LogRecordType.Error);
            }
        }


        #region CSV-import methods
        private void ImportCsv()
        {
            RetryCurrentTaskCommand = new DelegateCommand(() =>
            {
                CurrentTask = new ObservableTask(
                    ImportCsvAsync(),
                    StringConstants.Physics.CsvImportUiErrorMessage);
            });
            CancelCurrentTaskCommand = new DelegateCommand(() => { CurrentTask = null; });
            RetryCurrentTaskCommand.Execute();
        }


        private async Task ImportCsvAsync()
        {
            try
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Calibration file (*.csv)|*.csv";
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FilePathValidation.CheckTraversalSecurity(openFileDialog.FileName, ".csv");
                    CsvConfigurationFilepath = openFileDialog.FileName;
                    await TrySetConfigurationFromCsvAsync();
                }
            }
            catch (FilePathValidationException ex)
            {
                CsvConfigurationFilepath = string.Empty;
                DialogService.ReportError(
                    StringConstants.Common.Validation.FilePathErrorTitle, 
                    ex.Message);
            }
            catch (Exception ex)
            {
                CsvConfigurationFilepath = string.Empty;
                string errorTitle = StringConstants.Physics.ConfigurationFileLoadErrorTitle;
                DialogService.ReportError(errorTitle, ex.Message);

                if (ex is not InvalidDataException)
                {
                    // We have the validation errors written to the log already, so no need in this caption
                    _ = LogWriter.LogAsync(
                        $"{errorTitle}. {ex.Message}. {ex.InnerException}",
                        LogRecordSeverity.Error,
                        LogRecordType.Error);
                }
            }
        }


        private async Task TrySetConfigurationFromCsvAsync()
        {
            CsvConfiguration csvConfiguration = new CsvConfiguration();
            using (var streamReader = new StreamReader(CsvConfigurationFilepath))
            {
                csvConfiguration.ReadCsv(streamReader);
            }

            // Validate configuration consistency
            var errors = CsvConsistencyCheck(csvConfiguration);
            if (errors != null && errors.Count > 0)
            {
                // TODO: ensure that we write this log at least locally
                foreach (var error in errors)
                {
                    _= LogWriter.LogAsync(error, LogRecordSeverity.Error, LogRecordType.Error);
                }
                // log all errors and fail
                throw new InvalidDataException(StringConstants.Physics.CsvFileFormatError);
            }

            // Now we check for an existing applicator
            var collimator = TrySelectCollimatorBySerial(csvConfiguration.Collimator);
            csvConfiguration.Collimator.Id = collimator.Id;

            // Then we check that only one matching active and default preset exists,
            // and if there's none of them, we create one.
            var preset = await TrySelectMatchingPresetAsync(collimator.Configuration, csvConfiguration.Preset);

            // Now we can select the applicator and start copying data into controls
            SelectedCollimatorType = collimator.Configuration.Type;
            SelectedEnergy = collimator.Configuration.Energy;
            
            await FetchConfigurationTask; // ensure that we load the preset first

            FillConfigurationModelsWithCsvConfig(csvConfiguration);
        }

        private IList<string> CsvConsistencyCheck(CsvConfiguration csvConfiguration)
        {
            var errors = new List<string>();
            // 1. check if all tables present:
            var tableList = new List<(string name, object value)> { 
                (CsvConfiguration.COLLIMATOR_TABLE, csvConfiguration.Collimator),
                (CsvConfiguration.PRESET_CONFIGURATION_TABLE, csvConfiguration.Preset),
                (CsvConfiguration.COIL_CONFIGURATION_TABLE, csvConfiguration.CoilConfigurations),
                (CsvConfiguration.CORRECTION_MATRIX_TABLE, csvConfiguration.CorrectionMatrixEntries),
                (CsvConfiguration.REFERENCE_FIELD_TABLE, csvConfiguration.ReferenceFieldEntries),
                (CsvConfiguration.HEATER_CURRENT_TABLE, csvConfiguration.HeaterCurrentConfig),
                (CsvConfiguration.OUTPUT_FACTOR_TABLE, csvConfiguration.OutputFactorEntries),
            };
            foreach (var table in tableList)
            {
                if (table.value == null)
                {
                    errors.Add($"Configuration CSV consistency error: missing {table.name} table");
                }
            }

            // 2. Check if coil configuration and output factors contain
            // complete set of fields according to applicator size:
            if (csvConfiguration.Collimator != null) 
            {
                var collimatorType = csvConfiguration.Collimator.Configuration.Type;
                var requiredFields = TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(collimatorType).Values;
                if (csvConfiguration.CoilConfigurations != null) 
                {
                    var fieldSet = csvConfiguration.CoilConfigurations.Select(c => c.FieldName).ToHashSet();
                    foreach (var requiredField in requiredFields)
                    {
                        if (!fieldSet.Contains(requiredField))
                        {
                            var csvFieldName = new CsvTreatmentFieldName(requiredField);
                            errors.Add($"{CsvConfiguration.COIL_CONFIGURATION_TABLE} CSV table consistency error: {csvFieldName} field is missing");
                        }
                    }
                }

                if (csvConfiguration.OutputFactorEntries != null)
                {
                    var fieldSet = csvConfiguration.OutputFactorEntries.Select(c => c.FieldName).ToHashSet();
                    foreach (var requiredField in requiredFields)
                    {
                        if (!fieldSet.Contains(requiredField))
                        {
                            var csvFieldName = new CsvTreatmentFieldName(requiredField);
                            errors.Add($"{CsvConfiguration.COIL_CONFIGURATION_TABLE} CSV table consistency error: {csvFieldName} field is missing");
                        }
                    }
                }
            }

            // 3. Check if correction matrices and reference fields are specified for both back and front magnetometers:
            var magnetometersToCheck = new List<MagnetometerType> { MagnetometerType.Back, MagnetometerType.Front};
            if (csvConfiguration.CorrectionMatrixEntries != null)
            {
                var typeSet = csvConfiguration.CorrectionMatrixEntries.Select(cm => cm.MagnetometerType).ToHashSet();
                foreach(var magnetometer in magnetometersToCheck)
                {
                    if (!typeSet.Contains(magnetometer))
                    {
                        var csvMagnetometer = new CsvMagnetometerType(magnetometer);
                        errors.Add($"{CsvConfiguration.CORRECTION_MATRIX_TABLE} CSV table consistency error: {csvMagnetometer} magnetometer record is missing");
                    }
                }
            }
            
            if (csvConfiguration.ReferenceFieldEntries != null)
            {
                var typeSet = csvConfiguration.ReferenceFieldEntries.Select(cm => cm.MagnetometerType).ToHashSet();
                foreach (var magnetometer in magnetometersToCheck)
                {
                    if (!typeSet.Contains(magnetometer))
                    {
                        var csvMagnetometer = new CsvMagnetometerType(magnetometer);
                        errors.Add($"{CsvConfiguration.REFERENCE_FIELD_TABLE} CSV table consistency error: {csvMagnetometer} magnetometer record is missing");
                    }
                }
            }

            // 4. Check if heater current is specified
            if (csvConfiguration.HeaterCurrentConfig != null)
            {
                if (csvConfiguration.HeaterCurrentConfig.HeaterCurrent is null)
                {
                    errors.Add($"{CsvConfiguration.HEATER_CURRENT_TABLE} CSV table consistency error: the record is missing");
                }
            }

            // 5. Value null/range check:
            if (csvConfiguration.CoilConfigurations != null)
            {
                foreach (var field in csvConfiguration.CoilConfigurations)
                {
                    CheckFieldValueForError(ref errors, CsvConfiguration.COIL_CONFIGURATION_TABLE, field.FieldName,
                        field.FocusCurrent, PhysicsValueRange.FocusCurrentMin, PhysicsValueRange.FocusCurrentMax, nameof(CsvCoilConfiguration.FocusCurrent));
                    CheckFieldValueForError(ref errors, CsvConfiguration.COIL_CONFIGURATION_TABLE, field.FieldName,
                        field.XDeflectionCurrent, PhysicsValueRange.XDeflectionCurrentMin, PhysicsValueRange.XDeflectionCurrentMax, nameof(CsvCoilConfiguration.XDeflectionCurrent));
                    CheckFieldValueForError(ref errors, CsvConfiguration.COIL_CONFIGURATION_TABLE, field.FieldName,
                        field.YDeflectionCurrent, PhysicsValueRange.YDeflectionCurrentMin, PhysicsValueRange.YDeflectionCurrentMax, nameof(CsvCoilConfiguration.YDeflectionCurrent));
                }
            }

            if (csvConfiguration.CorrectionMatrixEntries != null)
            {
                foreach (var entry in csvConfiguration.CorrectionMatrixEntries)
                {
                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.CORRECTION_MATRIX_TABLE, entry.MagnetometerType,
                        entry.Cm11, PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax, nameof(CsvCorrectionMatrix.CM11));

                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.CORRECTION_MATRIX_TABLE, entry.MagnetometerType,
                        entry.Cm12, PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax, nameof(CsvCorrectionMatrix.CM12));

                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.CORRECTION_MATRIX_TABLE, entry.MagnetometerType,
                        entry.Cm13, PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax, nameof(CsvCorrectionMatrix.CM13));

                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.CORRECTION_MATRIX_TABLE, entry.MagnetometerType,
                        entry.Cm21, PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax, nameof(CsvCorrectionMatrix.CM21));

                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.CORRECTION_MATRIX_TABLE, entry.MagnetometerType,
                        entry.Cm23, PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax, nameof(CsvCorrectionMatrix.CM22));

                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.CORRECTION_MATRIX_TABLE, entry.MagnetometerType,
                        entry.Cm23, PhysicsValueRange.CorrectionMatrixMin, PhysicsValueRange.CorrectionMatrixMax, nameof(CsvCorrectionMatrix.CM23));
                }
            }

            if (csvConfiguration.ReferenceFieldEntries != null)
            {
                foreach (var entry in csvConfiguration.ReferenceFieldEntries)
                {
                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.REFERENCE_FIELD_TABLE, entry.MagnetometerType,
                        entry.Rf11, PhysicsValueRange.ReferenceFieldsMin, PhysicsValueRange.ReferenceFieldsMax, nameof(CsvReferenceField.RF11));
                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.REFERENCE_FIELD_TABLE, entry.MagnetometerType,
                        entry.Rf21, PhysicsValueRange.ReferenceFieldsMin, PhysicsValueRange.ReferenceFieldsMax, nameof(CsvReferenceField.RF21));
                    CheckMagnetometerValueForError(ref errors, CsvConfiguration.REFERENCE_FIELD_TABLE, entry.MagnetometerType,
                        entry.Rf31, PhysicsValueRange.ReferenceFieldsMin, PhysicsValueRange.ReferenceFieldsMax, nameof(CsvReferenceField.RF31));
                }
            }

            if (csvConfiguration.HeaterCurrentConfig?.HeaterCurrent != null)
            {
                double value = csvConfiguration.HeaterCurrentConfig.HeaterCurrent.Value;
                double minValue = PhysicsValueRange.HeaterCurrentMin;
                double maxValue = PhysicsValueRange.HeaterCurrentMax;
                if (value < minValue || value > maxValue)
                {
                    errors.Add($"{CsvConfiguration.HEATER_CURRENT_TABLE} CSV table validation error:"+
                        $" {nameof(CsvHeaterCurrent.HeaterCurrent)} value is out of range [{minValue}...{maxValue}]");
                }
            }

            if (csvConfiguration.OutputFactorEntries != null)
            {
                foreach (var field in csvConfiguration.OutputFactorEntries)
                {
                    CheckFieldValueForError(ref errors, CsvConfiguration.OUTPUT_FACTOR_TABLE, field.FieldName,
                        field.Factor, PhysicsValueRange.OutputFactorMin, PhysicsValueRange.OutputFactorMax, nameof(CsvOutputFactor.Factor));
                }
            }


            return errors;
        }

        private static void CheckFieldValueForError(
            ref List<string> errors, 
            string tableName,
            TreatmentFieldName fieldName, 
            double? fieldValue, 
            double minValue, 
            double maxValue, 
            string propertyName)
        {
            var csvTypeName = new CsvTreatmentFieldName(fieldName);

            if (fieldValue is null)
            {
                errors.Add($"{tableName} CSV table - {csvTypeName} validation error: {propertyName} has invalid value");
            }
            else if (fieldValue < minValue || fieldValue > maxValue)
            {
                errors.Add($"{tableName} CSV table - {csvTypeName} validation error: {propertyName} value is out of range [{minValue}...{maxValue}]");
            }
        }

        private static void CheckMagnetometerValueForError(
            ref List<string> errors,
            string tableName,
            MagnetometerType magnetometerType,
            double? fieldValue,
            double minValue,
            double maxValue,
            string propertyName)
        {
            var csvFieldName = new CsvMagnetometerType(magnetometerType);

            if (fieldValue is null)
            {
                errors.Add($"{tableName} CSV table - {csvFieldName} field validation error: {propertyName} has invalid value");
            }
            else if (fieldValue < minValue || fieldValue > maxValue)
            {
                errors.Add($"{tableName} CSV table - {csvFieldName} field validation error: {propertyName} value is out of range [{minValue}...{maxValue}]");
            }
        }


        private ICollimator TrySelectCollimatorBySerial(ICollimator collimator)
        {
            if (collimator == null)
            {
                throw new Exception("Applicator description is missing");
            }

            var matchingCollimator = CollimatorModel.Collimators.FirstOrDefault(c => c.Serial.Equals(collimator.Serial));
            if (matchingCollimator == null)
            {
                throw new Exception($"Cannot find the applicator #{collimator.Serial}");
            }
            else if (matchingCollimator.IsActive == false)
            {
                throw new Exception($"Applicator #{collimator.Serial} is not active");
            }
            else if (!matchingCollimator.Configuration.IsSame(collimator.Configuration))
            {
                throw new Exception($"Applicator #{collimator.Serial} does not match by size or energy");
            }

            return matchingCollimator;
        }

        private Task<IPresetConfiguration?> TrySelectMatchingPresetAsync(
            ICollimatorConfiguration collimatorConfiguration, 
            IPresetConfiguration presetToFind)
        {
            if (presetToFind == null || !presetToFind.IsActive || !presetToFind.IsDefault)
            {
                throw new ArgumentException("Preset is missing or is not set as active and default");
            }

            var defaultPresets = collimatorConfiguration.Presets.Where(p => p.IsActive && p.IsDefault).ToList();

            var preset = defaultPresets.FirstOrDefault();
            if (defaultPresets.Count > 1)
            {
                throw new Exception("Preset ambiguity error: there are several default active presets");
            }
            else if (defaultPresets.Count == 0)
            {
                //// No default preset, add one now:
                //preset = await CollimatorModel.AddPresetAsync(collimatorConfiguration, presetToFind.PresetName, isActive:true, isDefault:true);
                throw new InvalidDataException("Preset is missing");
            }

            return Task.FromResult(preset);
        }

        private void FillConfigurationModelsWithCsvConfig(CsvConfiguration csvConfiguration)
        {
            if (csvConfiguration.CoilConfigurations is not null)
            {
                var coilConfigurationsToFill = CoilConfigurationStore.Configuration.GetConfiguration().ToDictionary(x => x.FieldName, x => x);
                foreach (var coilConfiguration in csvConfiguration.CoilConfigurations)
                {
                    if (coilConfigurationsToFill.TryGetValue(coilConfiguration.FieldName, out var config))
                    {
                        config.SetupFormValue(coilConfiguration);
                    }
                }
            }

            if (csvConfiguration.CorrectionMatrixEntries is not null)
            {
                foreach (var correctionMatrix in csvConfiguration.CorrectionMatrixEntries)
                {
                    switch (correctionMatrix.MagnetometerType)
                    {
                        case MagnetometerType.Back:
                            correctionMatrix.CopyProperties(MagnetometerCorrectionsStore.Corrections.BackMatrix);
                            break;
                        case MagnetometerType.Front:
                            correctionMatrix.CopyProperties(MagnetometerCorrectionsStore.Corrections.FrontMatrix);
                            break;
                        default: // Unsupported magnetometer type
                            break;
                            //throw new Exception($"Invalid magnetometer type {correctionMatrix.MagnetometerType}");
                    }
                }
            }

            if (csvConfiguration.ReferenceFieldEntries is not null)
            {
                foreach (var csvReferenceField in csvConfiguration.ReferenceFieldEntries)
                {
                    switch (csvReferenceField.MagnetometerType)
                    {
                        case MagnetometerType.Back:
                            csvReferenceField.CopyProperties(MagnetometerCorrectionsStore.Corrections.BackReferenceField);
                            break;
                        case MagnetometerType.Front:
                            csvReferenceField.CopyProperties(MagnetometerCorrectionsStore.Corrections.FrontReferenceField);
                            break;
                        default: // Unsupported magnetometer type
                            break;
                            //throw new Exception($"Invalid magnetometer type {correctionMatrix.MagnetometerType}");
                    }
                }
            }

            csvConfiguration.HeaterCurrentConfig?.CopyProperties(HeaterCurrentStore.HeaterCurrent);

            if (csvConfiguration.OutputFactorEntries is not null)
            {
                var outputFactorsToFill = OutputFactorConfigurationStore.Configuration.OutputFactors.ToDictionary(x => x.FieldName, x => x);
                foreach (var outputFactor in csvConfiguration.OutputFactorEntries)
                {
                    if (outputFactorsToFill.ContainsKey(outputFactor.FieldName))
                    {
                        outputFactor.CopyProperties(outputFactorsToFill[outputFactor.FieldName]);
                    }
                }
            }
        }
        #endregion CSV-import methods



        #region Observable task
        private ObservableTask? _currentTask;
        public ObservableTask? CurrentTask
        {
            get => _currentTask;
            set => SetProperty(ref _currentTask, value);
        }

        private DelegateCommand? _retryCurrentTaskCommand;
        public DelegateCommand? RetryCurrentTaskCommand
        {
            get => _retryCurrentTaskCommand;
            set => SetProperty(ref _retryCurrentTaskCommand, value);
        }

        private DelegateCommand? _cancelCurrentTaskCommand;
        public DelegateCommand? CancelCurrentTaskCommand
        {
            get => _cancelCurrentTaskCommand;
            set => SetProperty(ref _cancelCurrentTaskCommand, value);
        }

        #endregion Observable task



        #region private methods

        private void SaveConfiguration()
        {
            RetryCurrentTaskCommand = new DelegateCommand(() =>
            {
                CurrentTask = new ObservableTask(
                    SaveConfigurationAsync(),
                    StringConstants.Physics.ConfigurationSaveErrorMessage);
            });

            CancelCurrentTaskCommand = new DelegateCommand(() => { CurrentTask = null; });
            RetryCurrentTaskCommand.Execute();
        }


        private async Task SaveConfigurationAsync()
        {
            try
            {
                await System.Windows.Application.Current.Dispatcher.Invoke(async () =>
                {
                    var defaultPreset = SelectedConfiguration.DefaultPreset;
                    if (BaseEntry.IsBlankId(defaultPreset.Id))
                    {
                        if (SelectedConfiguration.Presets.Select(p => p is {IsActive: true, IsDefault: true}).Count() == 1)
                        {
                            SelectedConfiguration.Presets.Remove(defaultPreset);
                        }

                        var createdPreset = await PresetConfigurationCommands.CreateAsync(defaultPreset);
                        SelectedConfiguration.AddPreset(createdPreset);
                    }

                    await HeaterCurrentStore.SubmitHeaterCurrentAsync();
                    await OutputFactorConfigurationStore.SubmitOutputFactorsAsync();
                    await CoilConfigurationStore.SubmitCollimatorConfigurationAsync();
                    await MagnetometerCorrectionsStore.SubmitMagnetometerParametersAsync();

                    var preset = SelectedConfiguration?.DefaultPreset;
                    if (preset != null)
                    {
                        // Invalidate preset approval, TODO: change this to the proper domain/persistence logic
                        preset.ApprovedBy = string.Empty;
                        ApproveCommand?.RaiseCanExecuteChanged();
                    }
                });
                
                PopUpService.ShowMessage(
                    StringConstants.Common.SettingsDialogTitle,
                    StringConstants.Physics.SavedDataRequiredApprovalMessage,
                    Xcc.Core.Enums.ReportType.Info);
            }
            catch (Exception ex)
            {
                _= LogWriter.LogAsync(
                    $"{StringConstants.Physics.HeaterCurrentSaveErrorMessage} {ex.Message}. {ex.InnerException?.Message}",
                    LogRecordSeverity.Error, LogRecordType.Error);
                throw;
            }
            finally
            {
                SaveCommand?.RaiseCanExecuteChanged();
                ApproveCommand?.RaiseCanExecuteChanged();
            }
        }
        private bool CanApprove()
        {
            var preset = SelectedConfiguration?.DefaultPreset;
            bool validForApprove = HeaterCurrentStore.IsValid &&
                OutputFactorConfigurationStore.IsValid &&
                MagnetometerCorrectionsStore.IsValid &&
                CoilConfigurationStore.IsValid &&
                !HeaterCurrentStore.IsModified && 
                !OutputFactorConfigurationStore.IsModified &&
                !MagnetometerCorrectionsStore.IsModified &&
                !CoilConfigurationStore.IsModified;
            return (preset is not null && !preset.IsApproved && validForApprove);
        }

        private void CommandsRaiseCanExecute()
        {
            RaisePropertyChanged(nameof(CanSave));

            if (IsValid == false)
            {
                CalibrationDataWarningMessage = StringConstants.Physics.PhysicsDataIsInvalid;
            }
            else if (IsModified)
            {
                CalibrationDataWarningMessage = StringConstants.Physics.PhysicsDataIsModified;
            }
            else
            {
                CalibrationDataWarningMessage = null;
            }

            ApproveCommand?.RaiseCanExecuteChanged();
        }

        #endregion private methods
    }
}
