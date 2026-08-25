using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.Ucsi.Models;

public enum UcsiMode
{
    Live,
    Replay,
}

public enum SessionTransportState
{
    Idle,
    Recording,
    Loading,
    Paused,
    Playing,
}

public enum TelemetrySourceKind
{
    Udp,
    Dummy,
}

public enum TelemetryValueKind
{
    Numeric,
    Boolean,
    Enum,
    String,
    Identifier,
}

public readonly record struct UcsiTelemetrySample(
    long LiveSequence,
    DateTimeOffset ReceivedAtUtc,
    long LiveElapsedTicks,
    ISystemTelemetry Telemetry,
    IReadOnlyList<FaultEntry> ActiveFaults);

internal readonly record struct RecordingEnvelope(
    UcsiTelemetrySample Sample,
    ReadOnlyMemory<byte> RawDatagram,
    long ElapsedTicks,
    TelemetrySourceKind SourceKind);

public readonly record struct ReplayFrame(
    long RowIndex,
    long ElapsedTicks,
    UcsiTelemetrySample Sample);

public readonly record struct TelemetryGraphPoint(
    long ElapsedTicks,
    double? Value);

public sealed record ReplayGraphSeries(
    string ParameterId,
    IReadOnlyList<TelemetryGraphPoint> Points);
