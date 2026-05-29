using Com.Empyreanmed.Heracles.Series.V1;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcSeriesMethodsInvoker : AbstractChildGrpcInvoker<Series>
    {
        public GrpcSeriesMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new SeriesService.SeriesServiceClient(Channel);
        }

        public SeriesService.SeriesServiceClient Client { get; }

        public override async Task<Series> CreateAsync(Series entry)
        {
            var request = new CreateSeriesRequest { Series = entry };
            request.Series.ClearId();

            var response = await CallWithOptions(Client.CreateSeriesAsync, request);
            return response.Series;
        }
        public override async Task<Series> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetSeriesAsync,
                new GetSeriesRequest { SeriesId = entryId });
            return response.Series;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteSeriesAsync,
                new DeleteSeriesRequest { SeriesId = entryId });
            return true;
        }

        public override async Task<Series> UpdateAsyncWithMask(Series entry, FieldMask mask)
        {
            var request = new UpdateSeriesRequest { Series = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateSeriesAsync, request);
            return response.Series;
        }

        public override async Task<ICollection<Series>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListSeriesAsync,
                new ListSeriesRequest { DiagnosisId = parentId });

            return response.Series;
        }

        public async Task SendDicomDataAsync(int index, byte[] file, int chunkSize, long seriesId)
        {
            var ourCallOptions = GetCallOptions();
            var response = Client.SendDicom(ourCallOptions.Headers, ourCallOptions.Deadline, ourCallOptions.CancellationToken);

            int toSend = file.Length;
            int pos = 0;
            while (toSend > 0)
            {
                int partToSend = Math.Min(toSend, chunkSize);
                var chunk = ByteString.CopyFrom(file, pos, partToSend);
                await response.RequestStream.WriteAsync(
                    new SendDicomRequest { DicomFileData = chunk, FileIndex = index, SeriesId = seriesId });

                var R = response.ToString();
                pos += partToSend;
                toSend -= partToSend;
            }
        }

        public async Task SendDicomFilesAsync(string[] files, int chunkSize, long seriesId)
        {
            var ourCallOptions = GetCallOptions();
            var response = Client.SendDicom(ourCallOptions.Headers);
            int fileIndex = 0;

            foreach (var dicomFilePath in files)
            {
                byte[] dicomData = await File.ReadAllBytesAsync(dicomFilePath);

                int toSend = dicomData.Length;

                int pos = 0;
                while (toSend > 0)
                {
                    int partToSend = Math.Min(toSend, chunkSize);
                    var chunk = ByteString.CopyFrom(dicomData, pos, partToSend);
                    await response.RequestStream.WriteAsync(
                        new SendDicomRequest { DicomFileData = chunk, FileIndex = fileIndex, SeriesId = seriesId });

//                    var R = response.ToString();
                    pos += partToSend;
                    toSend -= partToSend;
                }

                fileIndex++;

            }

            await response.RequestStream.CompleteAsync();
            var dicomResponse = await response.ResponseAsync;

        }
    }

}
