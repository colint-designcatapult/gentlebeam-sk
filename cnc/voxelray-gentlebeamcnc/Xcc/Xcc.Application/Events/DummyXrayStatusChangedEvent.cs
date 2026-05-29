using Prism.Events;
using Xcc.Core.Enums;

namespace Xcc.Application.Events
{
    public class DummyXrayStatusChangedEvent : PubSubEvent<DummyXrayStatusChangedEventArgs>
    {
    }

    public class DummyXrayStatusChangedEventArgs
    {
        public DummyXrayStatus Status { get; set; }

        public object Parameter { get; set; } = null;
    }
}
