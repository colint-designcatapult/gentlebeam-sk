using Com.Empyreanmed.Heracles.Plans.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Exceptions;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using Xcc.Infra.UserSessions.BearerToken;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcPlanCommands 
        : ChildEntryCommandWrapper<IPlan, Plan, GrpcPlanMethodsInvoker>
        , IEmrPlanCommands
    {
        public GrpcPlanCommands(GrpcPlanMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }

        public async Task<IPlan?> FindPendingPlanAsync()
        {
            try
            {
                var plan = await Invoker.FindPendingPlanRequestAsync();
                return plan == null || plan.HasId == false ? null : ConvertFromProto(plan);
            }
            catch (Exception ex)
            {
                string msg = "Failed to execute pending plan search";
                throw new DataServiceException(msg, ex);
            }
        }

        public async Task<IPlan?> FindLoadedPlanAsync()
        {
            try
            {
                var plan = await Invoker.FindLoadedPlanRequestAsync();
                return plan == null || plan.HasId == false ? null : ConvertFromProto(plan);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to exectute loaded plan search";
                throw new DataServiceException(msg, ex);
            }
        }

        public async Task LoadForTreatmentAsync(long planId, bool isPartial)
        {
            try
            {
                await Invoker.LoadForTreatmentAsync(planId, isPartial);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to load for treatment: planId = {planId}";
                throw new DataServiceException(msg, ex);
            }
        }

        public async Task TreatmentLoadAcknowledgeAsync(long planId)
        {
            try
            {
                await Invoker.TreatmentLoadAcknowledgeAsync(planId);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to acknowledge treatment load: planId = {planId}";
                throw new DataServiceException(msg, ex);
            }
        }

        public async Task UnloadFromTreatmentAsync(long planId)
        {
            try
            {
                await Invoker.UnloadFromTreatmentAsync(planId);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to unload from treatment: planId = {planId}";
                throw new PlanForTreatmentException(msg, ex);
            }
        }

        public async Task<IPlan?> UpdateStatusAsync(string email, string password, long planId, Core.Enums.PlanStatus status)
        {
            try
            {
                var plan = await Invoker.UpdateStatusAsync(email, password, planId, status);
                return plan == null || plan.HasId == false ? null : ConvertFromProto(plan);
            }
            catch (Exception ex)
            {
                string msg = $"Failed to update plan status";
                throw new DataServiceException(msg, ex);
            }

        }
    }

    public class GrpcPlanEventStream(
        Invokers.GrpcPlanMethodsInvoker invoker,
        IBearerTokenUserSessionManager userSessionManager)
        : AbstractGrpcEventStream<IPlan>(userSessionManager)
        , IPlanEventStream
    {
        protected override async Task HandleStreamAsync(Action<IPlan> streamCallback, CancellationToken cancellationToken)
        {
            using var stream = invoker.OpenPlanEventsStream(cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                var data = await stream.ReceiveAsync();
                if (data?.Plan != null)
                {
                    streamCallback.Invoke(ProtoTypesConverter.FromProto(data.Plan));
                }
            }
        }
    }

    public class GrpcLoadForTreatmentEventStream(
        Invokers.GrpcPlanMethodsInvoker invoker,
        IBearerTokenUserSessionManager userSessionManager)
        : AbstractGrpcEventStream<LoadForTreatmentEventsStreamArgs>(userSessionManager)
        , ILoadForTreatmentEventStream
    {
        protected override async Task HandleStreamAsync(Action<LoadForTreatmentEventsStreamArgs> streamCallback, CancellationToken token)
        {
            using var stream = invoker.OpenLoadForTreatmentEventsStream(token);
            while (!token.IsCancellationRequested)
            {
                var data = await stream.ReceiveAsync();
                if (data?.Plan != null)
                {
                    IPlan plan = ProtoTypesConverter.FromProto(data.Plan);
                    IPatient? patient = (data.Patient is null) ? null : ProtoTypesConverter.FromProto(data.Patient);
                    streamCallback.Invoke(new LoadForTreatmentEventsStreamArgs(plan, patient));
                }
            }
        }
    }
}
