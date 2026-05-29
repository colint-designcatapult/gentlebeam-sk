using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Heracles.Core.Models.RDBMS;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using CoilConfiguration = Com.Empyreanmed.Heracles.CoilConfigurations.V1.CoilConfiguration;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcCoilConfigurationCommands
        : ChildEntryCommandWrapper<ICoilConfigurationEntry, CoilConfiguration, GrpcCoilConfigurationMethodsInvoker>
        , ICoilConfigurationCommands
    {
        public GrpcCoilConfigurationCommands(GrpcCoilConfigurationMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
