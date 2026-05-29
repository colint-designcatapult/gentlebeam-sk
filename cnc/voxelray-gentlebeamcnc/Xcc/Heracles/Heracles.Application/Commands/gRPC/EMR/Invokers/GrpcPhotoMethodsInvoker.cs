using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Photos.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Heracles.Core.Models.EMR;
using Xcc.Core.Infra.gRPC;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcPhotoMethodsInvoker : AbstractChildGrpcInvoker<Photo>
    {
        public GrpcPhotoMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new PhotosService.PhotosServiceClient(Channel);
        }

        public PhotosService.PhotosServiceClient Client { get; }

        public override async Task<Photo> CreateAsync(Photo entry)
        {
            var request = new CreatePhotoRequest { Photo = entry };
            request.Photo.ClearId();

            var response = await CallWithOptions(Client.CreatePhotoAsync, request);
            return response.Photo;
        }
        public override async Task<Photo> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetPhotoAsync,
                new GetPhotoRequest { PhotoId = entryId });
            return response.Photo;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeletePhotoAsync,
                new DeletePhotoRequest { PhotoId = entryId });
            return true;
        }

        public override async Task<Photo> UpdateAsyncWithMask(Photo entry, FieldMask mask)
        {
            var request = new UpdatePhotoRequest { Photo = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdatePhotoAsync, request);
            return response.Photo;
        }

        public override async Task<ICollection<Photo>> ReadListAsync(long diagnosisId)
        {
            var response = await CallWithOptions(
                Client.ListPhotosAsync,
                new ListPhotosRequest { DiagnosisId = diagnosisId });

            return response.Photos;
        }

        public IDataStreamReader<ReceivePhotoResponse> ReceivePhotoAsync(long id, CancellationToken cancellationToken)
        {
            var callOptions = new CallOptions().WithCancellationToken(cancellationToken).WithHeaders(GrpcSettings.Headers);
            var stream = Client.ReceivePhoto(
                new ReceivePhotoRequest{ PhotoId = id },
                callOptions);
            
            return new GrpcStreamReader<ReceivePhotoResponse>(stream, cancellationToken);
        }
        
        public async Task SendPhotoAsync(IPhoto photo, int chunkSize, CancellationToken token)
        {
            var callOptions = new CallOptions().WithCancellationToken(token).WithHeaders(GrpcSettings.Headers);
            using var response = Client.SendPhoto(callOptions);

            var toSend = photo.Data.Length;
            var totalChunks = toSend / chunkSize + (toSend % chunkSize == 0 ? 0 : 1);
            var pos = 0;
            var index = 0;

            while (toSend > 0)
            {
                var partToSend = Math.Min(toSend, chunkSize);
                var chunk = ByteString.CopyFrom(photo.Data, pos, partToSend);
                await response.RequestStream.WriteAsync(
                    new SendPhotoRequest { 
                        ChunkData = chunk,
                        ChunkIndex = index, 
                        TotalChunks = totalChunks,
                        PhotoId = photo.Id
                    }, token);

                pos += partToSend;
                toSend -= partToSend;
                ++index;
            }

            await response.RequestStream.CompleteAsync();
            //var result = await response.ResponseAsync;
            //throw new DataServiceException($"PhotoService.SendPhoto error: {result.Message}");
        }
    }

}
