using Com.Empyreanmed.Heracles.Roles.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class RoleServiceImpl : RoleService.RoleServiceBase
{
    private readonly SqliteProtoRepository<Role> _repo;
    public RoleServiceImpl(SqliteProtoRepository<Role> repo) => _repo = repo;

    public override async Task<ListRolesResponse> ListRoles(ListRolesRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListRolesResponse();
        r.Roles.AddRange(items);
        return r;
    }

    public override async Task<GetRoleResponse> GetRole(GetRoleRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"Role {request.Id} not found"));
        return new GetRoleResponse { Role = item };
    }

    public override async Task<CreateRoleResponse> CreateRole(CreateRoleRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.Role);
        return new CreateRoleResponse { Role = created };
    }

    public override async Task<UpdateRoleResponse> UpdateRole(UpdateRoleRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.Role.Id, request.Role);
        return new UpdateRoleResponse { Role = updated };
    }

    public override async Task<DeleteRoleResponse> DeleteRole(DeleteRoleRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteRoleResponse();
    }
}
