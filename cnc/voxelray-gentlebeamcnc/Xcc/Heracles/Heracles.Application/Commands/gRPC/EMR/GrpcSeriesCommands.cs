using Com.Empyreanmed.Heracles.Series.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using System.Threading.Tasks;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcSeriesCommands 
        : ChildEntryCommandWrapper<ISeries, Series, GrpcSeriesMethodsInvoker>
        , IEmrSeriesCommands
    {
        public GrpcSeriesCommands(GrpcSeriesMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
        }

        public Task SendDicomDataAsync(int index, byte[] file, int chunkSize, long seriesId)
        {
            return Invoker.SendDicomDataAsync(index, file, chunkSize, seriesId);
        }

        public Task SendDicomFilesAsync(string[] files, int chunkSize, long seriesId)
        {
            return Invoker.SendDicomFilesAsync(files, chunkSize, seriesId);
        }
    }
}
