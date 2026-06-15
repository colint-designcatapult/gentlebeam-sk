using Com.Empyreanmed.Heracles.Simulations.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcSimulationCommands 
        : ChildEntryCommandWrapper<ISimulation, Simulation, GrpcSimulationMethodsInvoker>
        , IEmrSimulationCommands
    {
        public GrpcSimulationCommands(GrpcSimulationMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
