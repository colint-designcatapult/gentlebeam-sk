using Heracles.Core.Commands;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System.Threading.Tasks;

namespace Heracles.Indoor.Models.UseCases
{
    public interface IPlanLoading
    {
        bool IsTargetsMismatch(TargetType planTargetType, TargetType targetType);

        bool CanLoadForTreatment(IPlan plan, TargetType targetType);

        Task LoadForTreatmentAsync(long planId, bool isPartial);
    }

    public class PlanLoading : IPlanLoading
    {
        public IEmrPlanCommands EmrPlanCommands { get; }


        public PlanLoading(IEmrPlanCommands emrPlanCommands)
        {
            EmrPlanCommands = emrPlanCommands;
        }

        public bool CanLoadForTreatment(IPlan plan, TargetType targetType)
        {
            if (plan == null)
                return false;

            // todo: check the conditions
            return plan.Status == PlanStatus.APPROVED &&
                    targetType == plan.CollimatorType;
        }

        public bool IsTargetsMismatch(TargetType planTargetType, TargetType targetType)
        {
            return planTargetType != targetType;
        }

        public Task LoadForTreatmentAsync(long planId, bool isPartial)
        {
            return EmrPlanCommands.LoadForTreatmentAsync(planId, isPartial);
        }

    }
}
