using Com.Empyreanmed.Heracles.Warmups.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Application.Commands;
using Xcc.Core.Models.RDBMS;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcWarmupCommands 
        : ChildEntryCommandWrapper<IWarmUp, Warmup, GrpcWarmupMethodsInvoker>
        , IWarmupCommands
    {
        public GrpcWarmupCommands(GrpcWarmupMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
