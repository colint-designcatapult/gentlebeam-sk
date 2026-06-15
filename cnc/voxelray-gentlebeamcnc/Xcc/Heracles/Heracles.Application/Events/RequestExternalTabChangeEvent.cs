using Heracles.Application.Enums;
using Prism.Events;

namespace Heracles.Application.Events;

public class RequestExternalTabChangeEvent : PubSubEvent<ExternalTabName> {}