using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using Collimator = Com.Empyreanmed.Heracles.Collimators.V1.Collimator;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcCollimatorCommands 
        : ChildEntryCommandWrapper<ICollimator, Collimator, GrpcCollimatorMethodsInvoker>
        , ICollimatorCommands
    {
        public GrpcCollimatorCommands(GrpcCollimatorMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
