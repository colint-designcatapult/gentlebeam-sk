namespace Xcc.Core.Domain.GryphonBoard
{
    public class GcbOperationalEntry : IGcbOperationalDataPoint
    {
        public int PointIndex { get; set; }
        public float Duration { get; set; }
        public float ActualDuration { get; set; }
        public float Current { get; set; }
        public int Energy { get; set; }

        public float FilamentSetpoint { get; set; }
        public float FocusCoilSetpoint { get; set; }
        public float XCoilSetpoint { get; set; }
        public float YCoilSetpoint { get; set; }

        public GcbOperationalEntry()
        {
        }
    }
}
