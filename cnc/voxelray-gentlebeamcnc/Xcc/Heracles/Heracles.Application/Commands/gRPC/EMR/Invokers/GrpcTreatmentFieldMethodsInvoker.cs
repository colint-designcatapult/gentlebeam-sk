using Com.Empyreanmed.Heracles.TreatmentFields.V1;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcTreatmentFieldMethodsInvoker : AbstractChildGrpcInvoker<TreatmentField>
    {
        public GrpcTreatmentFieldMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new TreatmentFieldService.TreatmentFieldServiceClient(Channel);
        }

        public TreatmentFieldService.TreatmentFieldServiceClient Client { get; private set; }

        public override async Task<TreatmentField> CreateAsync(TreatmentField entry)
        {
            var request = new CreateTreatmentFieldRequest { TreatmentField = entry };
            request.TreatmentField.ClearId();

            var response = await CallWithOptions(Client.CreateTreatmentFieldAsync, request);
            return response.TreatmentField;
        }
        public override async Task<TreatmentField> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetTreatmentFieldAsync,
                new GetTreatmentFieldRequest { TreatmentFieldId = entryId });
            return response.TreatmentField;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteTreatmentFieldAsync,
                new DeleteTreatmentFieldRequest { TreatmentFieldId = entryId });
            return true;
        }

        public override async Task<TreatmentField> UpdateAsyncWithMask(TreatmentField entry, FieldMask mask)
        {
            var request = new UpdateTreatmentFieldRequest { TreatmentField = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateTreatmentFieldAsync, request);
            return response.TreatmentField;
        }

        public override async Task<ICollection<TreatmentField>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListTreatmentFieldsAsync,
                new ListTreatmentFieldsRequest { PlanId = parentId });

            return response.TreatmentFields;
        }
        public Task<ICollection<TreatmentField>> CreateBunchAsync(RepeatedField<TreatmentField> fields)
        {
            throw new NotImplementedException("Moses infrastructure not implemented: CreateBatchTreatmentFieldsRequest.TreatmentFields is read only");

            //var request = new CreateBatchTreatmentFieldsRequest { TreatmentFields = fields};
            //request.TreatmentField.ClearId();

            //var response = await CallWithOptions(Client.CreateTreatmentFieldAsync, request);
            //return response.TreatmentField;
        }
    }
}
