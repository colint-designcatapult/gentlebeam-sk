namespace Xcc.Core.Domain.GryphonBoard;

public readonly record struct TelemetryVector3(float X, float Y, float Z)
{
    public override string ToString() => $"[{X:F}, {Y:F}, {Z:F}]";
}
