using Com.Empyreanmed.Heracles.HeaterCurrentConfigs.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcHeaterCurrentConfigCommands
        : ChildEntryCommandWrapper<IHeaterCurrentConfig, HeaterCurrentConfig, GrpcHeaterCurrentConfigMethodsInvoker>
        , IHeaterCurrentConfigCommands
    {
        public GrpcHeaterCurrentConfigCommands(GrpcHeaterCurrentConfigMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
