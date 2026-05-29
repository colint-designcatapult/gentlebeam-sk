using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Intensities.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcIntensityMethodsInvoker : AbstractChildGrpcInvoker<Intensity>
    {
        public GrpcIntensityMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new IntensityService.IntensityServiceClient(Channel);
        }

        public IntensityService.IntensityServiceClient Client { get; private set; }

        public async override Task<Intensity> CreateAsync(Intensity entry)
        {
            var request = new CreateIntensityRequest { Intensity = entry };
            request.Intensity.ClearId();

            var response = await CallWithOptions(Client.CreateIntensityAsync, request);
            return response.Intensity;
        }

        public async override Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteIntensityAsync,
                new DeleteIntensityRequest { Id = entryId });
            return true;
        }

        public async override Task<Intensity> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetIntensityAsync,
                new GetIntensityRequest { Id = entryId });
            return response.Intensity;
        }

        public async override Task<ICollection<Intensity>> ReadListAsync(long qcSampleFieldsId)
        {
            var response = await CallWithOptions(
                Client.ListIntensitiesAsync,
                new ListIntensitiesRequest { QcsampleFieldsId = qcSampleFieldsId });

            return response.Intensities.Where(i => i.QcsampleFieldsId == qcSampleFieldsId).ToList(); // todo: temp filtering until gRPC does not return a correct list
        }

        public async override Task<Intensity> UpdateAsyncWithMask(Intensity entry, FieldMask mask)
        {
            var request = new UpdateIntensityRequest { Intensity = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateIntensityAsync, request);
            return response.Intensity;
        }
    }

}
