using Com.Empyreanmed.Heracles.QcsampleFields.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Heracles.Core.Models.RDBMS;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcQcSampleFieldCommands 
        : ChildEntryCommandWrapper<IQcSampleField, QCSampleField, GrpcQcSampleFieldMethodsInvoker>
        , IQcSampleFieldCommands
    {
        public GrpcQcSampleFieldCommands(GrpcQcSampleFieldMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
