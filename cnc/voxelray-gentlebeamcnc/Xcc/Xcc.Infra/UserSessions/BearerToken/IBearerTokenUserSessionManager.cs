namespace Xcc.Infra.UserSessions.BearerToken
{
    public interface IBearerTokenUserSessionManager : INotifyUserSessionChanged
    {
        BearerTokenUserSession UserSession { get; }
        void StartUserSession(string username, string bearerToken);
        void CloseUserSession();
        void ExpireUserSession();
        void LockUserSession();
        void UnlockUserSession(string username, string bearerToken);
    }
}
