using Com.Empyreanmed.Heracles.Visits.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcVisitCommands 
        : ChildEntryCommandWrapper<IVisit, Visit, GrpcVisitMethodsInvoker>
        , IEmrVisitCommands
    {
        public GrpcVisitCommands(GrpcVisitMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
