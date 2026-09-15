using System.Globalization;
using System.Text.Json;
using Com.Empyreanmed.Heracles.Enums.V1;
using Com.Empyreanmed.Heracles.Logs.V1;
using Com.Empyreanmed.Heracles.Auth.V1;
using Com.Empyreanmed.Heracles.Users.V1;
using Grpc.Core;
using Heracles.Indoor.SqliteGrpcServer.Infrastructure;

namespace Heracles.Indoor.SqliteGrpcServer.Services;

public sealed class AuthServiceImpl : AuthService.AuthServiceBase
{
    private readonly SqliteProtoRepository<User> _users;
    private readonly AuditSessionRegistry _sessions;
    private readonly SqliteProtoRepository<Log> _logs;
    internal const uint FailedLoginLimit = 10;

    public AuthServiceImpl(
        SqliteProtoRepository<User> users, AuditSessionRegistry sessions, SqliteProtoRepository<Log> logs)
    {
        _users = users;
        _sessions = sessions;
        _logs = logs;
    }

    public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
    {
        var (_, token) = await AuthenticateCoreAsync(request.Username, request.Password, createSession: true);
        return new LoginResponse { JwtToken = token! };
    }

    public async Task<User> AuthenticateAsync(string username, string password)
    {
        var (user, _) = await AuthenticateCoreAsync(username, password, createSession: false);
        return user;
    }

    private async Task<(User User, string? Token)> AuthenticateCoreAsync(
        string username, string password, bool createSession)
    {
        User? actor = null;
        var outcome = "persistence_error";
        try
        {
            var user = await _users.UpdateFirstAsync(
                candidate => candidate.Username.Equals(username, StringComparison.OrdinalIgnoreCase),
                candidate =>
                {
                    actor = candidate;
                    if (candidate.FailedLoginAttempts >= FailedLoginLimit)
                    {
                        outcome = "locked";
                        return false;
                    }
                    if (candidate.Password != password)
                    {
                        outcome = "invalid_password";
                        if (createSession)
                            candidate.FailedLoginAttempts++;
                        return createSession;
                    }

                    outcome = "success";
                    if (createSession)
                        candidate.FailedLoginAttempts = 0;
                    candidate.LastAccessed = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
                    return true;
                });

            if (user is null)
                outcome = "unknown_user";
            if (outcome != "success")
            {
                var detail = outcome switch
                {
                    "locked" => "Account is locked",
                    "unknown_user" => "User not found",
                    _ => "Invalid password"
                };
                throw new RpcException(new Status(StatusCode.Unauthenticated, detail));
            }
        }
        catch (RpcException exception) when (exception.StatusCode == StatusCode.Unauthenticated)
        {
            throw;
        }
        catch
        {
            outcome = "persistence_error";
            // Repository exception messages can contain credentials or stored data.
            throw new RpcException(new Status(StatusCode.Internal, "Authentication could not be completed."));
        }
        finally
        {
            await RecordOutcomeAsync(createSession ? "Login" : "Authenticate", outcome, actor, username);
        }

        // No session is issued, and no approval is returned, until its audit is durable.
        var token = createSession
            ? _sessions.Create(actor!.Username, actor.Id.ToString(CultureInfo.InvariantCulture))
            : null;
        return (actor!, token);
    }

    private async Task RecordOutcomeAsync(string action, string outcome, User? user, string attemptedUsername)
    {
        try
        {
            await _logs.CreateAsync(new Log
            {
                Type = LOGTYPE.Security,
                Severity = outcome switch
                {
                    "success" => SEVERITY.Info,
                    "persistence_error" => SEVERITY.Error,
                    _ => SEVERITY.Warn
                },
                // Only allowlisted identity/outcome fields; JSON escapes user-supplied
                // newlines and delimiters without ever serializing credentials.
                Message = JsonSerializer.Serialize(new
                {
                    action,
                    outcome,
                    actor = user?.Username ?? attemptedUsername,
                    userId = user?.Id.ToString(CultureInfo.InvariantCulture)
                })
            });
        }
        catch
        {
            // Never expose storage details or allow an unaudited authentication.
            // Failed-login counter updates have already committed independently.
            throw new RpcException(new Status(StatusCode.Internal, "Authentication audit could not be recorded."));
        }
    }
}
