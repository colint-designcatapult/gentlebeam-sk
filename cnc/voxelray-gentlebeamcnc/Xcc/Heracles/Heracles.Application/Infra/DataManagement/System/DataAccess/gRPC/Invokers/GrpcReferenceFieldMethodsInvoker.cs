using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.ReferenceFields.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcReferenceFieldMethodsInvoker : AbstractChildGrpcInvoker<ReferenceField>
    {
        public GrpcReferenceFieldMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new ReferenceFieldService.ReferenceFieldServiceClient(Channel);
        }

        public ReferenceFieldService.ReferenceFieldServiceClient Client { get; private set; }

        public override async Task<ReferenceField> CreateAsync(ReferenceField entry)
        {
            var request = new CreateReferenceFieldRequest { ReferenceField = entry };
            request.ReferenceField.ClearId();

            var response = await CallWithOptions(Client.CreateReferenceFieldAsync, request);
            return response.ReferenceField;
        }
        public override async Task<ReferenceField> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetReferenceFieldAsync,
                new GetReferenceFieldRequest { Id = entryId });
            return response.ReferenceField;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteReferenceFieldAsync,
                new DeleteReferenceFieldRequest { Id = entryId });
            return true;
        }

        public override async Task<ReferenceField> UpdateAsyncWithMask(ReferenceField entry, FieldMask mask)
        {
            var request = new UpdateReferenceFieldRequest { ReferenceField = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateReferenceFieldAsync, request);
            return response.ReferenceField;
        }

        public override async Task<ICollection<ReferenceField>> ReadListAsync(long presetId)
        {
            var response = await CallWithOptions(
                Client.ListReferenceFieldsAsync,
                new ListReferenceFieldsRequest { PresetConfigurationId = presetId });

            return response.ReferenceFields.OrderBy(rf => rf.Id).ToList();
        }
    }

}
