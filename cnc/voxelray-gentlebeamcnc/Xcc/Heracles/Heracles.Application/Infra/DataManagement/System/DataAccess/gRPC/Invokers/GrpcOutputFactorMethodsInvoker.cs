using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.OutputFactors.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcOutputFactorMethodsInvoker : AbstractChildGrpcInvoker<OutputFactor>
    {
        public GrpcOutputFactorMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new OutputFactorService.OutputFactorServiceClient(Channel);
        }

        public OutputFactorService.OutputFactorServiceClient Client { get; private set; }

        public override async Task<OutputFactor> CreateAsync(OutputFactor entry)
        {
            var request = new CreateOutputFactorRequest { OutputFactor = entry };
            request.OutputFactor.ClearId();

            var response = await CallWithOptions(Client.CreateOutputFactorAsync, request);
            return response.OutputFactor;
        }
        public override async Task<OutputFactor> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetOutputFactorAsync,
                new GetOutputFactorRequest { Id = entryId });
            return response.OutputFactor;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteOutputFactorAsync,
                new DeleteOutputFactorRequest { Id = entryId });
            return true;
        }

        public override async Task<OutputFactor> UpdateAsyncWithMask(OutputFactor entry, FieldMask mask)
        {
            var request = new UpdateOutputFactorRequest { OutputFactor = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateOutputFactorAsync, request);
            return response.OutputFactor;
        }

        public override async Task<ICollection<OutputFactor>> ReadListAsync(long presetId)
        {
            var response = await CallWithOptions(
                Client.ListOutputFactorsAsync,
                new ListOutputFactorsRequest { PresetConfigurationId = presetId });

            return response.OutputFactors.OrderBy(o => o.Id).ToList();
        }
    }

}
