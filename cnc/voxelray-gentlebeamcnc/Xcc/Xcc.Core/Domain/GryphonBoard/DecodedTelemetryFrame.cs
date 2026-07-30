using System;

namespace Xcc.Core.Domain.GryphonBoard;

public readonly record struct DecodedTelemetryFrame(
    DateTimeOffset ReceivedAtUtc,
    ISystemTelemetry Telemetry,
    ReadOnlyMemory<byte> RawDatagram);

public interface IDecodedTelemetryFrameSink
{
    bool IsEnabled { get; }
    void Publish(DecodedTelemetryFrame frame);
}

public interface IDecodedTelemetryFrameSource
{
    IDisposable Subscribe(Action<DecodedTelemetryFrame> handler);
}
