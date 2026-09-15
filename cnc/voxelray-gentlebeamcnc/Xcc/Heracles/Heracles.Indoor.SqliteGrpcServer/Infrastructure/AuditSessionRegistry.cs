using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

public sealed class AuditSessionRegistry
{
    private readonly ConcurrentDictionary<string, (string Username, string UserId)> _sessions =
        new(StringComparer.Ordinal);

    public string Create(string username, string userId)
    {
        Span<byte> bytes = stackalloc byte[32];
        while (true)
        {
            RandomNumberGenerator.Fill(bytes);
            var token = Convert.ToHexString(bytes);
            if (_sessions.TryAdd(token, (username, userId)))
                return token;
        }
    }

    public bool TryResolve(string token, out string username, out string userId)
    {
        if (!string.IsNullOrEmpty(token) && _sessions.TryGetValue(token, out var session))
        {
            username = session.Username;
            userId = session.UserId;
            return true;
        }

        username = string.Empty;
        userId = string.Empty;
        return false;
    }
}
