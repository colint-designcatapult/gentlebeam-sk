using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.SafetyChecks.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Infra.DataManagement.System.DataAccess.gRPC.Invokers
{
    public class GrpcSafetyCheckMethodsInvoker : AbstractRootGrpcInvoker<SafetyCheck>
    {
        public GrpcSafetyCheckMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new SafetyCheckService.SafetyCheckServiceClient(Channel);
        }

        public SafetyCheckService.SafetyCheckServiceClient Client { get; private set; }

        public override async Task<SafetyCheck> CreateAsync(SafetyCheck entry)
        {
            var request = new CreateSafetyCheckRequest { SafetyCheck = entry };
            if (request.SafetyCheck.HasId)
                request.SafetyCheck.ClearId();

            var response = await CallWithOptions(Client.CreateSafetyCheckAsync, request);
            return response.SafetyCheck;
        }

        public override async Task<ICollection<SafetyCheck>> ReadAllAsync()
        {
            var response = await CallWithOptions(
                Client.ListSafetyChecksAsync,
                new ListSafetyChecksRequest { });

            return response.SafetyChecks;
        }

        public override async Task<SafetyCheck> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetSafetyCheckAsync,
                new GetSafetyCheckRequest { Id = entryId });
            return response.SafetyCheck;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteSafetyCheckAsync,
                new DeleteSafetyCheckRequest { Id = entryId });
            return true;
        }

        public override async Task<SafetyCheck> UpdateAsyncWithMask(SafetyCheck entry, FieldMask mask)
        {
            var request = new UpdateSafetyCheckRequest { SafetyCheck = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateSafetyCheckAsync, request);
            return response.SafetyCheck;
        }

    }

}
