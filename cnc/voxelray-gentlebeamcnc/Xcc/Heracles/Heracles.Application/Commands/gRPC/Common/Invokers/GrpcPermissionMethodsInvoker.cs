using System.Collections.Generic;
using System.Threading.Tasks;
using Com.Empyreanmed.Heracles.RolesPermissions.V1;
using Google.Protobuf.WellKnownTypes;
using Xcc.Infra.Networking.gRPC.Channels;
using Xcc.Infra.Persistence.DataAccess.gRPC.Invokers;

namespace Heracles.Application.Commands.gRPC.Common.Invokers;

public class GrpcPermissionMethodsInvoker : AbstractChildGrpcInvoker<RolesPermissions>
{
    public GrpcPermissionMethodsInvoker(IGrpcChannelManager grpcSettings)
        : base(grpcSettings)
    {
        Client = new RolesPermissionsService.RolesPermissionsServiceClient(Channel);
    }

    public RolesPermissionsService.RolesPermissionsServiceClient Client { get; private set; }

    public override async Task<RolesPermissions> CreateAsync(RolesPermissions entry)
    {
        var request = new CreateRolesPermissionsRequest { RolesPermissions = entry };
        request.RolesPermissions.ClearId();

        var response = await CallWithOptions(Client.CreateRolesPermissionsAsync, request);
        return response.RolesPermissions;
    }
    public override async Task<RolesPermissions> ReadAsync(long entryId)
    {
        var response = await CallWithOptions(
            Client.GetRolesPermissionsAsync,
            new GetRolesPermissionsRequest { Id = entryId });
        return response.RolesPermissions;
    }

    public override async Task<bool> DeleteAsync(long entryId)
    {
        var response = await CallWithOptions(
            Client.DeleteRolesPermissionsAsync,
            new DeleteRolesPermissionsRequest { Id = entryId });
        return true;
    }

    public override async Task<RolesPermissions> UpdateAsyncWithMask(RolesPermissions entry, FieldMask mask)
    {
        var request = new UpdateRolesPermissionsRequest() { RolesPermissions = entry, UpdateMask = mask };
        var response = await CallWithOptions(Client.UpdateRolesPermissionsAsync, request);
        return response.RolesPermissions;
    }

    public override async Task<ICollection<RolesPermissions>> ReadListAsync(long roleId)
    {
        var response = await CallWithOptions(
            Client.ListRolesPermissionsAsync,
            new ListRolesPermissionsRequest{RoleId = roleId});

        return response.RolesPermissions;

    }
}