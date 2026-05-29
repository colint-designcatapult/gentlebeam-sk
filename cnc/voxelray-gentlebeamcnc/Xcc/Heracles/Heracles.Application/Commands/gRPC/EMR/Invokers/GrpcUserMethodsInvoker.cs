using Com.Empyreanmed.Heracles.Users.V1;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.EMR.Invokers
{
    public class GrpcUserMethodsInvoker : AbstractRootGrpcInvoker<User>
    {
        public GrpcUserMethodsInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
            Client = new UsersService.UsersServiceClient(Channel);
        }

        public UsersService.UsersServiceClient Client { get; private set; }

        public override async Task<User> CreateAsync(User entry)
        {
            var request = new CreateUserRequest { User = entry };
            request.User.ClearId();

            var response = await CallWithOptions(Client.CreateUserAsync, request);
            return response.User;
        }
        public override async Task<User> ReadAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.GetUserAsync,
                new GetUserRequest { UserId = entryId });
            return response.User;
        }

        public override async Task<bool> DeleteAsync(long entryId)
        {
            var response = await CallWithOptions(
                Client.DeleteUserAsync,
                new DeleteUserRequest { UserId = entryId });
            return true;
        }

        public override async Task<User> UpdateAsyncWithMask(User entry, FieldMask mask)
        {
            var request = new UpdateUserRequest { User = entry, UpdateMask = mask };
            var response = await CallWithOptions(Client.UpdateUserAsync, request);
            return response.User;
        }

        public override async Task<ICollection<User>> ReadAllAsync()
        {
            var response = await CallWithOptions(
                Client.ListUsersAsync,
                new ListUsersRequest());

            return response.Users;

        }
    }

}
