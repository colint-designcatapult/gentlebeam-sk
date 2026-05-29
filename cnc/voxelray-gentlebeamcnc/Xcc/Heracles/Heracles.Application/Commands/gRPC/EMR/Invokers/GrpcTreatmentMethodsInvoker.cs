using Com.Empyreanmed.Heracles.Treatments.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcTreatmentMethodsInvoker : AbstractChildGrpcInvoker<Treatment>
    {
        public GrpcTreatmentMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new TreatmentService.TreatmentServiceClient(Channel);
        }

        public TreatmentService.TreatmentServiceClient Client { get; private set; }

        public override async Task<Treatment> CreateAsync(Treatment entry)
        {
            var request = new CreateTreatmentRequest { Treatment = entry };
            request.Treatment.ClearId();
            //request.Treatment.ClearVisitId(); // todo: Moses should take care about Visits, so Visit parameter should be optional here

            var response = await CallWithOptions(Client.CreateTreatmentAsync, request);
            return response.Treatment;
        }
        public override async Task<Treatment> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetTreatmentAsync,
                new GetTreatmentRequest { TreatmentId = entryId });
            return response.Treatment;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteTreatmentAsync,
                new DeleteTreatmentRequest { TreatmentId = entryId });
            return true;
        }

        public override async Task<Treatment> UpdateAsyncWithMask(Treatment entry, FieldMask mask)
        {
            var request = new UpdateTreatmentRequest { Treatment = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateTreatmentAsync, request);
            return response.Treatment;
        }

        public override async Task<ICollection<Treatment>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListTreatmentsByPlanIdAsync,
                new ListTreatmentsByPlanIdRequest { Id = parentId });

            return response.Treatments;
        }
    }
}
