using Com.Empyreanmed.Heracles.Diagnoses.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcDiagnosisCommands 
        : ChildEntryCommandWrapper<IDiagnosis, Diagnosis, GrpcDiagnosisMethodsInvoker>
        , IEmrDiagnosisCommands
    {
        public GrpcDiagnosisCommands(GrpcDiagnosisMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
