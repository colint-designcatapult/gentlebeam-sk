using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using CollimatorConfiguration = Com.Empyreanmed.Heracles.CollimatorConfigurations.V1.CollimatorConfiguration;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcCollimatorConfigurationCommands
        : RootEntryCommandWrapper<ICollimatorConfiguration, CollimatorConfiguration, GrpcCollimatorConfigurationMethodsInvoker>
        , ICollimatorConfigurationCommands
    {
        public GrpcCollimatorConfigurationCommands(GrpcCollimatorConfigurationMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
