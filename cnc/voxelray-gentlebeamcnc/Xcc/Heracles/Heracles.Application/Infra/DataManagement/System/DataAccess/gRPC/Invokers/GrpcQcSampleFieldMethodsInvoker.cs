using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.QcsampleFields.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcQcSampleFieldMethodsInvoker : AbstractChildGrpcInvoker<QCSampleField>
    {
        public GrpcQcSampleFieldMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new QCSampleFieldService.QCSampleFieldServiceClient(Channel);
        }

        public QCSampleFieldService.QCSampleFieldServiceClient Client { get; private set; }

        public override async Task<QCSampleField> CreateAsync(QCSampleField entry)
        {
            var request = new CreateQCSampleFieldRequest { Qcsamplefield = entry };
            if (request.Qcsamplefield.HasId)
                request.Qcsamplefield.ClearId();

            var response = await CallWithOptions(Client.CreateQCSampleFieldAsync, request);
            return response.Qcsamplefield;
        }
        public override async Task<QCSampleField> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetQCSampleFieldAsync,
                new GetQCSampleFieldRequest { QcsamplefieldId = entryId });
            return response.Qcsamplefield;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteQCSampleFieldAsync,
                new DeleteQCSampleFieldRequest { QcsamplefieldId = entryId });
            return true;
        }

        public override async Task<QCSampleField> UpdateAsyncWithMask(QCSampleField entry, FieldMask mask)
        {
            var request = new UpdateQCSampleFieldRequest { Qcsamplefield = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateQCSampleFieldAsync, request);
            return response.Qcsamplefield;
        }

        public override async Task<ICollection<QCSampleField>> ReadListAsync(long qcSampleId)
        {
            var response = await CallWithOptions(
                Client.ListQCSampleFieldsAsync,
                new ListQCSampleFieldsRequest { QcsampleId = qcSampleId });

            return response.Qcsamplefields;
        }
    }

}
