using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Warmups.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcWarmupMethodsInvoker : AbstractChildGrpcInvoker<Warmup>
    {
        public GrpcWarmupMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new WarmupService.WarmupServiceClient(Channel);
        }

        public WarmupService.WarmupServiceClient Client { get; private set; }

        public override async Task<Warmup> CreateAsync(Warmup entry)
        {
            var request = new CreateWarmupRequest { Warmup = entry };
            request.Warmup.ClearId();

            var response = await CallWithOptions(Client.CreateWarmupAsync, request);
            return response.Warmup;
        }
        public override async Task<Warmup> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetWarmupAsync,
                new GetWarmupRequest { Id = entryId });
            return response.Warmup;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteWarmupAsync,
                new DeleteWarmupRequest { Id = entryId });
            return true;
        }

        public override async Task<Warmup> UpdateAsyncWithMask(Warmup entry, FieldMask mask)
        {
            var request = new UpdateWarmupRequest { Warmup = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateWarmupAsync, request);
            return response.Warmup;
        }

        // We need to have it compatible with Morpheus now, as they share common Xcc WarmupService
        public override async Task<ICollection<Warmup>> ReadListAsync(long parentId = -1)
        {
            var response = await CallWithOptions(
                Client.ListWarmupsAsync,
                new ListWarmupsRequest { });

            return response.Warmups;
        }
    }

}
