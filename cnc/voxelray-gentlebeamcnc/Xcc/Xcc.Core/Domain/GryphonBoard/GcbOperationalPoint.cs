using System;
using Xcc.Core.Common;

namespace Xcc.Core.Domain.GryphonBoard
{
    public struct GcbOperationalPoint
    {
        /// <summary>
        /// The index of the point within the treatment plan
        /// </summary>
        public int PointIndex { get; set; }

        /// <summary>
        /// [sec] The total execution time of the point
        /// </summary>
        public float TotalPointTime { get; set; }

        /// <summary>
        /// [sec] The remaining execution time of the point at the moment of ReleasePoint command
        /// </summary>
        public float InitialRemainingPointTime { get; set; }

        /// <summary>
        /// [sec] The remaining execution time of the point
        /// </summary>
        public float RemainingPointTime { get; set; }

        /// <summary>
        /// [kV]
        /// </summary>
        public float SetpointKv { get; set; }

        /// <summary>
        /// [mA]
        /// </summary>
        public float TargetMA { get; set; }

        /// <summary>
        /// [mA] The filament current for the point
        /// </summary>
        public float FilamentSetpoint { get; set; }

        /// <summary>
        /// [mA] The x coil current for the point
        /// </summary>
        public float XCoilSetpoint { get; set; }

        /// <summary>
        /// [mA] The y coil current for the point
        /// </summary>
        public float YCoilSetpoint { get; set; }

        /// <summary>
        /// [mA] The focus coil current for the point
        /// </summary>
        public float FocusCoilSetpoint { get; set; }

        /// <summary>
        /// Specifies whether the point will be executed automatically or will require a 'Beam On' button press to execute
        /// The first point in the sequence always does NOT auto-execute even if this flag is true
        /// </summary>
        public bool AutoExecution { get; set; }

        public DeflectionCurrentCorrection CoilSetpointCorrection { get; private set; }

        public float ActualDuration { get => TotalPointTime - RemainingPointTime; }

        public bool Equals(GcbOperationalPoint other)
        {
            return PointIndex == other.PointIndex &&
                TotalPointTime == other.TotalPointTime &&
                Math.Abs(RemainingPointTime - other.RemainingPointTime) < 0.001 &&
                SetpointKv == other.SetpointKv &&
                TargetMA == other.TargetMA &&
                FilamentSetpoint == other.FilamentSetpoint &&
                XCoilSetpoint == other.XCoilSetpoint &&
                YCoilSetpoint == other.YCoilSetpoint &&
                FocusCoilSetpoint == other.FocusCoilSetpoint &&
                AutoExecution == other.AutoExecution;
        }

        public bool IsSamePoint(GcbOperationalPoint other)
        {
            return PointIndex == other.PointIndex &&
                TotalPointTime == other.TotalPointTime &&
                SetpointKv == other.SetpointKv &&
                TargetMA == other.TargetMA &&
                FilamentSetpoint == other.FilamentSetpoint &&
                // We can't compare points by deflection current, as we rely on magnetometric correction now:
                //XCoilSetpoint == other.XCoilSetpoint &&
                //YCoilSetpoint == other.YCoilSetpoint &&
                FocusCoilSetpoint == other.FocusCoilSetpoint &&
                AutoExecution == other.AutoExecution;
        }

        public GcbOperationalPoint WithSetpointCorrection(DeflectionCurrentCorrection correction)
        {
            var output = this;
            output.XCoilSetpoint = (float)correction.CorrectX(this.XCoilSetpoint);
            output.YCoilSetpoint = (float)correction.CorrectY(this.YCoilSetpoint);
            output.CoilSetpointCorrection = correction;
            return output;
        }
    }
}
