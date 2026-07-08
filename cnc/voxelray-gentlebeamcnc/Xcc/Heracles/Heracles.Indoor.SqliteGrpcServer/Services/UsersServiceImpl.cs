using Com.Empyreanmed.Heracles.Users.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class UsersServiceImpl : UsersService.UsersServiceBase
{
    private readonly SqliteProtoRepository<User> _repo;
    public UsersServiceImpl(SqliteProtoRepository<User> repo) => _repo = repo;

    public override async Task<ListUsersResponse> ListUsers(ListUsersRequest request, ServerCallContext context)
    {
        var items = await _repo.ReadAllAsync();
        var r = new ListUsersResponse();
        r.Users.AddRange(items);
        return r;
    }

    public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var item = await _repo.ReadAsync(request.UserId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found"));
        return new GetUserResponse { User = item };
    }

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var created = await _repo.CreateAsync(request.User);
        return new CreateUserResponse { User = created };
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var updated = await _repo.UpdateAsync(request.User.Id, request.User);
        return new UpdateUserResponse { User = updated };
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        await _repo.DeleteAsync(request.UserId);
        return new DeleteUserResponse();
    }
}
