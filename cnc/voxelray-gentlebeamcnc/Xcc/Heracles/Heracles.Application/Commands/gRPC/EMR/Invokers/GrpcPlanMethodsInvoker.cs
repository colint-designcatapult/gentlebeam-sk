using Com.Empyreanmed.Heracles.Plans.V1;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Infra.gRPC;
using Xcc.Core.Models;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcPlanMethodsInvoker : AbstractChildGrpcInvoker<Plan>
    {
        public PlanService.PlanServiceClient Client { get; }
        public IAppGlobals AppGlobals { get; }
        
        public GrpcPlanMethodsInvoker(
            IAppGlobals appGlobals,
            IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new PlanService.PlanServiceClient(Channel);
            AppGlobals = appGlobals;
        }

        public override async Task<ICollection<Plan>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListPlansAsync,
                new ListPlansRequest { PrescriptionId = parentId });
            return response.Plans;
        }

        public override async Task<Plan> CreateAsync(Plan entry)
        {
            var request = new CreatePlanRequest { Plan = entry };
            request.Plan.ClearId();

            var response = await CallWithOptions(Client.CreatePlanAsync, request);
            return response.Plan;
        }

        public override async Task<Plan> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetPlanAsync,
                new GetPlanRequest { PlanId = entryId });
            return response.Plan;
        }

        public override async Task<Plan> UpdateAsyncWithMask(Plan entry, FieldMask mask)
        {
            var request = new UpdatePlanRequest { Plan = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdatePlanAsync, request);
            return response.Plan;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeletePlanAsync, 
                new DeletePlanRequest { PlanId = entryId });
            return true;
        }

        public async Task LoadForTreatmentAsync(long planId, bool isPartial)
        {
            var response = await Client.LoadForTreatmentAsync(
                new LoadForTreatmentRequest { Id = planId, IsPartial = isPartial },
                GetCallOptions().WithCancellationToken(AppGlobals.AppCancellationTokenSource.Token)
                );
        }

        public async Task<Plan> FindPendingPlanRequestAsync()
        {
            var response = await CallWithOptions(
                Client.FindPendingPlanAsync,
                new FindPendingPlanRequest { });
            return response.Plan;
        }

        public async Task<Plan> FindLoadedPlanRequestAsync()
        {
            var response = await CallWithOptions(
                Client.FindLoadedPlanAsync,
                new FindLoadedPlanRequest { });
            return response.Plan;
        }

        public async Task TreatmentLoadAcknowledgeAsync(long planId)
        {
            var response = await Client.TreatmentLoadAckAsync(
                new TreatmentLoadAckRequest { Id = planId },
                GetCallOptions().WithCancellationToken(AppGlobals.AppCancellationTokenSource.Token)
                );
        }

        public async Task UnloadFromTreatmentAsync(long planId)
        {
            var response = await Client.UnloadFromTreatmentAsync(
                new UnloadFromTreatmentRequest { Id = planId }, GetCallOptions());
        }

        public IDataStreamReader<PlanEventsResponse> OpenPlanEventsStream(CancellationToken cancellationToken)
        {
            var streamCallOptions = new CallOptions().WithCancellationToken(cancellationToken).WithHeaders(GrpcSettings.Headers);
            var response = Client.PlanEvents(new PlanEventsRequest(), streamCallOptions);
            return new GrpcStreamReader<PlanEventsResponse>(response, cancellationToken);
        }

        public IDataStreamReader<LoadForTreatmentEventsResponse> OpenLoadForTreatmentEventsStream(CancellationToken cancellationToken)
        {
            var streamCallOptions = new CallOptions().WithCancellationToken(cancellationToken).WithHeaders(GrpcSettings.Headers);
            var response = Client.LoadForTreatmentEvents(new LoadForTreatmentEventsRequest(), streamCallOptions);
            return new GrpcStreamReader<LoadForTreatmentEventsResponse>(response, cancellationToken);
        }

        public async Task<Plan> UpdateStatusAsync(string email, string password, long planId, Core.Enums.PlanStatus status)
        {
            var request = new UpdatePlanPrescriptionSimulationStatusRequest {
                Username = email,
                Password = password,
                PlanId = planId,
                Status = ProtoTypesConverter.ToProto(status)
            };
            var response = await CallWithOptions(Client.UpdatePlanPrescriptionSimulationStatusAsync, request);

            return response.UpdatedPlan;
        }

    }
}
