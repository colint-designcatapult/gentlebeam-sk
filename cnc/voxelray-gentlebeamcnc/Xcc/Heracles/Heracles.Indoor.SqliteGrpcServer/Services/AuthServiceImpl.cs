using Com.Empyreanmed.Heracles.Auth.V1;
using Com.Empyreanmed.Heracles.Users.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class AuthServiceImpl : AuthService.AuthServiceBase
{
    private readonly SqliteProtoRepository<User> _users;

    public AuthServiceImpl(SqliteProtoRepository<User> users) => _users = users;

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var all = await _users.ReadAllAsync();
        var user = all.FirstOrDefault(u =>
            u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));

        if (user is null)
            throw new RpcException(new Status(StatusCode.Unauthenticated,
                $"User '{request.Username}' not found"));

        // Plain-text password comparison (embedded server only).
        if (user.Password != request.Password)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid password"));

        // Return a simple opaque bearer token.
        var token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{user.Username}:{Guid.NewGuid()}"));

        return new LoginResponse { JwtToken = token };
    }
}
