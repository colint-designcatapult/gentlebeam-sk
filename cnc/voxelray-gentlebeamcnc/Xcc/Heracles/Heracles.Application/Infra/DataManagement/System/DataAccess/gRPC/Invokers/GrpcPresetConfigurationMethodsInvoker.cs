using Com.Empyreanmed.Heracles.PresetConfigurations.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcPresetConfigurationMethodsInvoker : AbstractChildGrpcInvoker<PresetConfiguration>
    {
        public GrpcPresetConfigurationMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new PresetConfigurationService.PresetConfigurationServiceClient(Channel);
        }

        public PresetConfigurationService.PresetConfigurationServiceClient Client { get; private set; }

        public override async Task<PresetConfiguration> CreateAsync(PresetConfiguration entry)
        {
            var request = new CreatePresetConfigurationRequest { PresetConfiguration = entry };
            request.PresetConfiguration.ClearId();

            var response = await CallWithOptions(Client.CreatePresetConfigurationAsync, request);
            return response.PresetConfiguration;
        }
        public override async Task<PresetConfiguration> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetPresetConfigurationAsync,
                new GetPresetConfigurationRequest { Id = entryId });
            return response.PresetConfiguration;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeletePresetConfigurationAsync,
                new DeletePresetConfigurationRequest { Id = entryId });
            return true;
        }

        public override async Task<PresetConfiguration> UpdateAsyncWithMask(PresetConfiguration entry, FieldMask mask)
        {
            var request = new UpdatePresetConfigurationRequest { PresetConfiguration = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdatePresetConfigurationAsync, request);
            return response.PresetConfiguration;
        }

        public override async Task<ICollection<PresetConfiguration>> ReadListAsync(long configurationId)
        {
            var response = await CallWithOptions(
                Client.ListPresetConfigurationsAsync,
                new ListPresetConfigurationsRequest { CollimatorConfigurationId = configurationId });

            return response.PresetConfigurations.OrderBy(p => p.Id).ToList();
        }

        public async Task<PresetConfiguration> ApproveAsync(long entryId, string username, string password)
        {
            var response = await CallWithOptions(
                Client.ApprovePresetConfigurationAsync,
                    new ApprovePresetConfigurationRequest { 
                        PresetConfigurationId = entryId, Username = username, Password = password 
                    });

            return response.ApprovedPresetConfiguration;
        }
    }

}
