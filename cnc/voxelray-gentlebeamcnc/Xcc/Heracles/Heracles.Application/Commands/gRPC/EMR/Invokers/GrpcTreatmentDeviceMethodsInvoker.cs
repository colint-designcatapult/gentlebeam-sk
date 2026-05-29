using Com.Empyreanmed.Heracles.TreatmentDevices.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcTreatmentDeviceMethodsInvoker : AbstractChildGrpcInvoker<TreatmentDevice>
    {
        public GrpcTreatmentDeviceMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new TreatmentDevicesService.TreatmentDevicesServiceClient(Channel);
        }

        public TreatmentDevicesService.TreatmentDevicesServiceClient Client { get; private set; }

        public override async Task<TreatmentDevice> CreateAsync(TreatmentDevice entry)
        {
            var request = new CreateTreatmentDeviceRequest { TreatmentDevice = entry };
            request.TreatmentDevice.ClearId();

            var response = await CallWithOptions(Client.CreateTreatmentDeviceAsync, request);
            return response.TreatmentDevice;
        }
        public override async Task<TreatmentDevice> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetTreatmentDeviceAsync,
                new GetTreatmentDeviceRequest { TreatmentDeviceId = entryId });
            return response.TreatmentDevice;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteTreatmentDeviceAsync,
                new DeleteTreatmentDeviceRequest { TreatmentDeviceId = entryId });
            return true;
        }

        public override async Task<TreatmentDevice> UpdateAsyncWithMask(TreatmentDevice entry, FieldMask mask)
        {
            var request = new UpdateTreatmentDeviceRequest { TreatmentDevice = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateTreatmentDeviceAsync, request);
            return response.TreatmentDevice;
        }

        public override async Task<ICollection<TreatmentDevice>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListTreatmentDevicesAsync,
                new ListTreatmentDevicesRequest { SimulationId = parentId });

            return response.TreatmentDevices;
        }
    }
}
