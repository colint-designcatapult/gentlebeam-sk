using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.CorrectionMatrix.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcCorrectionMatrixMethodsInvoker : AbstractChildGrpcInvoker<CorrectionMatrix>
    {
        public GrpcCorrectionMatrixMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new CorrectionMatrixService.CorrectionMatrixServiceClient(Channel);
        }

        public CorrectionMatrixService.CorrectionMatrixServiceClient Client { get; private set; }

        public override async Task<CorrectionMatrix> CreateAsync(CorrectionMatrix entry)
        {
            var request = new CreateCorrectionMatrixRequest { CorrectionMatrix = entry };
            request.CorrectionMatrix.ClearId();

            var response = await CallWithOptions(Client.CreateCorrectionMatrixAsync, request);
            return response.CorrectionMatrix;
        }
        public override async Task<CorrectionMatrix> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetCorrectionMatrixAsync,
                new GetCorrectionMatrixRequest { Id = entryId });
            return response.CorrectionMatrix;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteCorrectionMatrixAsync,
                new DeleteCorrectionMatrixRequest { Id = entryId });
            return true;
        }

        public override async Task<CorrectionMatrix> UpdateAsyncWithMask(CorrectionMatrix entry, FieldMask mask)
        {
            var request = new UpdateCorrectionMatrixRequest { CorrectionMatrix = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateCorrectionMatrixAsync, request);
            return response.CorrectionMatrix;
        }

        public override async Task<ICollection<CorrectionMatrix>> ReadListAsync(long presetId)
        {
            var response = await CallWithOptions(
                Client.ListCorrectionMatricesAsync,
                new ListCorrectionMatricesRequest { PresetConfigurationId = presetId });

            return response.CorrectionMatrices.OrderBy(m => m.Id).ToList();
        }
    }

}
