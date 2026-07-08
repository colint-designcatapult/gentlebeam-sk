using Com.Empyreanmed.Heracles.RolesPermissions.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class RolesPermissionsServiceImpl : RolesPermissionsService.RolesPermissionsServiceBase
{
    private readonly SqliteProtoRepository<RolesPermissions> _repo;
    public RolesPermissionsServiceImpl(SqliteProtoRepository<RolesPermissions> repo) => _repo = repo;

    public override async Task<ListRolesPermissionsResponse> ListRolesPermissions(
        ListRolesPermissionsRequest request, ServerCallContext context)
    {
        IList<RolesPermissions> items;
        if (request.HasRoleId)
        {
            var all = await _repo.ReadAllAsync();
            items = all.Where(p => p.RoleId == request.RoleId).ToList();
        }
        else
        {
            items = await _repo.ReadAllAsync();
        }

        var r = new ListRolesPermissionsResponse();
        r.RolesPermissions.AddRange(items);
        return r;
    }

    public override async Task<GetRolesPermissionsResponse> GetRolesPermissions(
        GetRolesPermissionsRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"RolesPermissions {request.Id} not found"));
        return new GetRolesPermissionsResponse { RolesPermissions = item };
    }

    public override async Task<CreateRolesPermissionsResponse> CreateRolesPermissions(
        CreateRolesPermissionsRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.RolesPermissions);
        return new CreateRolesPermissionsResponse { RolesPermissions = created };
    }

    public override async Task<UpdateRolesPermissionsResponse> UpdateRolesPermissions(
        UpdateRolesPermissionsRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.RolesPermissions.Id, request.RolesPermissions);
        return new UpdateRolesPermissionsResponse { RolesPermissions = updated };
    }

    public override async Task<DeleteRolesPermissionsResponse> DeleteRolesPermissions(
        DeleteRolesPermissionsRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteRolesPermissionsResponse();
    }
}
