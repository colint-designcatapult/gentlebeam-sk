using System.Threading;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Photos.V1;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Application.Infra.DataManagement.EMR.DataAccess.gRPC;
using Heracles.Core.Commands;
using Heracles.Core.Models.EMR;
using Xcc.Infra.Persistence.DataAccess.gRPC;
using ProtoTypesConverter = Heracles.Application.Protos.ProtoTypesConverter;

namespace Heracles.Application.Commands.gRPC.EMR
{
    public class GrpcPhotoCommands 
        : ChildEntryCommandWrapper<IPhotoDescription, Photo, GrpcPhotoMethodsInvoker>
        , IEmrPhotoCommands
    {

        public GrpcPhotoCommands(GrpcPhotoMethodsInvoker invoker)
            : base(invoker, ProtoTypesConverter.ToProto, ProtoTypesConverter.FromProto)
        {
            _streamReader = new GrpcPhotoStreamReader(invoker);
        }

        public Task SendPhotoAsync(IPhoto photo, int chunkSize, CancellationToken token)
        {
            return Invoker.SendPhotoAsync(photo, chunkSize, token);
        }

        public Task<IPhoto?> ReceivePhotoAsync(IPhotoDescription photoDescription, CancellationToken token)
        {
            return _streamReader.ReceivePhotoAsync(photoDescription, token);
        }

        private readonly GrpcPhotoStreamReader _streamReader;
    }
}
