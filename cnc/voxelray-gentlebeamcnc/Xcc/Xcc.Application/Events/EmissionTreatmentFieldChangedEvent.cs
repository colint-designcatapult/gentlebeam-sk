using Prism.Events;

namespace Xcc.Application.Events
{
    public class EmissionTreatmentFieldChangedEvent : PubSubEvent<EmissionTreatmentFieldChangedEventArgs>
    {
    }

    public class EmissionTreatmentFieldChangedEventArgs
    {
        public int OperationalPoint { get; set; }
        public float TimerValue { get; set; }
        public float Current { get; set; }
    }
}
