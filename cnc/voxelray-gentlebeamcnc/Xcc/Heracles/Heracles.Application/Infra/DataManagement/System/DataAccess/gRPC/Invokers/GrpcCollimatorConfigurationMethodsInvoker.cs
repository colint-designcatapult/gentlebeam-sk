using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.CollimatorConfigurations.V1;
using Com.Empyreanmed.Heracles.Enums.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcCollimatorConfigurationMethodsInvoker : AbstractRootGrpcInvoker<CollimatorConfiguration>
    {
        public GrpcCollimatorConfigurationMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new CollimatorConfigurationService.CollimatorConfigurationServiceClient(Channel);
        }

        public CollimatorConfigurationService.CollimatorConfigurationServiceClient Client { get; }

        public override async Task<CollimatorConfiguration> CreateAsync(CollimatorConfiguration entry)
        {
            var request = new CreateCollimatorConfigurationRequest { CollimatorConfiguration = entry };
            if (request.CollimatorConfiguration.HasId)
                request.CollimatorConfiguration.ClearId();

            var response = await CallWithOptions(Client.CreateCollimatorConfigurationAsync, request);
            return response.CollimatorConfiguration;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteCollimatorConfigurationAsync,
                new DeleteCollimatorConfigurationRequest { Id = entryId });
            return true;
        }

        public override async Task<ICollection<CollimatorConfiguration>> ReadAllAsync()
        {
            var response = await CallWithOptions(
                Client.ListCollimatorConfigurationsAsync,
                new ListCollimatorConfigurationsRequest());

            return response.CollimatorConfigurations;
        }

        public override async Task<CollimatorConfiguration> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetCollimatorConfigurationAsync,
                new GetCollimatorConfigurationRequest { Id = entryId });
            return response.CollimatorConfiguration;
        }

        public async Task<CollimatorConfiguration> SearchCollimatorConfigration(TARGETTYPE targetType, ENERGY energy, SSDTYPE ssdType)
        {
            var response = await CallWithOptions(
                Client.SearchCollimatorConfigurationAsync,
                new SearchCollimatorConfigurationRequest
                {
                    TargetType = targetType,
                    Energy = energy,
                    Ssd = ssdType
                });

            return response.CollimatorConfigurations;
        }

        public override async Task<CollimatorConfiguration> UpdateAsyncWithMask(CollimatorConfiguration entry, FieldMask mask)
        {
            var request = new UpdateCollimatorConfigurationRequest { CollimatorConfiguration = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateCollimatorConfigurationAsync, request);
            return response.CollimatorConfiguration;
        }
    }

}
