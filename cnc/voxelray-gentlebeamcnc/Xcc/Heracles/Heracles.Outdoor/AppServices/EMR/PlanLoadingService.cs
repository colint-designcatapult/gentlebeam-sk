using Heracles.Application.Infra.DataManagement.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;

namespace Heracles.External.AppServices.Plan
{
    public class PlanLoadingService
    {
        public PlanLoadingService(
            ITreatmentInfoStore treatmentInfoStore,
            IPatientRepository patientRepository,
            IEmrDiagnosisCommands diagnosisCommands,
            IEmrSimulationCommands simulationCommands,
            IEmrPrescriptionCommands prescriptionCommands,
            IPlanRepository planRepository)
        {
            TreatmentInfoStore = treatmentInfoStore;
            PatientRepository = patientRepository;
            DiagnosisCommands = diagnosisCommands;
            SimulationCommands = simulationCommands;
            PrescriptionCommands = prescriptionCommands;
            PlanRepository = planRepository;
        }

        public ITreatmentInfoStore TreatmentInfoStore { get; }
        public IPatientRepository PatientRepository { get; }
        public IEmrDiagnosisCommands DiagnosisCommands { get; }
        public IEmrSimulationCommands SimulationCommands { get; }
        public IEmrPrescriptionCommands PrescriptionCommands { get; }
        public IPlanRepository PlanRepository { get; }

        public async Task<ITreatmentInfoStore> FetchPlanDataAsync(IPlan? plan, bool forceReload = false)
        {
            // This will save us some time: don't reload the same plan if not asked to
            if (plan?.Id == TreatmentInfoStore.Plan?.Id && TreatmentInfoStore.IsComplete() && !forceReload)
            {
                return TreatmentInfoStore;
            }

            TreatmentInfoStore.Reset();
            if (plan != null)
            {
                TreatmentInfoStore.Plan = new Application.Models.RDBMS.EMR.Plan(
                    plan,
                    await PlanRepository.FetchTreatmentFieldsAsync(plan.Id, plan.CollimatorType));

                TreatmentInfoStore.Prescription = await PrescriptionCommands.ReadAsync(TreatmentInfoStore.Plan.PrescriptionId);
                TreatmentInfoStore.Simulation = await SimulationCommands.ReadAsync(TreatmentInfoStore.Prescription.SimulationId);
                TreatmentInfoStore.Diagnosis = await DiagnosisCommands.ReadAsync(TreatmentInfoStore.Simulation.DiagnosisId);
                TreatmentInfoStore.Patient = await PatientRepository.FetchAsync(TreatmentInfoStore.Diagnosis.PatientId);
            }

            return TreatmentInfoStore;
        }
    }
}
