using Com.Empyreanmed.Heracles.ActualTreatmentFields.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcActualTreatmentFieldCommands
        : ChildEntryCommandWrapper<IActualTreatmentField, ActualTreatmentField, GrpcActualTreatmentFieldMethodsInvoker>
        , IEmrActualTreatmentFieldCommands
    {
        public GrpcActualTreatmentFieldCommands(GrpcActualTreatmentFieldMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
