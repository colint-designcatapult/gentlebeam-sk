using Com.Empyreanmed.Heracles.SafetyChecks.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Heracles.Core.Models.RDBMS;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcSafetyCheckCommands 
        : RootEntryCommandWrapper<ISafetyCheck, SafetyCheck, GrpcSafetyCheckMethodsInvoker>
        , ISafetyCheckCommands
    {
        public GrpcSafetyCheckCommands(GrpcSafetyCheckMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
