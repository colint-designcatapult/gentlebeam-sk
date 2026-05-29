using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Collimators.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcCollimatorMethodsInvoker : AbstractChildGrpcInvoker<Collimator>
    {
        public GrpcCollimatorMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new CollimatorService.CollimatorServiceClient(Channel);
        }

        public CollimatorService.CollimatorServiceClient Client { get; private set; }

        public override async Task<Collimator> CreateAsync(Collimator entry)
        {
            var request = new CreateCollimatorRequest { Collimator = entry };
            if (request.Collimator.HasId)
                request.Collimator.ClearId();

            var response = await CallWithOptions(Client.CreateCollimatorAsync, request);
            return response.Collimator;
        }
        public override async Task<Collimator> ReadAsync(long serial)
        {
            var response = await CallWithOptions(
                Client.GetCollimatorAsync,
                new GetCollimatorRequest { Serial = serial.ToString() });
            return response.Collimator;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteCollimatorAsync,
                new DeleteCollimatorRequest { Id = entryId });
            return true;
        }

        public override async Task<Collimator> UpdateAsyncWithMask(Collimator entry, FieldMask mask)
        {
            var request = new UpdateCollimatorRequest { Collimator = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateCollimatorAsync, request);
            return response.Collimator;
        }

        public override async Task<ICollection<Collimator>> ReadListAsync(long configurationId)
        {
            var response = await CallWithOptions(
                Client.ListCollimatorsAsync,
                new ListCollimatorsRequest() { CollimatorConfigurationId = configurationId });

            return response.Collimators;
        }
    }

}
