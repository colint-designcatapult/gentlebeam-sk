namespace Xcc.Infra.UserSessions
{
    public enum UserSessionEventType
    {
        Open = 1,
        Lock, // session is about to get locked
        Expiration, // session is about to expire
        Locked, // session was locked
        Unlocked, // session was unlocked
        Close  // session is about to close
    }
}
