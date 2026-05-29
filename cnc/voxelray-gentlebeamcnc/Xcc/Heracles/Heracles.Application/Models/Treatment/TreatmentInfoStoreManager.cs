using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;

using Prism.Mvvm;

using System;
using System.Threading.Tasks;
using Xcc.Core.Logging;

namespace Heracles.Application.Models.Treatment
{
    public interface ITreatmentInfoStoreController
    {
        Task CurrentTask { get; }
        Task<IPlan> TryPrepareLoadedPlanAsync();
    }

    public class TreatmentInfoStoreManager : BindableBase, ITreatmentInfoStoreController
    {
        private Task currentTask;

        public Task CurrentTask { get => currentTask; private set => SetProperty(ref currentTask, value); }

        private readonly ITreatmentInfoStore treatmentInfoStore;
        private readonly ISimulationRepository simulationRepository;
        private readonly IPrescriptionRepository prescriptionRepository;
        private readonly IEmrPlanCommands emrPlanCommands; // TODO: probably need to replace with some other plan service later
        private readonly IEmrDiagnosisCommands emrDiagnosisCommands;
        private readonly IEmrPatientCommands emrPatientCommands;
        private readonly ILogRepository _logWriter;

        private void SetTask(Task task)
        {
            if (CurrentTask.IsCompleted)
            {
                CurrentTask = task;
            }
            else
            {
                throw new InvalidOperationException("Task planning error: there is a running task already");
            }
        }

        public TreatmentInfoStoreManager(
            ITreatmentInfoStore treatmentInfoStore,
            ISimulationRepository simulationQueries,
            IPrescriptionRepository prescriptionRepository,
            IEmrPlanCommands emrPlanCommands,
            IEmrDiagnosisCommands emrDiagnosisCommands,
            IEmrPatientCommands emrPatientCommands,
            ILogRepository logWriter)
        {
            this.treatmentInfoStore = treatmentInfoStore;
            this.simulationRepository = simulationQueries;
            this.prescriptionRepository = prescriptionRepository;
            this.emrPlanCommands = emrPlanCommands;
            this.emrDiagnosisCommands = emrDiagnosisCommands;
            this.emrPatientCommands = emrPatientCommands;
            this._logWriter = logWriter;
            //treatmentInfoStore.PropertyChanged += (s, e) => OnTreatmentInfoStoreChanged(e.PropertyName);
        }


        public async Task<IPlan> TryPrepareLoadedPlanAsync()
        {
            try
            {
                var plan = await emrPlanCommands.FindLoadedPlanAsync();
                if (plan == null)
                {
                    plan = await emrPlanCommands.FindPendingPlanAsync();
                }
                if (plan is not null)
                {
                    treatmentInfoStore.Plan = plan;
                    var prescription = await prescriptionRepository.FetchAsync(plan.PrescriptionId);
                    var simulation = await simulationRepository.FetchAsync(prescription.SimulationId);
                    var diagnosis = treatmentInfoStore.Diagnosis = await emrDiagnosisCommands.ReadAsync(simulation.DiagnosisId);
                    var patient = treatmentInfoStore.Patient = await emrPatientCommands.ReadAsync(diagnosis.PatientId);
                }
                return plan;
            }
            catch (Exception ex)
            {
                _logWriter.Log($"Error on loaded plan recovery: {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
                return null;
            }

        }




        private async Task<ISimulation> FetchSimulationAsync(long diagnosisId)
        {
            return treatmentInfoStore.Simulation = 
                await simulationRepository.FetchLatestSimulationAsync(diagnosisId);
        }

        private async Task<IPrescription> FetchPrescriptionAsync(long simulationId)
        {
            return treatmentInfoStore.Prescription = 
                await prescriptionRepository.FetchLatestPrescriptionAsync(simulationId);
        }


    }
}
