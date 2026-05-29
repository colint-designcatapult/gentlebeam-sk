using Com.Empyreanmed.Heracles.Prescriptions.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcPrescriptionMethodsInvoker : AbstractChildGrpcInvoker<Prescription>
    {
        public GrpcPrescriptionMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new PrescriptionService.PrescriptionServiceClient(Channel);
        }

        public PrescriptionService.PrescriptionServiceClient Client { get; }

        public override async Task<Prescription> CreateAsync(Prescription entry)
        {
            var request = new CreatePrescriptionRequest { Prescription = entry };
            request.Prescription.ClearId();

            var response = await CallWithOptions(Client.CreatePrescriptionAsync, request);
            return response.Prescription;
        }
        public override async Task<Prescription> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetPrescriptionAsync,
                new GetPrescriptionRequest { Id = entryId });
            return response.Prescription;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeletePrescriptionAsync,
                new DeletePrescriptionRequest { PrescriptionId = entryId });
            return true;
        }

        public override async Task<Prescription> UpdateAsyncWithMask(Prescription entry, FieldMask mask)
        {
            var request = new UpdatePrescriptionRequest { Prescription = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdatePrescriptionAsync, request);
            return response.Prescription;
        }

        public override async Task<ICollection<Prescription>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListPrescriptionsAsync,
                new ListPrescriptionsRequest { SimulationId = parentId });

            return response.Prescriptions;
        }
    }
}
