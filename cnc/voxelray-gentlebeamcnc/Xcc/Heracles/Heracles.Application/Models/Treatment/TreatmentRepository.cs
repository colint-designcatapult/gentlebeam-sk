using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heracles.Application.Models.Treatment
{
    public interface ITreatmentRepository
    {
        Task<ITreatment> FetchLatestTreatmentByPlanAsync(IPlan plan);
        Task<ICollection<IActualTreatmentField>> FetchTreatmentFieldsAsync(long treatmentId);
    }

    public class TreatmentRepository : ITreatmentRepository
    {
        public TreatmentRepository(
            IEmrTreatmentCommands treatmentCommands,
            IEmrActualTreatmentFieldCommands actualTreatmentFieldCommands)
        {
            TreatmentCommands = treatmentCommands;
            ActualTreatmentFieldCommands = actualTreatmentFieldCommands;
        }

        public IEmrTreatmentCommands TreatmentCommands { get; }
        public IEmrActualTreatmentFieldCommands ActualTreatmentFieldCommands { get; }

        public async Task<ITreatment> FetchLatestTreatmentByPlanAsync(IPlan plan)
        {
            var treatments = await TreatmentCommands.ReadListAsync(plan.Id);
            var lastTreatment = treatments.OrderBy(x => x.Id).LastOrDefault();
            if (lastTreatment != null)
            {
                lastTreatment = new RDBMS.EMR.Treatment(
                    lastTreatment,
                    plan,
                    await FetchTreatmentFieldsAsync(lastTreatment.Id));
            }
            return lastTreatment;
        }

        public Task<ICollection<IActualTreatmentField>> FetchTreatmentFieldsAsync(long treatmentId)
        {
            return ActualTreatmentFieldCommands.ReadListAsync(treatmentId);
        }
    }
}
