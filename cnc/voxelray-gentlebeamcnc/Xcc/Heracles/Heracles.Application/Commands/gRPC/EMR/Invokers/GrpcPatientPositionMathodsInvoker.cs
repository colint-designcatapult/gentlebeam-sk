using Com.Empyreanmed.Heracles.Positions.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcPatientPositionMethodsInvoker : AbstractChildGrpcInvoker<Position>
    {
        public GrpcPatientPositionMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new PositionService.PositionServiceClient(Channel);
        }

        public PositionService.PositionServiceClient Client { get; private set; }

        public override async Task<Position> CreateAsync(Position entry)
        {
            var request = new CreatePositionRequest { Position = entry };

            var response = await CallWithOptions(Client.CreatePositionAsync, request);
            return response.Position;
        }
        public override async Task<Position> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetPositionAsync,
                new GetPositionRequest { Id = entryId });
            return response.Position;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeletePositionAsync,
                new DeletePositionRequest { Id = entryId });
            return true;
        }

        public override async Task<Position> UpdateAsyncWithMask(Position entry, FieldMask mask)
        {
            var request = new UpdatePositionRequest { Position = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdatePositionAsync, request);
            return response.Position;
        }

        public override async Task<ICollection<Position>> ReadListAsync(long simulationId)
        {
            var response = await CallWithOptions(
                Client.ListPositionsAsync,
                new ListPositionsRequest { Id = simulationId });

            return response.Positions;
        }
    }
}
