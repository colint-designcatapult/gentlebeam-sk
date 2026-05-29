using System;
using System.Threading;

namespace Xcc.Infra.UserSessions.BearerToken
{
    public class BearerTokenUserSession(
        string username,
        string bearerToken,
        DateTime expirationDate,
        CancellationToken? globalToken) : UserSession(username, expirationDate, globalToken)
    {
        public string AuthBearerToken => bearerToken;
        public bool IsLocked => string.IsNullOrEmpty(AuthBearerToken);
        public static BearerTokenUserSession NoSession { get; } = new BearerTokenUserSession(
            username: string.Empty, bearerToken: string.Empty, expirationDate: DateTime.Now, globalToken: null);

        public BearerTokenUserSession Lock()
        {
            // Create a new session with empty bearerToken and cancelled local token
            Close();
            return new BearerTokenUserSession(Username, string.Empty, ExpirationDate, GlobalCancellationToken);
        }

        public BearerTokenUserSession Expire()
        {
            // Create a new session with empty bearerToken and cancelled local token
            Close();
            return new BearerTokenUserSession(Username, string.Empty, DateTime.Now, GlobalCancellationToken);
        }


        public BearerTokenUserSession Unlock(string bearerToken, DateTime expirationDate)
        {
            // Create a new session with a new bearerToken and global cancellation token
            return new BearerTokenUserSession(Username, bearerToken, expirationDate, GlobalCancellationToken);
        }

        public new BearerTokenUserSession Close()
        {
            base.Close();
            return NoSession;
        }
    }

}
