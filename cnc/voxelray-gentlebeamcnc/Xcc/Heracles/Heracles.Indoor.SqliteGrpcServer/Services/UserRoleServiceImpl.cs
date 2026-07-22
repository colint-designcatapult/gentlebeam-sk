using Com.Empyreanmed.Heracles.UserRoles.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class UserRoleServiceImpl : UserRoleService.UserRoleServiceBase
{
    private readonly SqliteProtoRepository<UserRole> _repo;
    public UserRoleServiceImpl(SqliteProtoRepository<UserRole> repo) => _repo = repo;

    public override async Task<ListUserRolesResponse> ListUserRoles(ListUserRolesRequest request, ServerCallContext context)
    {
        IList<UserRole> items;
        if (request.HasUserId && !string.IsNullOrEmpty(request.UserId))
        {
            var all = await _repo.ReadAllAsync();
            items = all.Where(u => u.UserId == request.UserId).ToList();
        }
        else
        {
            items = await _repo.ReadAllAsync();
        }

        var r = new ListUserRolesResponse();
        r.UserRoles.AddRange(items);
        return r;
    }

    public override async Task<GetUserRoleResponse> GetUserRole(GetUserRoleRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.Id)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"UserRole {request.Id} not found"));
        return new GetUserRoleResponse { UserRole = item };
    }

    public override async Task<CreateUserRoleResponse> CreateUserRole(CreateUserRoleRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.UserRole);
        return new CreateUserRoleResponse { UserRole = created };
    }

    public override async Task<UpdateUserRoleResponse> UpdateUserRole(UpdateUserRoleRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.UserRole.Id, request.UserRole);
        return new UpdateUserRoleResponse { UserRole = updated };
    }

    public override async Task<DeleteUserRoleResponse> DeleteUserRole(DeleteUserRoleRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.Id);
        return new DeleteUserRoleResponse();
    }
}
