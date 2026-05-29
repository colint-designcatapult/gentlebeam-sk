using System;

namespace Xcc.Infra.UserSessions
{
    public interface INotifyUserSessionChanged
    {
        event EventHandler<UserSessionEventArgs>? UserSessionChanged;
    }
}
