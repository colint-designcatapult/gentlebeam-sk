using Com.Empyreanmed.Heracles.Visits.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcVisitMethodsInvoker : AbstractChildGrpcInvoker<Visit>
    {
        public GrpcVisitMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new VisitsService.VisitsServiceClient(Channel);
        }

        public VisitsService.VisitsServiceClient Client { get; private set; }

        public override async Task<Visit> CreateAsync(Visit entry)
        {
            var request = new CreateVisitRequest { Visit = entry };
            request.Visit.ClearId();

            var response = await CallWithOptions(Client.CreateVisitAsync, request);
            return response.Visit;
        }
        public override async Task<Visit> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetVisitAsync,
                new GetVisitRequest { VisitId = entryId });
            return response.Visit;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteVisitAsync,
                new DeleteVisitRequest { VisitId = entryId });
            return true;
        }

        public override async Task<Visit> UpdateAsyncWithMask(Visit entry, FieldMask mask)
        {
            var request = new UpdateVisitRequest { Visit = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateVisitAsync, request);
            return response.Visit;
        }

        public override async Task<ICollection<Visit>> ReadListAsync(long parentId)
        {
            var response = await CallWithOptions(
                Client.ListVisitsAsync,
                new ListVisitsRequest { PatientId = parentId });

            return response.Visits;
        }
    }

}
