using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcEmissionTreatmentFieldMethodsInvoker : AbstractChildGrpcInvoker<EmissionTreatmentField>
    {
        public GrpcEmissionTreatmentFieldMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new EmissionTreatmentFieldService.EmissionTreatmentFieldServiceClient(Channel);
        }

        public EmissionTreatmentFieldService.EmissionTreatmentFieldServiceClient Client { get; private set; }

        public override async Task<EmissionTreatmentField> CreateAsync(EmissionTreatmentField entry)
        {
            var request = new CreateEmissionTreatmentFieldRequest { EmissionTreatmentField = entry };
            request.EmissionTreatmentField.ClearId();

            var response = await CallWithOptions(Client.CreateEmissionTreatmentFieldAsync, request);
            return response.EmissionTreatmentField;
        }
        public override async Task<EmissionTreatmentField> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetEmissionTreatmentFieldAsync,
                new GetEmissionTreatmentFieldRequest { EmissionTreatmentFieldId = entryId });
            return response.EmissionTreatmentField;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteEmissionTreatmentFieldAsync,
                new DeleteEmissionTreatmentFieldRequest { EmissionTreatmentFieldId = entryId });
            return true;
        }

        public override async Task<EmissionTreatmentField> UpdateAsyncWithMask(EmissionTreatmentField entry, FieldMask mask)
        {
            var request = new UpdateEmissionTreatmentFieldRequest { EmissionTreatmentField = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateEmissionTreatmentFieldAsync, request);
            return response.EmissionTreatmentField;
        }

        public override async Task<ICollection<EmissionTreatmentField>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListEmissionTreatmentFieldsAsync,
                new ListEmissionTreatmentFieldsRequest { ActualTreatmentFieldId = parentId });

            return response.EmissionTreatmentFields;
        }
    }
}
