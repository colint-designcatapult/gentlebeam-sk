using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcHeaterCurrentConfigMethodsInvoker : AbstractChildGrpcInvoker<HeaterCurrentConfig>
    {
        public GrpcHeaterCurrentConfigMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new HeaterCurrentConfigService.HeaterCurrentConfigServiceClient(Channel);
        }

        public HeaterCurrentConfigService.HeaterCurrentConfigServiceClient Client { get; private set; }

        public override async Task<HeaterCurrentConfig> CreateAsync(HeaterCurrentConfig entry)
        {
            var request = new CreateHeaterCurrentConfigRequest { HeaterCurrentConfig = entry };
            request.HeaterCurrentConfig.ClearId();

            var response = await CallWithOptions(Client.CreateHeaterCurrentConfigAsync, request);
            return response.HeaterCurrentConfig;
        }
        public override async Task<HeaterCurrentConfig> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetHeaterCurrentConfigAsync,
                new GetHeaterCurrentConfigRequest { Id = entryId });
            return response.HeaterCurrentConfig;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteHeaterCurrentConfigAsync,
                new DeleteHeaterCurrentConfigRequest { Id = entryId });
            return true;
        }

        public override async Task<HeaterCurrentConfig> UpdateAsyncWithMask(HeaterCurrentConfig entry, FieldMask mask)
        {
            var request = new UpdateHeaterCurrentConfigRequest { HeaterCurrentConfig = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateHeaterCurrentConfigAsync, request);
            return response.HeaterCurrentConfig;
        }

        public override async Task<ICollection<HeaterCurrentConfig>> ReadListAsync(long presetId)
        {
            var response = await CallWithOptions(
                Client.ListHeaterCurrentConfigsAsync,
                new ListHeaterCurrentConfigsRequest { PresetConfigurationId = presetId });

            return response.HeaterCurrentConfigs
                .Where(h => h.PresetConfigurationId == presetId)
                .OrderBy(h => h.Id).ToList();
        }
    }

}
