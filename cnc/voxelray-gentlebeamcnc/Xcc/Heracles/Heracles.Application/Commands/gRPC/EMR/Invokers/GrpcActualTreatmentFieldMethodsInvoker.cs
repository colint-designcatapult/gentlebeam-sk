using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcActualTreatmentFieldMethodsInvoker : AbstractChildGrpcInvoker<ActualTreatmentField>
    {
        public GrpcActualTreatmentFieldMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new ActualTreatmentFieldService.ActualTreatmentFieldServiceClient(Channel);
        }

        public ActualTreatmentFieldService.ActualTreatmentFieldServiceClient Client { get; private set; }

        public override async Task<ActualTreatmentField> CreateAsync(ActualTreatmentField entry)
        {
            var request = new CreateActualTreatmentFieldRequest { ActualTreatmentField = entry };
            request.ActualTreatmentField.ClearId();

            var response = await CallWithOptions(Client.CreateActualTreatmentFieldAsync, request);
            return response.ActualTreatmentField;
        }
        public override async Task<ActualTreatmentField> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetActualTreatmentFieldAsync,
                new GetActualTreatmentFieldRequest { ActualTreatmentFieldId = entryId });
            return response.ActualTreatmentField;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteActualTreatmentFieldAsync,
                new DeleteActualTreatmentFieldRequest { ActualTreatmentFieldId = entryId });
            return true;
        }

        public override async Task<ActualTreatmentField> UpdateAsyncWithMask(ActualTreatmentField entry, FieldMask mask)
        {
            var request = new UpdateActualTreatmentFieldRequest { ActualTreatmentField = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateActualTreatmentFieldAsync, request);
            return response.ActualTreatmentField;
        }

        public override async Task<ICollection<ActualTreatmentField>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListActualTreatmentFieldsAsync,
                new ListActualTreatmentFieldsRequest { TreatmentFieldId = parentId });

            return response.ActualTreatmentFields;
        }
    }
}
