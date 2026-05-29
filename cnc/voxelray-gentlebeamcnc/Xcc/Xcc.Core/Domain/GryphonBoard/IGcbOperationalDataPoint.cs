namespace Xcc.Core.Domain.GryphonBoard
{
    /// <summary>
    /// Abstract (Plan, QC etc.) data to be loaded to GCB
    /// </summary>
    public interface IGcbOperationalDataPoint
    {
        int PointIndex { get; set; }
        float Duration { get; set; }
        float ActualDuration { get; set; }
        float Current { get; set; }
        int Energy { get; set; }

        float FilamentSetpoint { get; set; }
        float FocusCoilSetpoint { get; set; }
        float XCoilSetpoint { get; set; }
        float YCoilSetpoint { get; set; }
    }
}
