using Prism.Events;
using Xcc.Infra.UserSessions;

namespace Xcc.Application.AppLayer.UserSessions
{
    // The reason of this event is to deliver UserSession events to those views like MainWindow
    // that are instantiated before any singleton is registered in the module
    public class UserSessionEvent : PubSubEvent<UserSessionEventArgs>
    {
    }
}
