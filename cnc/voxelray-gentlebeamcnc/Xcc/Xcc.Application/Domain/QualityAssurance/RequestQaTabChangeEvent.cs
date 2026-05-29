using Prism.Events;

namespace Xcc.Application.Domain.QualityAssurance;

public class RequestQaTabChangeEvent : PubSubEvent<QaTabName> {}