using Com.Empyreanmed.Heracles.Simulations.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcSimulationMethodsInvoker : AbstractChildGrpcInvoker<Simulation>
    {
        public GrpcSimulationMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new SimulationService.SimulationServiceClient(Channel);
        }

        public SimulationService.SimulationServiceClient Client { get; private set; }

        public override async Task<Simulation> CreateAsync(Simulation entry)
        {
            var request = new CreateSimulationRequest { Simulation = entry };
            request.Simulation.ClearId();

            var response = await CallWithOptions(Client.CreateSimulationAsync, request);
            return response.Simulation;
        }
        public override async Task<Simulation> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetSimulationAsync,
                new GetSimulationRequest { SimulationId = entryId });
            return response.Simulation;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteSimulationAsync,
                new DeleteSimulationRequest { SimulationId = entryId });
            return true;
        }

        public override async Task<Simulation> UpdateAsyncWithMask(Simulation entry, FieldMask mask)
        {
            var request = new UpdateSimulationRequest { Simulation = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateSimulationAsync, request);
            return response.Simulation;
        }

        public override async Task<ICollection<Simulation>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListSimulationsAsync,
                new ListSimulationsRequest { DiagnosisId = parentId });

            return response.Simulations;
        }
    }
}
