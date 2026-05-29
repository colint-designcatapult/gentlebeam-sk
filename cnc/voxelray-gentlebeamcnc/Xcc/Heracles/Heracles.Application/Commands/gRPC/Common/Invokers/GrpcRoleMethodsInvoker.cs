using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.Roles.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.Common.Invokers;

public class GrpcRoleMethodsInvoker : AbstractRootGrpcInvoker<Role>
{
    public GrpcRoleMethodsInvoker(IGrpcChannelManager grpcSettings)
        : base(grpcSettings)
    {
        Client = new RoleService.RoleServiceClient(Channel);
    }

    public RoleService.RoleServiceClient Client { get; private set; }

    public override async Task<Role> CreateAsync(Role entry)
    {
        var request = new CreateRoleRequest { Role = entry };
        request.Role.ClearId();

        var response = await CallWithOptions(Client.CreateRoleAsync, request);
        return response.Role;
    }
    public override async Task<Role> ReadAsync(long entryId)
    {
        var response = await CallWithOptions(
            Client.GetRoleAsync,
            new GetRoleRequest { Id = entryId });
        return response.Role;
    }

    public override async Task<bool> DeleteAsync(long entryId)
    {
        var response = await CallWithOptions(
            Client.DeleteRoleAsync,
            new DeleteRoleRequest { Id = entryId });
        return true;
    }

    public override async Task<Role> UpdateAsyncWithMask(Role entry, FieldMask mask)
    {
        var request = new UpdateRoleRequest { Role = entry, UpdateMask = mask };
        var response = await CallWithOptions(Client.UpdateRoleAsync, request);
        return response.Role;
    }

    public override async Task<ICollection<Role>> ReadAllAsync()
    {
        var response = await CallWithOptions(
            Client.ListRolesAsync,
            new ListRolesRequest());

        return response.Roles;

    }
}