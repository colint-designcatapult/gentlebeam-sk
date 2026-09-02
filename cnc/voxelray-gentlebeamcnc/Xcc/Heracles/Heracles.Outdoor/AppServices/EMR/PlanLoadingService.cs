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
                TreatmentPlanFieldRules.EnsureValid(TreatmentInfoStore.Plan.TreatmentFields);
                return TreatmentInfoStore;
            }

            if (plan is null)
            {
                TreatmentInfoStore.Reset();
                return TreatmentInfoStore;
            }

            var treatmentFields = await PlanRepository.FetchTreatmentFieldsAsync(plan.Id, plan.CollimatorType);
            TreatmentPlanFieldRules.EnsureValid(treatmentFields);
            var hydratedPlan = new Application.Models.RDBMS.EMR.Plan(plan, treatmentFields);
            var prescription = await PrescriptionCommands.ReadAsync(hydratedPlan.PrescriptionId);
            var simulation = await SimulationCommands.ReadAsync(prescription.SimulationId);
            var diagnosis = await DiagnosisCommands.ReadAsync(simulation.DiagnosisId);
            var patient = await PatientRepository.FetchAsync(diagnosis.PatientId);

            TreatmentInfoStore.Reset();
            TreatmentInfoStore.Plan = hydratedPlan;
            TreatmentInfoStore.Prescription = prescription;
            TreatmentInfoStore.Simulation = simulation;
            TreatmentInfoStore.Diagnosis = diagnosis;
            TreatmentInfoStore.Patient = patient;

            return TreatmentInfoStore;
        }
    }
}
