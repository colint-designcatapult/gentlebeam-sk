using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Head.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcHeadMethodsInvoker : AbstractRootGrpcInvoker<Head>
    {

        public GrpcHeadMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new HeadService.HeadServiceClient(Channel);
        }

        public HeadService.HeadServiceClient Client { get; }

        public override async Task<Head> CreateAsync(Head entry)
        {
            var request = new CreateHeadRequest { Head = entry };
            if (request.Head.HasId)
                request.Head.ClearId();

            var response = await CallWithOptions(Client.CreateHeadAsync, request);
            return response.Head;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteHeadAsync,
                new DeleteHeadRequest { Id = entryId });
            return true;
        }

        public override async Task<ICollection<Head>> ReadAllAsync()
        {
            var response = await CallWithOptions(
                Client.ListHeadsAsync,
                new ListHeadsRequest());

            return response.Heads;
        }

        public override async Task<Head> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetHeadAsync,
                new GetHeadRequest { Id = entryId });
            return response.Head;
        }

        public override async Task<Head> UpdateAsyncWithMask(Head entry, FieldMask mask)
        {
            var request = new UpdateHeadRequest { Head = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateHeadAsync, request);
            return response.Head;
        }
    }

}
