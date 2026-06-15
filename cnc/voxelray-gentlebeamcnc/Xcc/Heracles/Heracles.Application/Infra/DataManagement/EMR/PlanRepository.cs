using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heracles.Application.Helpers;
using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Infra.DataManagement.EMR
{
    public interface IPlanRepository
    {
        Task<ICollection<ITreatmentField>> FetchTreatmentFieldsAsync(long planId, TargetType collimatorType);
        Task<(IPlan, ICollection<ITreatmentField>)> SaveAsync(IPlan plan, IEnumerable<ITreatmentField> treatmentFields);
        Task<IPlan?> FetchLatestPlanAsync(long prescriptionId);
        Task<ICollection<ITreatmentField>> FetchTreatmentFieldsAsync(long planId);
        Task<ICollection<ITreatmentField>> FetchOrderedTreatmentFieldsAsync(long planId);
        Task<IPlan> FindPendingPlanAsync();
        Task<IPlan> CreatePlanAsync(IPlan plan);
        Task<IPlan> UpdatePlanAsync(IPlan? oldPlan, IPlan newPlan);
        Task<IPlan> UpdateStatusAsync(string email, string password, long planId, PlanStatus status);
        Task UnloadFromTreatmentAsync(long planId);
        Task<ITreatmentField> CreateTreatmentFieldAsync(ITreatmentField treatmentField);
        Task<ITreatmentField> UpdateTreatmentFieldAsync(ITreatmentField? old, ITreatmentField treatmentField);
        Task DeleteTreatmentFieldAsync(long treatmentFieldId);
    }

    public class PlanRepository : IPlanRepository
    {
        public PlanRepository(
            IEmrPlanCommands emrPlanCommands,
            IEmrTreatmentFieldCommands emrTreatmentFieldCommands)
        {
            PlanCommands = emrPlanCommands;
            TreatmentFieldCommands = emrTreatmentFieldCommands;
        }

        public IEmrPlanCommands PlanCommands { get; }
        public IEmrTreatmentFieldCommands TreatmentFieldCommands { get; }


        public async Task<ICollection<ITreatmentField>> FetchTreatmentFieldsAsync(long planId, TargetType collimatorType)
        {
            var treatmentFieldNameMapping = 
                TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(collimatorType);
            
            var orderedFields = await FetchTreatmentFieldsAsync(planId);
            foreach (var field in orderedFields)
            {
                field.DisplayValue = TargetTypeConverter.GetBackwardFieldNameMapping(treatmentFieldNameMapping, field.Name);
            }
            return orderedFields;
        }
        
        public async Task<(IPlan, ICollection<ITreatmentField>)> SaveAsync(IPlan plan, IEnumerable<ITreatmentField> treatmentFields)
        {
            IPlan savedPlan;
            if (BaseEntry.IsBlankEntry(plan))
                savedPlan = await PlanCommands.CreateAsync(plan);
            else
                savedPlan = await PlanCommands.UpdateAsync(null, plan);

            var list = treatmentFields.ToList();

            var savedTreatmentFields = new List<ITreatmentField>(list);
            foreach (var tf in list)
            {
                tf.PlanId = savedPlan.Id;
                ITreatmentField savedTf;
                if (BaseEntry.IsBlankEntry(tf))
                    savedTf = await TreatmentFieldCommands.CreateAsync(tf);
                else
                    savedTf = await TreatmentFieldCommands.UpdateAsync(null!, tf);
                savedTreatmentFields.Add(savedTf);
            }

            return (savedPlan, savedTreatmentFields);
        }
        
        public Task<IPlan> FindPendingPlanAsync()
        {
            return PlanCommands.FindPendingPlanAsync();
        }

        public async Task<IPlan?> FetchLatestPlanAsync(long prescriptionId)
        {
            var plans = await PlanCommands.ReadListAsync(prescriptionId);
            return plans?.OrderBy(p => p.Id).LastOrDefault();
        }

        public async Task<ICollection<ITreatmentField>> FetchTreatmentFieldsAsync(long planId)
        {
            var fetchedTreatmentFields = await TreatmentFieldCommands.ReadListAsync(planId);

            return fetchedTreatmentFields.ToList();
        }

        public async Task<ICollection<ITreatmentField>> FetchOrderedTreatmentFieldsAsync(long planId)
        {
            var fetchedTreatmentFields = await FetchTreatmentFieldsAsync(planId);

            return fetchedTreatmentFields.OrderBy(field => field.Id).ToList();
        }

        public Task<IPlan> CreatePlanAsync(IPlan plan)
        {
            return PlanCommands.CreateAsync(plan);
        }

        public Task<IPlan> UpdatePlanAsync(IPlan? oldPlan, IPlan newPlan)
        {
            return PlanCommands.UpdateAsync(oldPlan, newPlan);
        }

        public Task<IPlan> UpdateStatusAsync(string email, string password, long planId, PlanStatus status)
        {
            return PlanCommands.UpdateStatusAsync(email, password, planId, status);
        }

        public Task UnloadFromTreatmentAsync(long planId)
        {
            return PlanCommands.UnloadFromTreatmentAsync(planId);
        }

        public Task<ITreatmentField> CreateTreatmentFieldAsync(ITreatmentField treatmentField)
        {
            return TreatmentFieldCommands.CreateAsync(treatmentField);
        }

        public Task<ITreatmentField> UpdateTreatmentFieldAsync(ITreatmentField? old, ITreatmentField treatmentField)
        {
            return TreatmentFieldCommands.UpdateAsync(old, treatmentField);
        }

        public Task DeleteTreatmentFieldAsync(long treatmentFieldId)
        {
            return TreatmentFieldCommands.DeleteAsync(treatmentFieldId);
        }
    }
}
