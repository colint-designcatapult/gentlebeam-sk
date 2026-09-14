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
        var user = await AuthenticateAsync(request.Username, request.Password);

        // Return a simple opaque bearer token.
        var token = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{user.Username}:{Guid.NewGuid()}"));

        return new LoginResponse { JwtToken = token };
    }

    public async Task<User> AuthenticateAsync(string username, string password)
    {
        var all = await _users.ReadAllAsync();
        var user = all.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user is null)
            throw new RpcException(new Status(StatusCode.Unauthenticated,
                $"User '{username}' not found"));

        if (user.Password != password)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "Invalid password"));

        user.LastAccessed = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
        await _users.UpdateAsync(user.Id, user, preserveOutputOnly: false);
        return user;
    }
}
