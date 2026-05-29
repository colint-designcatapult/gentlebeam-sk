using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.Common;
using Heracles.Application.Events;
using Heracles.Application.Models;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Supervision;
using Heracles.Application.Models.Supervision.DisruptiveActions;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Enums;
using Heracles.Core.Models;
using Heracles.Core.Models.EMR;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Indoor.ViewModels
{
    public class CheckableEntry<TValue> : BindableBase
    {
        public TValue Value { get; set; }
        public string DisplayName { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                SetProperty(ref _isChecked, value);
                IsCheckedChanged?.Invoke(this, Value);
            }
        }

        public event EventHandler<TValue> IsCheckedChanged;
    }


    public class SimulationViewModel : BindableBase
    {
        #region Contructors
        public SimulationViewModel(
            IRegionManager regionManager,
            ILogWriter logWriter,
            IDialogService dialogService,
            ISimulationRepository simulationRepository,
            ITreatmentInfoStore treatmentInfoStore,
            IEventAggregator eventAggregator,
            IDisruptiveActionWatchdogFactory disruptiveActionWatchdogFactory,
            IAcquisitionResultStore acquisitionResultStore,
            IAuthorizedUserStore authorizedUserStore,
            IPlanModel planModel)
        {
            RegionManager = regionManager;
            LogWriter = logWriter;
            DialogService = dialogService;
            SimulationRepository = simulationRepository;
            TreatmentInfoStore = treatmentInfoStore;
            EventAggregator = eventAggregator;
            AcquisitionResultStore = acquisitionResultStore;
            AuthorizedUserStore = authorizedUserStore;
            PlanModel = planModel;
            eventAggregator.GetEvent<AcquisitionCompletedEvent>().Subscribe(SetSimulationFormLesionDepth);
            eventAggregator.GetEvent<PlanStatusChangedEvent>().Subscribe((_) => FetchSimulation());

            // Set watchdog on the SimulationForm.IsModified flag
            QuitTreatmentActionWatchdog = disruptiveActionWatchdogFactory.MakeWatchdog<QuitTreatmentAction, ISimulationState>(
                args: new DisruptiveActionLockArgs()
                {
                    LockType = DisruptiveActionLockType.Warn,
                    Message = "Save Simulation changes",
                    //InvokeAction = () => simulationModel.DiscardChanges()
                },
                predicate: (ISimulationState state) => state?.IsModified ?? false,
                observableObject: SimulationForm);

            TreatmentInfoStore.DiagnosisChanged += OnDiagnosisChanged;
            TreatmentInfoStore.SimulationChanged += OnSimulationChanged;
            TreatmentInfoStore.TreatmentDevicesChanged += OnTreatmentDevicesChanged;
            TreatmentInfoStore.PatientPositionsChanged += OnPatientPositionsChanged;
        }
        
        public SimulationViewModel() { }
        #endregion


        #region Read-only properties
        public IRegionManager RegionManager { get; }
        public ILogWriter LogWriter { get; }
        public IDialogService DialogService { get; }
        public ISimulationRepository SimulationRepository { get; }
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public IEventAggregator EventAggregator { get; }
        public IAcquisitionResultStore AcquisitionResultStore { get; }
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public IPlanModel PlanModel { get; }
        private IDisruptiveActionWatchdog<ISimulationState> QuitTreatmentActionWatchdog { get; }
        #endregion Read-only properties


        #region Properties
        
        private ISimulationState _simulationForm;
        public ISimulationState SimulationForm
        {
            get => _simulationForm;
            set
            {
                if (_simulationForm != null &&
                    _simulationForm.Equals(value) == false)
                {
                    _simulationForm.IsModifiedChanged -= SimulationFormOnIsModifiedChanged;
                    _simulationForm.IsValidChanged -= SimulationFormOnIsValidChanged;
                    _simulationForm.Changed -= SimulationFormOnChanged;
                }

                if (SetProperty(ref _simulationForm, value))
                {
                    if (_simulationForm is not null)
                    {
                        _simulationForm.IsModifiedChanged += SimulationFormOnIsModifiedChanged;
                        _simulationForm.IsValidChanged += SimulationFormOnIsValidChanged;
                        _simulationForm.Changed += SimulationFormOnChanged;
                    }

                    EventAggregator.GetEvent<SimulationFormChanged>().Publish(value);

                    QuitTreatmentActionWatchdog.SetObject(value);
                    CommandsCanExecuteChanged();
                }
            }
        }
        
        private ObservableTask _currentSimulationTask;
        public ObservableTask CurrentSimulationTask
        {
            get => _currentSimulationTask;
            set => SetProperty(ref _currentSimulationTask, value);
        }
        #endregion Properties


        #region Commands
        private DelegateCommand? _addCommand;
        public DelegateCommand AddCommand => _addCommand ??= new DelegateCommand(
            () =>   
            {
                if (TreatmentInfoStore.Diagnosis == null)
                    throw new InvalidOperationException("Cannot create a simulation: there is no field selected");

                TreatmentInfoStore.Simulation = null;

                SimulationForm = new SimulationState
                {
                    DiagnosisId = TreatmentInfoStore.Diagnosis.Id,
                    PerformedBy = AuthorizedUserStore.AuthorizedUser.EmailAddress,
                    TreatmentDevices = [DeviceType.Thyroid, DeviceType.LeadApron]
                };

                _simulationForms[SimulationForm.DiagnosisId] = SimulationForm;
            },
            // Allow adding only if the diagnosis already exists in the DB 
            // and the simulation is not blank (as we don't want to add a blank over an existing blank)
            canExecuteMethod: () => TreatmentInfoStore?.Diagnosis != null &&
                                    (SimulationForm == null || !BaseEntry.IsBlankEntry(SimulationForm)));


        private DelegateCommand? _saveCommand;
        public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
            ()=>
            {
                SaveSimulation();
                RemoveFormFromDictionary(TreatmentInfoStore.Diagnosis.Id);
            }, 
            canExecuteMethod: () => SimulationForm is not null &&
                                    SimulationForm.IsModified &&
                                    SimulationForm.IsValid);


        private DelegateCommand? _reloadCommand;
        public DelegateCommand ReloadCommand => _reloadCommand ??= new DelegateCommand(() =>
        {
            SimulationForm = null;

            RemoveFormFromDictionary(TreatmentInfoStore.Diagnosis.Id);
            FetchSimulation();
        });

        private DelegateCommand? _retrySimulationTaskCommand;
        public DelegateCommand RetrySimulationCommand
        {
            get => _retrySimulationTaskCommand;
            set => SetProperty(ref _retrySimulationTaskCommand, value);
        }
        #endregion Commands


        #region Private methods

        private void RemoveFormFromDictionary(long diagnosisId)
        {
            _simulationForms.Remove(diagnosisId);
        }

        private void SimulationFormOnChanged(object sender, EventArgs e)
        {
            var simulationState = sender as ISimulationState;
            _simulationForms[_simulationForm.DiagnosisId] = simulationState; // store simulation form after editing
            EventAggregator.GetEvent<SimulationFormChanged>().Publish(simulationState);
        }

        private void SimulationFormOnIsModifiedChanged(object sender, bool e)
        {
            CommandsCanExecuteChanged();
        }
        
        private void SimulationFormOnIsValidChanged(object sender, bool e)
        {
            CommandsCanExecuteChanged();
        }

        private void SetSimulationFormLesionDepth()
        {
            if (BaseEntry.IsNullOrBlankEntry(SimulationForm) ||
                SimulationForm.LesionDepth > 0.0 ||
                SimulationForm.Status == SimulationStatus.Approved)
            {
                return;
            }

            if (AcquisitionResultStore.LesionInfo.LesionDepth.HasValue && AcquisitionResultStore.LesionInfo.LesionDepth.Value > 0.0)
            {
                SimulationForm.LesionDepth = AcquisitionResultStore.LesionInfo.LesionDepth.Value;
                SaveSimulation();
            }
        }
        
        private void FetchSimulation()
        {
            RetrySimulationCommand = new DelegateCommand(() =>
            {
                CurrentSimulationTask = new ObservableTask(FetchSimulationAsync(), StringConstants.EMR.FetchSimulationMessage);
            });
            RetrySimulationCommand.Execute();
        }

        private async Task FetchSimulationAsync()
        {
            try
            {
                if (TreatmentInfoStore.Diagnosis is null)
                {
                    TreatmentInfoStore.Simulation = null;
                }
                else
                {
                    var simulation = await SimulationRepository.FetchLatestSimulationAsync(TreatmentInfoStore.Diagnosis.Id);
                    ICollection<ITreatmentDevice>? treatmentDevices = null;
                    ICollection<IPatientPosition>? patientPositions = null;

                    if (simulation is not null)
                    {
                        treatmentDevices = await SimulationRepository.FetchTreatmentDevicesAsync(simulation.Id);
                        patientPositions = await SimulationRepository.FetchPatientPositionsAsync(simulation.Id);

                        var devicesObservable = new ObservableCollection<DeviceType>(treatmentDevices.Select(d => d.DeviceName));
                        var positionsObservable = new ObservableCollection<PatientPosition>(patientPositions.Select(p => p.Position));

                        if (_simulationForms.ContainsKey(simulation.DiagnosisId))
                        {
                            var newFormState = new SimulationState(simulation);
                            newFormState.PatientPositions = positionsObservable;
                            newFormState.TreatmentDevices = devicesObservable;
                            newFormState.AcceptChanges();
                            _simulationForms[simulation.DiagnosisId] = newFormState;
                        }
                    }

                    TreatmentInfoStore.SetSimulation(simulation, treatmentDevices, patientPositions);
                }
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync(
                    $"{StringConstants.EMR.FetchSimulationMessage}. {ex.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }

        private void SaveSimulation()
        {
            if (SimulationForm.LesionDepth is null)
                DialogService.ShowDialog("AcknowledgeSimulationView");

            RetrySimulationCommand = new DelegateCommand(() =>
            {
                CurrentSimulationTask = new ObservableTask(SaveSimulationAsync(), StringConstants.EMR.SaveSimulationMessage);
            });
            RetrySimulationCommand.Execute();
        }
        
        private bool ConfirmSimulationChanges()
        {
            // Warn about losing the current plan
            return DialogService.Confirmation(
                StringConstants.Common.ConfirmationDialogTitle,
                StringConstants.EMR.SimulationChangeConfirmation);
        }

        private async Task SaveSimulationAsync()
        {
            bool changesAffectPlan = false;

            if (TreatmentInfoStore.Prescription is not null && TreatmentInfoStore.Plan is not null)
            {
                changesAffectPlan =
                    TreatmentInfoStore.Simulation.TargetType != SimulationForm.TargetType &&
                    PlanModel.TreatmentFields != null &&
                    PlanModel.TreatmentFields.Count > 0;
            }

            if (!changesAffectPlan || ConfirmSimulationChanges())
            {
                //backup simulation data before trying to save results
                var simulation = TreatmentInfoStore.Simulation;

                try
                {
                    simulation = new Simulation(await SimulationRepository.SubmitAsync(SimulationForm, simulation));

                    // fetch positions and devices to determine which of them should be deleted
                    var oldPositions = await SimulationRepository.FetchPatientPositionsAsync(simulation.Id);
                    var oldDevices = await SimulationRepository.FetchTreatmentDevicesAsync(simulation.Id);

                    var patientPositions = await SimulationRepository.SavePatientPositionListAsync(simulation.Id, SimulationForm.PatientPositions, oldPositions);
                    var treatmentDevices = await SimulationRepository.SaveTreatmentDeviceListAsync(simulation.Id, SimulationForm.TreatmentDevices, oldDevices);

                    if (_simulationForms.TryGetValue(simulation.DiagnosisId, out var existingSimulationForm))
                    {
                        existingSimulationForm.IsModified = false;
                    }

                    if (TreatmentInfoStore.Diagnosis is not null) //TODO: SubmitAsync shouldn't change chain Patient -> Diagnosis -> Simulation
                    {
                        TreatmentInfoStore.SetSimulation(simulation, treatmentDevices, patientPositions);
                    }
                }
                catch (Exception ex)
                {
                    _ = LogWriter.LogAsync(
                        $"{StringConstants.EMR.SaveSimulationMessage}. {ex.Message}", 
                        LogRecordSeverity.Error, LogRecordType.System);
                    throw;
                }
            }
        }

        private void CommandsCanExecuteChanged()
        {
            AddCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            ReloadCommand.RaiseCanExecuteChanged();
        }

        private void OnSimulationChanged(object sender, ISimulation s)
        {
            var diagnosisId = TreatmentInfoStore.Diagnosis?.Id;

            if (diagnosisId != null &&
                _simulationForms.ContainsKey(diagnosisId.Value))
            {
                UpdateSimulationFormFromDictionary(diagnosisId.Value);
            }
            else if (s != null)
            {
                SimulationForm = new SimulationState(s)
                {
                    PerformedBy = AuthorizedUserStore.AuthorizedUser.EmailAddress
                };
                SimulationForm.IsModified = false;
            }
            else 
            {
                SimulationForm = null;
            }

            CommandsCanExecuteChanged();
        }

        private void OnDiagnosisChanged(object sender, IDiagnosis d)
        {
            if (d is not null &&
                _simulationForms.ContainsKey(d.Id))
            {
                UpdateSimulationFormFromDictionary(d.Id);

                TreatmentInfoStore.Simulation = new Simulation(SimulationForm);
            }
            else
            {
                FetchSimulation();
            }

            CommandsCanExecuteChanged();
        }

        private void UpdateSimulationFormFromDictionary(long diagnosisId)
        {
            if (_simulationForms.TryGetValue(diagnosisId, out var simulationForm))
            {
                //have to save IsModified state before Set because some updates of PatientPositions/TreatmentDevices can happen.
                //but it will set IsModified = True
                var isModified = simulationForm.IsModified;
                SimulationForm = simulationForm;
                SimulationForm.IsModified = isModified;
            }
        }

        private void OnPatientPositionsChanged(object sender, ICollection<IPatientPosition> patientPositions)
        {
            if (SimulationForm == null)
                return;

            IEnumerable<PatientPosition> positions = patientPositions?.Select(p => p.Position) ?? new List<PatientPosition>();

            var isModified = SimulationForm.IsModified;
            SimulationForm.PatientPositions = new ObservableCollection<PatientPosition>(positions);
            SimulationForm.IsModified = isModified;
        }

        private void OnTreatmentDevicesChanged(object sender, ICollection<ITreatmentDevice> treatmentDevices)
        {
            if (SimulationForm == null)
                return;
            
            IEnumerable<DeviceType> devices = treatmentDevices?.Select(d => d.DeviceName) ?? new List<DeviceType>();

            var isModified = SimulationForm.IsModified;
            SimulationForm.TreatmentDevices = new ObservableCollection<DeviceType>(devices);
            SimulationForm.IsModified = isModified;
        }
        #endregion

        /// <summary>
        /// Stores the simulation data while editing
        /// Key: DiagnosisId
        /// </summary>
        private readonly IDictionary<long, ISimulationState> _simulationForms = new Dictionary<long, ISimulationState>();
    }

    public class SimulationFormChanged : PubSubEvent<ISimulationState?>
    {
    }
}
