using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.UserRoles.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.Common.Invokers;

public class GrpcUserRoleMethodsInvoker : AbstractChildGrpcInvoker<UserRole>
{
    public GrpcUserRoleMethodsInvoker(IGrpcChannelManager grpcSettings)
        : base(grpcSettings)
    {
        Client = new UserRoleService.UserRoleServiceClient(Channel);
    }

    public UserRoleService.UserRoleServiceClient Client { get; private set; }

    public override async Task<UserRole> CreateAsync(UserRole entry)
    {
        var request = new CreateUserRoleRequest { UserRole = entry };
        request.UserRole.ClearId();

        var response = await CallWithOptions(Client.CreateUserRoleAsync, request);
        return response.UserRole;
    }
    public override async Task<UserRole> ReadAsync(long entryId)
    {
        var response = await CallWithOptions(
            Client.GetUserRoleAsync,
            new GetUserRoleRequest { Id = entryId });
        return response.UserRole;
    }

    public override async Task<bool> DeleteAsync(long entryId)
    {
        var response = await CallWithOptions(
            Client.DeleteUserRoleAsync,
            new DeleteUserRoleRequest { Id = entryId });
        return true;
    }

    public override async Task<UserRole> UpdateAsyncWithMask(UserRole entry, FieldMask mask)
    {
        var request = new UpdateUserRoleRequest { UserRole = entry, UpdateMask = mask };
        var response = await CallWithOptions(Client.UpdateUserRoleAsync, request);
        return response.UserRole;
    }

    public override Task<ICollection<UserRole>> ReadListAsync(long userId)
    {
        throw new NotImplementedException("Protocol is not supported");
    }

    public async Task<ICollection<UserRole>> ReadListAsync(string userEmail)
    {
        var response = await CallWithOptions(
            Client.ListUserRolesAsync,
            new ListUserRolesRequest { UserId = userEmail });

        return response.UserRoles;
    }
}