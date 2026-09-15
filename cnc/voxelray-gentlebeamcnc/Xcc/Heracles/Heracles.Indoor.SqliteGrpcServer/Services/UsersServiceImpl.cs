using System.Globalization;
using System.Text.Json;
using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.Logs.V1;
using Com.Empyreanmed.Heracles.Roles.V1;
using Com.Empyreanmed.Heracles.UserRoles.V1;
using Com.Empyreanmed.Heracles.Users.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class UsersServiceImpl : UsersService.UsersServiceBase
{
    private readonly SqliteProtoRepository<User> _repo;
    private readonly SqlCipherConnectionFactory _connections;
    private readonly AuditSessionRegistry _sessions;
    private readonly SqliteProtoRepository<Log> _logs;
    private readonly SqliteProtoRepository<UserRole> _userRoles;
    private readonly SqliteProtoRepository<Role> _roles;

    public UsersServiceImpl(
        SqliteProtoRepository<User> repo, SqlCipherConnectionFactory connections,
        AuditSessionRegistry sessions, SqliteProtoRepository<Log> logs,
        SqliteProtoRepository<UserRole> userRoles, SqliteProtoRepository<Role> roles)
    {
        _repo = repo;
        _connections = connections;
        _sessions = sessions;
        _logs = logs;
        _userRoles = userRoles;
        _roles = roles;
    }

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

    public override async Task<ResetUserLockoutResponse> ResetUserLockout(
        ResetUserLockoutRequest request, ServerCallContext context)
    {
        var authorization = context.RequestHeaders.GetValue("authorization");
        if (authorization is null ||
            !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ||
            !_sessions.TryResolve(authorization[7..], out _, out var userId) ||
            !long.TryParse(userId, NumberStyles.None, CultureInfo.InvariantCulture, out var actorId) ||
            actorId <= 0)
            throw new RpcException(new Status(StatusCode.Unauthenticated, "An authenticated administrator is required."));

        try
        {
            await using var connection = await _connections.OpenAsync(context.CancellationToken);
            // Authorize against current persisted state under the same write lock
            // as the reset, so demotion, deletion or lockout cannot race this check.
            using var transaction = connection.BeginTransaction(deferred: false);
            var actor = await _repo.ReadAsync(actorId, transaction)
                ?? throw new RpcException(new Status(StatusCode.Unauthenticated, "The authenticated account no longer exists."));
            // Application roles come from the email-keyed mapping, not User.Role.
            var mapping = await _userRoles.ReadFirstAsync(
                candidate => candidate.UserId == actor.EmailAddress, transaction);
            var role = mapping is null ? null : await _roles.ReadAsync(mapping.RoleId, transaction);
            if (role?.RoleName != Xcc.Core.Domain.DataManagement.Common.Users.UserRole.BuiltInNames.Administrator ||
                actor.FailedLoginAttempts >= AuthServiceImpl.FailedLoginLimit)
                throw new RpcException(new Status(StatusCode.PermissionDenied, "An unlocked administrator account is required."));

            var target = await _repo.ReadAsync(request.UserId, transaction)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found"));
            if (target.FailedLoginAttempts >= AuthServiceImpl.FailedLoginLimit)
            {
                target.FailedLoginAttempts = 0;
                target = await _repo.UpdateAsync(target.Id, target, transaction, preserveOutputOnly: false);
                await _logs.CreateAsync(new Log
                {
                    Type = LOGTYPE.Security,
                    Severity = SEVERITY.Info,
                    Message = JsonSerializer.Serialize(new
                    {
                        action = "ResetUserLockout",
                        outcome = "success",
                        actor = actor.Username,
                        userId = actor.Id.ToString(CultureInfo.InvariantCulture),
                        targetUserId = target.Id.ToString(CultureInfo.InvariantCulture)
                    })
                }, transaction);
            }

            // The success entry and reset become visible together, or both roll back.
            transaction.Commit();
            return new ResetUserLockoutResponse { User = target };
        }
        catch (RpcException)
        {
            throw;
        }
        catch
        {
            throw new RpcException(new Status(StatusCode.Internal, "User lockout could not be reset."));
        }
    }
}
