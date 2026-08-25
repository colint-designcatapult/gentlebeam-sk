using System;
using System.Diagnostics;
using System.Threading;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Application.Models;

public sealed class DecodedTelemetryFrameHub : IDecodedTelemetryFrameSink, IDecodedTelemetryFrameSource
{
    private Action<DecodedTelemetryFrame>[] _handlers = [];

    public bool IsEnabled => Volatile.Read(ref _handlers).Length != 0;

    public IDisposable Subscribe(Action<DecodedTelemetryFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        while (true)
        {
            Action<DecodedTelemetryFrame>[] current = Volatile.Read(ref _handlers);
            var updated = new Action<DecodedTelemetryFrame>[current.Length + 1];
            Array.Copy(current, updated, current.Length);
            updated[^1] = handler;
            if (ReferenceEquals(Interlocked.CompareExchange(ref _handlers, updated, current), current))
                return new Subscription(this, handler);
        }
    }

    public void Publish(DecodedTelemetryFrame frame)
    {
        Action<DecodedTelemetryFrame>[] handlers = Volatile.Read(ref _handlers);
        foreach (Action<DecodedTelemetryFrame> handler in handlers)
        {
            try
            {
                handler(frame);
            }
            catch (Exception exception)
            {
                if (Remove(handler))
                    Trace.TraceError($"Decoded telemetry subscriber removed after an exception: {exception}");
            }
        }
    }

    private bool Remove(Action<DecodedTelemetryFrame> handler)
    {
        while (true)
        {
            Action<DecodedTelemetryFrame>[] current = Volatile.Read(ref _handlers);
            int index = Array.IndexOf(current, handler);
            if (index < 0)
                return false;

            if (current.Length == 1)
            {
                if (ReferenceEquals(Interlocked.CompareExchange(ref _handlers, [], current), current))
                    return true;
                continue;
            }

            var updated = new Action<DecodedTelemetryFrame>[current.Length - 1];
            if (index > 0)
                Array.Copy(current, 0, updated, 0, index);
            if (index < current.Length - 1)
                Array.Copy(current, index + 1, updated, index, current.Length - index - 1);
            if (ReferenceEquals(Interlocked.CompareExchange(ref _handlers, updated, current), current))
                return true;
        }
    }

    private sealed class Subscription(
        DecodedTelemetryFrameHub owner,
        Action<DecodedTelemetryFrame> handler) : IDisposable
    {
        private DecodedTelemetryFrameHub? _owner = owner;

        public void Dispose() => Interlocked.Exchange(ref _owner, null)?.Remove(handler);
    }
}
