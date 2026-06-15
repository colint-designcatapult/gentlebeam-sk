using Com.Empyreanmed.Heracles.ReferenceFields.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Core.Domain.DataManagement.System;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcReferenceFieldCommands 
        : ChildEntryCommandWrapper<IReferenceFieldEntry, ReferenceField, GrpcReferenceFieldMethodsInvoker>
        , IReferenceFieldCommands
    {
        public GrpcReferenceFieldCommands(GrpcReferenceFieldMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
