using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.CoilConfigurations.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcCoilConfigurationMethodsInvoker : AbstractChildGrpcInvoker<CoilConfiguration>
    {
        public GrpcCoilConfigurationMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new CoilConfigurationService.CoilConfigurationServiceClient(Channel);
        }

        public CoilConfigurationService.CoilConfigurationServiceClient Client { get; private set; }

        public override async Task<CoilConfiguration> CreateAsync(CoilConfiguration entry)
        {
            var request = new CreateCoilConfigurationRequest { CoilConfiguration = entry };
            request.CoilConfiguration.ClearId();

            var response = await CallWithOptions(Client.CreateCoilConfigurationAsync, request);
            return response.CoilConfiguration;
        }
        public override async Task<CoilConfiguration> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetCoilConfigurationAsync,
                new GetCoilConfigurationRequest { Id = entryId });
            return response.CoilConfiguration;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteCoilConfigurationAsync,
                new DeleteCoilConfigurationRequest { Id = entryId });
            return true;
        }

        public override async Task<CoilConfiguration> UpdateAsyncWithMask(CoilConfiguration entry, FieldMask mask)
        {
            var request = new UpdateCoilConfigurationRequest { CoilConfiguration = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateCoilConfigurationAsync, request);
            return response.CoilConfiguration;
        }

        public override async Task<ICollection<CoilConfiguration>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListCoilConfigurationsAsync,
                new ListCoilConfigurationsRequest {PresetConfigurationId = parentId});

            return response.CoilConfigurations.OrderBy(c => c.Id).ToList();
        }
    }

}
