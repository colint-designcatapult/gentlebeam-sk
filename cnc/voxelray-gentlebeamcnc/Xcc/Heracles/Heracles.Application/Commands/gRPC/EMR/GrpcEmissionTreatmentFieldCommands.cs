using Com.Empyreanmed.Heracles.EmissionTreatmentFields.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Application.Commands;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcEmissionTreatmentFieldCommands 
        : ChildEntryCommandWrapper<IEmissionTreatmentField, EmissionTreatmentField, GrpcEmissionTreatmentFieldMethodsInvoker>
        , IEmrEmissionTreatmentFieldCommands
    {
        public IDataServiceProvider DataServiceProvider { get; }

        public GrpcEmissionTreatmentFieldCommands(GrpcEmissionTreatmentFieldMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
