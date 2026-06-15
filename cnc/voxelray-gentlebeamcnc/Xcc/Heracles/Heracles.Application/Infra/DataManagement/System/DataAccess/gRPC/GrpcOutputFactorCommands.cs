using Com.Empyreanmed.Heracles.OutputFactors.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Heracles.Core.Models;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcOutputFactorCommands 
        : ChildEntryCommandWrapper<IOutputFactor, OutputFactor, GrpcOutputFactorMethodsInvoker>
        , IOutputFactorCommands
    {
        public GrpcOutputFactorCommands(GrpcOutputFactorMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
