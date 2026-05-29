using Heracles.Application.Common;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;

using Prism.Commands;
using Prism.Mvvm;

using Xcc.Application.Helpers;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.External.ViewModels
{
    public class PatientProfileViewModel : BindableBase
    {
        #region Contructors
        public PatientProfileViewModel(ITreatmentInfoStore treatmentInfoStore, IEmrTreatmentDeviceCommands emrTreatmentDeviceCommands, ILogRepository logWriter)
        {
            EmrTreatmentDeviceCommands = emrTreatmentDeviceCommands;
            LogWriter = logWriter;
            TreatmentInfoStore = treatmentInfoStore;
            TreatmentInfoStore.SimulationChanged += OnSimulationChanged;
        }

        public PatientProfileViewModel()
        {
            TreatmentInfoStore = new TreatmentInfoStore();
        }
        #endregion Contructors


        #region Read-only properties
        public ITreatmentInfoStore TreatmentInfoStore { get; }
        private IEmrTreatmentDeviceCommands EmrTreatmentDeviceCommands { get; }
        public ILogRepository LogWriter { get; }
        #endregion Read-only properties


        #region Commands
        private ObservableTask _currentTask;
        public ObservableTask CurrentTask
        {
            get => _currentTask;
            set => SetProperty(ref _currentTask, value);
        }

        private DelegateCommand? _retrySimulationTaskCommand;
        public DelegateCommand RetrySimulationCommand
        {
            get => _retrySimulationTaskCommand;
            set => SetProperty(ref _retrySimulationTaskCommand, value);
        }
        #endregion Commands


        #region Private methods
        private void OnSimulationChanged(object sender, Core.Models.EMR.ISimulation simulation)
        {
            if (simulation is not null)
            {
                FetchTreatmentDevices(simulation.Id);
            }
        }

        private void FetchTreatmentDevices(long parentId)
        {
            RetrySimulationCommand = new DelegateCommand(() =>
            {
                CurrentTask = new ObservableTask(
                    FetchTreatmentDevicesAsync(parentId), 
                    StringConstants.TreatmentConsole.FetchPatientDataMessage);
            });
            RetrySimulationCommand.Execute();
        }

        private async Task FetchTreatmentDevicesAsync(long parentId)
        {
            try
            {
                TreatmentInfoStore.TreatmentDevices = await EmrTreatmentDeviceCommands.ReadListAsync(parentId);
            }
            catch (Exception ex)
            {
                _ = LogWriter.LogAsync(
                    $"{StringConstants.TreatmentConsole.FetchPatientDataMessage}. {ex.Message}", 
                    LogRecordSeverity.Error, LogRecordType.System);
                throw;
            }
        }
        #endregion Private methods
    }
}
