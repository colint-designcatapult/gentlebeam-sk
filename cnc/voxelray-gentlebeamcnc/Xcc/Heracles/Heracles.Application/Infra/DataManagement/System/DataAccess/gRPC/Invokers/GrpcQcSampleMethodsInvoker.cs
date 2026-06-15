using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Qcsamples.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcQcSampleMethodsInvoker : AbstractChildGrpcInvoker<QCSample>
    {
        public GrpcQcSampleMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new QCSampleService.QCSampleServiceClient(Channel);
        }

        public QCSampleService.QCSampleServiceClient Client { get; private set; }

        public override async Task<QCSample> CreateAsync(QCSample entry)
        {
            var request = new CreateQCSampleRequest { Qcsample = entry };
            if (request.Qcsample.HasId)
                request.Qcsample.ClearId();

            var response = await CallWithOptions(Client.CreateQCSampleAsync, request);
            return response.Qcsample;
        }

        public override async Task<QCSample> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetQCSampleAsync,
                new GetQCSampleRequest { QcsampleId = entryId });
            return response.Qcsample;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteQCSampleAsync,
                new DeleteQCSampleRequest { QcsampleId = entryId });
            return true;
        }

        public override async Task<QCSample> UpdateAsyncWithMask(QCSample entry, FieldMask mask)
        {
            var request = new UpdateQCSampleRequest { Qcsample = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateQCSampleAsync, request);
            return response.Qcsample;
        }

        public override async Task<ICollection<QCSample>> ReadListAsync(long collimatorConfigurationId)
        {
            var response = await CallWithOptions(
                Client.ListQCSamplesAsync,
                new ListQCSamplesRequest { CollimatorConfigurationId = collimatorConfigurationId });

            return response.Qcsamples;
        }

        public async Task<QCSample> ApproveAsync(long entryId, string username, string password)
        {
            var response = await CallWithOptions(
                Client.ApproveQCSampleAsync,
                    new ApproveQCSampleRequest { QcsampleId = entryId, Username = username, Password = password });

            return response.ApprovedQcsample;
        }
    }

}
