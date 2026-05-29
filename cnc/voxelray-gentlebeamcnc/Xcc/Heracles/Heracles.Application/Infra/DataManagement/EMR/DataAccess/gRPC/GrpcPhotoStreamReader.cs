using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Heracles.Application.Commands.gRPC.EMR.Invokers;
using Heracles.Application.Models.RDBMS.EMR;
using Heracles.Core.Models.EMR;
using Xcc.Application.Helpers;
using Xcc.Core.Exceptions;

namespace Heracles.Application.Infra.DataManagement.EMR.DataAccess.gRPC
{
    public class GrpcPhotoStreamReader(
        GrpcPhotoMethodsInvoker invoker) : IPhotoStreamReader
    {
        public async Task<IPhoto?> ReceivePhotoAsync(IPhotoDescription photoDescription, CancellationToken token)
        {
            try
            {
                using var stream = invoker.ReceivePhotoAsync(photoDescription.Id, token);

                var data = new List<ByteString>();

                var totalChunks = 0;
                do
                {
                    var response = await stream.ReceiveAsync();
                    if (response.ChunkIndex != data.Count)
                        throw new Exception($"GrpcPhotoStreamReader: wrong chunk index. Expected = {data.Count}, received = {response.ChunkIndex}");

                    data.Add(response.ChunkData);

                    totalChunks = response.TotalChunks;

                } while (data.Count != totalChunks);

                token.ThrowIfCancellationRequested();

                return new Photo(photoDescription)
                {
                    Data = ByteArrayUtils.JoinByteArrays(data.Select(d => d.ToByteArray()))
                };
            }
            catch (OperationCanceledException operationCanceledException)
            {
                //empty
            }
            catch (Exception ex)
            {
                throw new DataServiceException("Photo receiving stream error.", ex);
            }

            return null;
        }
    }
}
