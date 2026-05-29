using Heracles.Core.Models.EMR;
using Prism.Events;

namespace Heracles.Application.Models;

public class PlanStatusChangedEvent : PubSubEvent<IPlan>
{
}
