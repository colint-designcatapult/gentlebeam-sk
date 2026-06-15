using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Application.AppLayer.Collimators;
using Heracles.Application.Common;
using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Helpers;
using Heracles.Application.Models;
using Heracles.Application.Models.EMR;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Supervision.DisruptiveActions;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Indoor.ViewModels.Dialogs;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Application.Common;
using Xcc.Application.Domain.System;
using Xcc.Application.Helpers;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels
{

    public class PrescriptionViewModel : BindableBase
    {
        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        #region Contructors

        public PrescriptionViewModel(
            IRegionManager regionManager,
            IPrescriptionRepository prescriptionRepository,
            IPlanModel planModel,
            ILogWriter logWriter,
            IDialogService dialogService,
            IDisruptiveActionWatchdogFactory disruptiveActionWatchdogFactory,
            ITreatmentInfoStore treatmentInfoStore,
            ITreatmentDoseCalculation treatmentDoseCalculation,
            ICollimatorModel collimatorModel,
            IDispatcherService dispatcherService,
            IEventAggregator eventAggregator,
            IPopUpService popUpService)
        {
            RegionManager = regionManager;
            PlanModel = planModel;
            LogWriter = logWriter;
            DialogService = dialogService;
            PrescriptionRepository = prescriptionRepository;

            TreatmentInfoStore = treatmentInfoStore;
            TreatmentDoseCalculation = treatmentDoseCalculation;
            CollimatorModel = collimatorModel;
            DispatcherService = dispatcherService;
            EventAggregator = eventAggregator;
            PopUpService = popUpService;
            TreatmentInfoStore.SimulationChanged += OnSimulationChanged;
            //TreatmentInfoStore.PrescriptionChanged += OnPrescriptionChanged;

            //Set watchdog on the PrescriptionForm.IsModified flag
            QuitTreatmentActionWatchdog = disruptiveActionWatchdogFactory.MakeWatchdog<QuitTreatmentAction, PrescriptionForm>(
                args: new DisruptiveActionLockArgs()
                {
                    LockType = DisruptiveActionLockType.Warn,
                    Message = "Save Prescription changes"
                },
                predicate: (PrescriptionForm? form) => form?.IsModified ?? false,
                observableObject: null
            );
        }
        #endregion


        #region Read-only properties

        public IRegionManager RegionManager { get; }
        public IPrescriptionRepository PrescriptionRepository { get; }
        public IPlanModel PlanModel { get; }
        public ILogWriter LogWriter { get; }
        public IDialogService DialogService { get; }
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public ITreatmentDoseCalculation TreatmentDoseCalculation { get; }
        public ICollimatorModel CollimatorModel { get; }
        public IDispatcherService DispatcherService { get; }
        public IEventAggregator EventAggregator { get; }
        public IPopUpService PopUpService { get; }
        private IDisruptiveActionWatchdog<PrescriptionForm> QuitTreatmentActionWatchdog { get; }

        /// <summary>
        /// Temporary implementation of Tdf values ItemSource
        /// </summary>
        public IEnumerable<TDF> AvailableTdfValues { get; } = Enum.GetValues<TDF>();

        #endregion Read-only properties


        #region Properties

        private PrescriptionForm? _prescriptionForm;
        public PrescriptionForm? PrescriptionForm
        {
            get => _prescriptionForm;
            set
            {
                if (_prescriptionForm != value && _prescriptionForm != null)
                {
                    _prescriptionForm.IsValidChanged -= PrescriptionOnIsValidChanged;
                    _prescriptionForm.IsModifiedChanged -= PrescriptionOnIsModifiedChanged;
                    _prescriptionForm.PropertyChanged -= PrescriptionOnPropertyChanged;
                }

                if (SetProperty(ref _prescriptionForm, value))
                {
                    if (value != null)
                    {
                        value.IsValidChanged += PrescriptionOnIsValidChanged;
                        value.IsModifiedChanged += PrescriptionOnIsModifiedChanged;
                        value.PropertyChanged += PrescriptionOnPropertyChanged;
                    }

                    RaiseCanExecuteCommands();
                    QuitTreatmentActionWatchdog.SetObject(value);
                    EventAggregator.GetEvent<PrescriptionFormChanged>().Publish(value);
                }
            }
        }

        private ObservableTask? _currentSimulationTask;
        public ObservableTask? CurrentPrescriptionTask
        {
            get => _currentSimulationTask;
            set => SetProperty(ref _currentSimulationTask, value);
        }

        
        #endregion Properties


        #region Commands

        private DelegateCommand? _addCommand;
        public DelegateCommand AddCommand => _addCommand ??= new DelegateCommand(
            OnAddClicked,
            canExecuteMethod: () => TreatmentInfoStore.Simulation != null &&
                                    (PrescriptionForm == null || !PrescriptionForm.IsBlank));

        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            executeMethod: () => SavePrescription(PrescriptionForm?.GetValue()),
            canExecuteMethod: () => PrescriptionForm is {IsModified: true, IsValid: true, Duration: > 0.0} ||
                                    PlanModel.IsModified && 
                                    PrescriptionForm?.Status != Status.APPROVED); 


        private DelegateCommand? _resetCommand;
        public DelegateCommand ResetCommand => _resetCommand ??= new DelegateCommand(
            executeMethod: FetchPrescription);


        private DelegateCommand? _retryPrescriptionTaskCommand;
        public DelegateCommand RetryPrescriptionCommand
        {
            get => _retryPrescriptionTaskCommand;
            set => SetProperty(ref _retryPrescriptionTaskCommand, value);
        }

        private DelegateCommand? _cancelPrescriptionTaskCommand;
        public DelegateCommand CancelPrescriptionTaskCommand => _cancelPrescriptionTaskCommand ??= new DelegateCommand(
            () =>
            {
                CurrentPrescriptionTask = null;
                PrescriptionForm = null;
            }
            );

        #endregion Commands


        #region Private methods

        private void OnAddClicked()
        {
            try
            {
                if (TreatmentInfoStore.Simulation == null)
                    throw new InvalidOperationException(StringConstants.EMR.CannotCreatePrescriptionNoSimulationError);

                if (PlanModel.IsModified && ConfirmPrescriptionChanges() == false)
                {
                    // User refused to lose changes in the plan
                    return;
                }

                TreatmentInfoStore.Prescription = null;

                Pathology? pathology = TreatmentInfoStore.Diagnosis?.Pathology;
                if (pathology == null)
                {
                    // default Prescription data
                    PrescriptionForm = new PrescriptionForm(
                        PopUpService,
                        new Prescription()
                        {
                            SimulationId = TreatmentInfoStore.Simulation.Id,
                            MinTdf = TDF.Tdf_94,
                            NumberOfFxs = 20,
                            FxsPerWeek = 4
                        },
                        GetDurationCalculator()
                        );
                }
                else
                {
                    PrescriptionForm =
                        PrescriptionForm.GetDefaultPrescriptionState(
                            PopUpService,
                            TreatmentInfoStore.Simulation,
                            pathology.Value,
                            GetDurationCalculator());
                }
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(StringConstants.EMR.PrescriptionError, ex.Message);
            }
        }

        private DurationCalculator GetDurationCalculator()
        {
            return new(
                CollimatorModel.CollimatorConfigurations
                    .Where(x => x.Type == TreatmentInfoStore.Simulation.TargetType)
                    .Select(x => (x.Energy, new OutputFactorInfo(outputFactor: 1.0, x.ReferencedDoseRate)))
                    .ToDictionary()
                );
        }

        private void PrescriptionOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            EventAggregator.GetEvent<PrescriptionFormChanged>().Publish(PrescriptionForm);
        }

        private void PrescriptionOnIsModifiedChanged(object? sender, bool e)
        {
            RaiseCanExecuteCommands();
        }

        private void PrescriptionOnIsValidChanged(object? sender, bool e)
        {
            RaiseCanExecuteCommands();
        }

        private void FetchPrescription()
        {
            RetryPrescriptionCommand = new DelegateCommand(() =>
            {
                CurrentPrescriptionTask = new ObservableTask(FetchPrescriptionAsync(), StringConstants.EMR.FetchPrescriptionMessage);
            });
            RetryPrescriptionCommand.Execute();
        }

        private async Task FetchPrescriptionAsync()
        {
            try
            {
                await Semaphore.WaitAsync();

                if (TreatmentInfoStore.Simulation is null)
                {
                    TreatmentInfoStore.Prescription = null;
                }
                else
                {
                    TreatmentInfoStore.Prescription = await PrescriptionRepository.FetchLatestPrescriptionAsync(TreatmentInfoStore.Simulation.Id);
                }

                await OnPrescriptionChanged(TreatmentInfoStore.Prescription);
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync(
                    $"{StringConstants.EMR.FetchPrescriptionMessage}. {ex.Message}",
                    LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
            finally
            {
                Semaphore.Release();
                RaiseCanExecuteCommands();
            }
        }

        private void SavePrescription(Prescription? prescriptionToSave)
        {
            if (!BaseEntry.IsNullOrBlankEntry(prescriptionToSave) &&
                TreatmentInfoStore.Prescription.NumberOfFxs != prescriptionToSave?.NumberOfFxs)
            {
                DialogService.ShowDialog("AcknowledgePrescriptionView", null,
                    result =>
                    {
                        if (result.Result == ButtonResult.OK)
                        {
                            var reason = result.Parameters.GetValue<string>(AcknowledgePrescriptionViewModel.SelectedOptionsParameterKey);

                            _ = LogWriter.LogAsync(reason, LogRecordSeverity.Info, LogRecordType.User);
                        }
                    });
            }

            RetryPrescriptionCommand = new DelegateCommand(() =>
            {
                CurrentPrescriptionTask = new ObservableTask(SavePrescriptionAsync(prescriptionToSave), StringConstants.EMR.SavePrescriptionMessage);
            });
            RetryPrescriptionCommand.Execute();
        }

        private async Task SavePrescriptionAsync(Prescription? prescriptionToSave)
        {
            try
            {
                if (prescriptionToSave is null) 
                    throw new NullReferenceException("Prescription data is missing");

                var newPrescription = 
                    new Prescription(await PrescriptionRepository.SubmitAsync(prescriptionToSave, initialState: TreatmentInfoStore.Prescription));

                if (PlanModel.Plan == null || prescriptionToSave.IsBlank)
                {
                    await PlanModel.OnUpdatePrescriptionAsync(newPrescription);
                }
                else if (TreatmentInfoStore.Simulation is not null && newPrescription?.Energy is not null)
                {
                    // TODO: workaround for not updating collimator type on prescription's energy change
                    PlanModel.SetCollimatorConfiguration(TreatmentInfoStore.Simulation.TargetType, newPrescription.Energy);
                }

                PlanModel.AddOrUpdateTreatmentField(newPrescription);

                await PlanModel.SubmitAsync(); // save the plan and TreatmentField, when Prescription is saved 

                TreatmentInfoStore.Prescription = newPrescription;

                PrescriptionForm = new PrescriptionForm(PopUpService, newPrescription, PrescriptionForm!.DurationCalculator);
                //}
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync(
                    $"{StringConstants.EMR.SavePrescriptionMessage}. {ex.Message}",
                    LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
            finally
            {
                RaiseCanExecuteCommands();
            }
        }
        
        private bool ConfirmPrescriptionChanges()
        {
            // Warn about losing the current plan
            return DialogService.Confirmation(
                StringConstants.Common.ConfirmationDialogTitle,
                StringConstants.EMR.PrescriptionChangeConfirmation);
        }

        private void RaiseCanExecuteCommands()
        {
            DispatcherService.Invoke(() =>
            {
                AddCommand.RaiseCanExecuteChanged();
                SaveCommand.RaiseCanExecuteChanged();
                ResetCommand.RaiseCanExecuteChanged();
            });
        }

        private void OnSimulationChanged(object? sender, Core.Models.EMR.ISimulation s)
        {
            FetchPrescription();
        }

        private async Task OnPrescriptionChanged(Core.Models.EMR.IPrescription? prescription)
        {
            var targetType = TreatmentInfoStore.Simulation?.TargetType ?? TargetType.TargetType_None;
            var previousTargetType = targetType;
            try
            {
                var plan = await PlanModel.OnUpdatePrescriptionAsync(prescription);
                previousTargetType = plan?.CollimatorType ?? TargetType.TargetType_None;
                // Some changes in prescription, need to reload treatment factors and check the plan,
                // as it may be so that the energy was changed:
                if (plan != null && prescription?.Energy != null)
                {
                    // TODO: we don't use output factors for 1-point applicators, as they're always equal to 1
                    //await TreatmentDoseCalculation.FetchTreatmentFactorsAsync(
                    //    PlanModel.CollimatorConfiguration,
                    //    targetType,
                    //    prescription.Energy.Value);

                    // TODO: But we need to validate against the dose rate, so we keep it here:
                    OnOutputFactorsChanged(); // validate

                    if (PlanModel.TreatmentFields.Count == 0)
                        PlanModel.AddOrUpdateTreatmentField(prescription);
                }
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(StringConstants.EMR.PlanLoadErrorDialogTitle, ex.Message);
            }

            if (prescription is null)
            {
                PrescriptionForm = null;
            }
            else
            {
                var dwellTime = prescription.DwellTime;

                bool recalculateDwellTime = 
                    prescription is { Energy: not 0, DailyDose: not 0 } &&
                    targetType != previousTargetType;
                if (recalculateDwellTime)
                {
                    ICollimatorConfiguration applicatorConfiguration = 
                        CollimatorModel.FindConfigurationByType(targetType, prescription.Energy)
                        ?? throw new NullReferenceException(StringConstants.EMR.CannotFindApplicatorConfigErrorMessage);

                    dwellTime = TreatmentDoseCalculation.CalculateDuration(
                        Application.Models.PlanModel.DefaultTreatmentFieldName,
                        applicatorConfiguration,
                        prescription.DailyDose);
                }

                PrescriptionForm = new PrescriptionForm(
                    PopUpService, 
                    new Prescription(prescription), 
                    GetDurationCalculator())
                {
                    Duration = dwellTime
                };

                if (recalculateDwellTime)
                {
                    SavePrescription(PrescriptionForm.GetValue());
                }
            }
        }

        private void OnOutputFactorsChanged()
        {
            try
            {
                PlanModel.ValidatePlanDosesAndEmissionCurrent();
                if (!PlanModel.IsValid && PlanModel.TreatmentFields.Count > 0)
                    RecalculateTreatmentFieldData();
            }
            catch (Exception ex)
            {
                PopUpService.LogAndShowError(StringConstants.EMR.PrescriptionError, StringConstants.EMR.PrescriptionValidationError, ex);
            }
            finally
            {
                RaiseCanExecuteCommands();
            }
        }

        private void RecalculateTreatmentFieldData()
        {
            DialogBoxResult recalculate = DialogBoxResult.None;

            if (TreatmentInfoStore.Plan?.Status == PlanStatus.APPROVED)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    recalculate = PopUpService.YesNoDialog(
                        StringConstants.EMR.UnapprovePrescriptionTitle,
                        StringConstants.EMR.PrescriptionValidationErrorUnapproveConfirmation);
                });

                if (recalculate == DialogBoxResult.Yes)
                    PlanModel.ShowVerifyDialog();
            }
            else
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    recalculate = PopUpService.YesNoDialog(
                        StringConstants.EMR.AdjustPrescriptionTitle,
                        StringConstants.EMR.PrescriptionValidationErrorConfirmation);
                });

                if (recalculate == DialogBoxResult.Yes)
                {
                    // update the field
                    var treatmentField = PlanModel.TreatmentFields.FirstOrDefault();
                    if (PlanModel.Prescription?.DailyDose == null)
                    {
                        PopUpService.LogAndShowError(
                            StringConstants.EMR.PlanValidationErrorTitle,
                            StringConstants.EMR.DailyDoseNotSetErrorMessage);
                        return;
                    }

                    if (treatmentField == null)
                    {
                        PopUpService.LogAndShowError(
                            StringConstants.EMR.PlanValidationErrorTitle,
                            StringConstants.EMR.PlanWithoutFieldsErrorMessage);
                        return;
                    }

                    treatmentField.DwellTime = TreatmentDoseCalculation.CalculateDuration(treatmentField.Name, PlanModel.CollimatorConfiguration, PlanModel.Prescription.DailyDose);
                    treatmentField.CalculatedDose = TreatmentDoseCalculation.CalculateDose(treatmentField.Name, PlanModel.CollimatorConfiguration, treatmentField.DwellTime);

                    if (PlanModel.Prescription?.Energy != null)
                        treatmentField.Current = CurrentCalculator.CalculateCurrent(PlanModel.Prescription.Energy);

                    PlanModel.AddOrUpdateField(treatmentField);

                    TreatmentInfoStore.Prescription.DwellTime = treatmentField.DwellTime;

                    PlanModel.ValidatePlanDosesAndEmissionCurrent();
                }
            }
        }

        #endregion Private methods
    }

    public class PrescriptionFormChanged : PubSubEvent<PrescriptionForm?>
    {
    }
}
