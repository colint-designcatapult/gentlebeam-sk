using Com.Empyreanmed.Heracles.Intensities.V1;
using Heracles.Application.Infra.DataManagement.System.DataAccess;
using Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers;
using Xcc.Core.Domain.DataManagement.System.QualityAssurance;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC
{
    public class GrpcIntensityCommands
        : ChildEntryCommandWrapper<IIntensity, Intensity, GrpcIntensityMethodsInvoker>
        , IIntensityCommands
    {
        public GrpcIntensityCommands(GrpcIntensityMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }
    }
}
