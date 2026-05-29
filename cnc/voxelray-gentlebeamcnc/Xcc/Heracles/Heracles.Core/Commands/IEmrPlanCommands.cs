using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Heracles.Core.Commands
{
    public interface IEmrPlanCommands : IAsyncChildEntryCommands<IPlan>
    {
        Task LoadForTreatmentAsync(long planId, bool isPartial); 
        Task TreatmentLoadAcknowledgeAsync(long planId);
        Task UnloadFromTreatmentAsync(long planId);
        Task<IPlan?> FindPendingPlanAsync();
        Task<IPlan?> FindLoadedPlanAsync();    
        Task<IPlan?> UpdateStatusAsync(string email, string password, long planId, PlanStatus status);
    }

    public interface ILoadForTreatmentEventStream : IEventStream<LoadForTreatmentEventsStreamArgs>
    {
    }

    public interface IPlanEventStream : IEventStream<IPlan>
    {
    }


    public class LoadForTreatmentEventsStreamArgs(IPlan plan, IPatient? patient)
    {
        public IPlan Plan { get; } = plan;
        public IPatient? Patient { get; } = patient;
    }
}
