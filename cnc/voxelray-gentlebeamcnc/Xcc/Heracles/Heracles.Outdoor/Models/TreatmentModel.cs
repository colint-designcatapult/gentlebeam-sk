using Grpc.Core;
using Heracles.Application.Common;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Application.Models.Treatment;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Heracles.External.Models
{
    public interface ITreatmentModel
    {
        ITreatment Treatment { get; }
        Task<ITreatment> SaveTreatmentData();
        void CloseTreatment();
        void SetTreatment(ITreatment treatment);
    }
    public class TreatmentModel : ITreatmentModel
    {
        public TreatmentModel(ILogRepository logWriter,
                              IPlanModel planModel,
                              IAuthorizedUserStore authorizedUserStore,
                              ITreatmentInfoStore treatmentInfoStore,
                              IEmrTreatmentCommands treatmentCommands,
                              IPatientRepository patientRepository,
                              IEmrPlanCommands planCommands,
                              IEmrPrescriptionCommands prescriptionCommands,
                              IEmrSimulationCommands simulationCommands,
                              IDialogService dialogService)
        {
            LogWriter = logWriter;
            PlanModel = planModel;
            AuthorizedUserStore = authorizedUserStore;
            TreatmentInfoStore = treatmentInfoStore;
            TreatmentCommands = treatmentCommands;
            PatientRepository = patientRepository;
            PlanCommands = planCommands;
            PrescriptionCommands = prescriptionCommands;
            SimulationCommands = simulationCommands;
            DialogService = dialogService;
        }

        #region Properties
        private IEmrTreatmentCommands TreatmentCommands { get; }
        private IPatientRepository PatientRepository { get; }
        private IEmrPlanCommands PlanCommands { get; }
        private IEmrPrescriptionCommands PrescriptionCommands { get; }
        private IEmrSimulationCommands SimulationCommands { get; }
        public IDialogService DialogService { get; }
        private ILogRepository LogWriter { get; }
        private IPlanModel PlanModel { get; }
        private IAuthorizedUserStore AuthorizedUserStore { get; }
        private ITreatmentInfoStore TreatmentInfoStore { get; }
        public ITreatment Treatment { get; private set; } = null;
        #endregion

        private double _previousCumulativeDose = 0.0;
        private int _previousFractionNumber = 0;

        public async Task<ITreatment> FetchLastTreatmentByPlan(long planId)
        {
            var treatments = await TreatmentCommands.ReadListAsync(planId);
            var lastTreatment = treatments.LastOrDefault();
            if (lastTreatment == null)
                throw new Exception($"Failed to fetch last Treatment for the Plan {planId}");

            if (DateTime.Now.Subtract(lastTreatment.CreationDate) > TimeSpan.FromHours(12))
            {
                if (!DialogService.Confirmation(
                    StringConstants.Common.ConfirmationDialogTitle,
                    StringConstants.TreatmentConsole.PlanRecoveryOldPreviousTreatmentConfirmation
                    ))
                {
                    return Treatment = null;
                }
            }

            return Treatment = lastTreatment;
        }

        public async Task<ITreatment> SaveTreatmentData()
        {
            if (!BaseEntry.IsNullOrBlankEntry(Treatment))
            {
                return Treatment;
            }
            else if (Treatment == null)
            {
                throw new NullReferenceException("Save treatment - error: treatment data is missing");
            }

            // Now, we have a blank treatment, and we need to create it in the DB along with a treatment visit:
            var lastVisit = TreatmentInfoStore.Patient?.Visit;
            try
            {
                // TODO: now Moses doesn't make any difference in visits by type, 
                // so even if we need a treatment visit, and the last one was for Simulation,
                // we are forced to use the last one, whatever it is
                lastVisit = await PatientRepository.GetSameDayVisitAsync(TreatmentInfoStore.Patient, DateTime.Now, Core.Enums.VisitType.Treatment);
                if (lastVisit != null && lastVisit == TreatmentInfoStore.Patient?.Visit)
                {
                    _ = LogWriter.LogAsync(
                        $"Treatment: there's a same day visit with id={lastVisit.Id}, it will be applied to the new treatment",
                        Xcc.Core.Enums.LogRecordSeverity.Warn, Xcc.Core.Enums.LogRecordType.System);
                }
            }
            catch (DataServiceException ex) when (ex.InnerException is RpcException exInner)
            {
                // In case if we thought that we should make a new visit, but moses decided otherwise:
                if (exInner.StatusCode == StatusCode.Internal)
                {
                    _ = LogWriter.LogAsync(
                        $"Failed to create a new visit. The previous one with id={lastVisit?.Id} will be applied to the new treatment.{Environment.NewLine}Error: {ex.Message}",
                        Xcc.Core.Enums.LogRecordSeverity.Warn, Xcc.Core.Enums.LogRecordType.System);
                }
                else
                    throw;
            }

            Treatment.VisitId = lastVisit.Id;

            return Treatment = new Treatment(
                await TreatmentCommands.CreateAsync(Treatment))
            {
                ActualTreatmentFields = Treatment.ActualTreatmentFields
            };
        }

        #region Private methods

        private async Task<ITreatment> UpdateTreatment(double currentDose) //todo: rewrite according to the requirements
        {            
            throw new NotImplementedException("UpdateTreatment: rewrite according to the requirements");
            
            await FetchPreviousData(PlanModel.Diagnosis.Id);
            
            var newTreatment = new Treatment(Treatment)
            {
                CumulativeDose = _previousCumulativeDose + currentDose
            };

            return await TreatmentCommands.UpdateAsync(Treatment, newTreatment);
        }

        private async Task FetchPreviousData(long diagnosisId)
        {
            long maxTreatmentId = 0L;
            _previousFractionNumber = 0;

            var simulations = await SimulationCommands.ReadListAsync(diagnosisId);
            foreach (var simulation in simulations)
            {
                var prescriptions = await PrescriptionCommands.ReadListAsync(simulation.Id);

                foreach (var prescription in prescriptions)
                {
                    var plans = await PlanCommands.ReadListAsync(prescription.Id);

                    foreach (var plan in plans)
                    {
                        var treatments = await TreatmentCommands.ReadListAsync(plan.Id);

                        foreach (var treatment in treatments)
                        {
                            if (treatment.Fraction > _previousFractionNumber)
                                _previousFractionNumber = treatment.Fraction;

                            if (treatment.Id > maxTreatmentId)
                            {
                                maxTreatmentId = treatment.Id;
                                _previousCumulativeDose = treatment.CumulativeDose;
                            }
                        }
                    }
                }
            }
        }

        public void CloseTreatment()
        {
            Treatment = null;
        }

        public void SetTreatment(ITreatment treatment)
        {
            Treatment = treatment;
        }

        #endregion
    }
}
