using Com.Empyreanmed.Heracles.Treatments.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcTreatmentCommands
        : ChildEntryCommandWrapper<ITreatment, Treatment, GrpcTreatmentMethodsInvoker>
        , IEmrTreatmentCommands
    {
        public GrpcTreatmentCommands(GrpcTreatmentMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
