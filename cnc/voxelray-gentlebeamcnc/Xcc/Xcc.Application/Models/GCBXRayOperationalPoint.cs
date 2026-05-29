namespace Xcc.Application.Models
{
    public class GCBXRayOperationalPoint
    {
        public int TreatmentPlanID { get; set; }
        public int OperationalPointCount { get; set; }
        public int OperationalPointIndex { get; set; }
        public float AccelerationPotentialTarget { get; set; } //[kv]
        public float XCoilTargetCurrent { get; set; } //[A]
        public float YCoilTargetCurrent { get; set; } //[A]
        public float LensTargetCurrent { get; set; } //[A]
        public float TargetTime { get; set; } //[min]
        public float HeaterTargetCurrent { get; set; } //[A]
    }
}
