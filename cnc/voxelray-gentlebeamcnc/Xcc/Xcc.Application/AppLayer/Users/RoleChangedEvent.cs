using Prism.Events;
using Xcc.Core.Domain.DataManagement.Common.Users;

namespace Xcc.Application.AppLayer.Users;

public class RoleChangedEvent : PubSubEvent<RoleRecord> { }