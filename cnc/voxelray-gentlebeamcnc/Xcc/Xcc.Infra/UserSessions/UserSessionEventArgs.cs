using System;

namespace Xcc.Infra.UserSessions
{
    public class UserSessionEventArgs(UserSession userSession, UserSessionEventType eventType) : EventArgs
    {
        public UserSession UserSession => userSession;
        public string Username => userSession.Username;
        public DateTime ExpirationDate => userSession.ExpirationDate;
        public UserSessionEventType EventType => eventType;
    }
}
