using Heracles.Ucsi.Models;

namespace Heracles.Ucsi.Services;

public sealed class TelemetryHistoryBuffer
{
    public const int DefaultCapacity = 30_000;

    private readonly object _sync = new();
    private readonly UcsiTelemetrySample[] _samples;
    private int _head;
    private int _count;

    public TelemetryHistoryBuffer() : this(DefaultCapacity)
    {
    }

    internal TelemetryHistoryBuffer(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _samples = new UcsiTelemetrySample[capacity];
    }

    public int Capacity => _samples.Length;

    public int Count
    {
        get
        {
            lock (_sync)
                return _count;
        }
    }

    public void Append(UcsiTelemetrySample sample)
    {
        lock (_sync)
        {
            if (_count > 0)
            {
                UcsiTelemetrySample latest = GetAtOffset(_count - 1);
                if (sample.LiveSequence <= latest.LiveSequence)
                    throw new ArgumentException("Live telemetry sequences must increase.", nameof(sample));
            }

            int destination = (_head + _count) % _samples.Length;
            _samples[destination] = sample;
            if (_count == _samples.Length)
                _head = (_head + 1) % _samples.Length;
            else
                _count++;
        }
    }

    public UcsiTelemetrySample? Latest()
    {
        lock (_sync)
            return _count == 0 ? null : GetAtOffset(_count - 1);
    }

    public IReadOnlyList<UcsiTelemetrySample> Snapshot()
    {
        lock (_sync)
        {
            var result = new UcsiTelemetrySample[_count];
            CopyTo(result, 0, 0, _count);
            return result;
        }
    }

    public IReadOnlyList<UcsiTelemetrySample> GetAfter(long sequence)
    {
        lock (_sync)
        {
            int first = FindFirstAfter(sequence);
            int resultCount = _count - first;
            if (resultCount <= 0)
                return Array.Empty<UcsiTelemetrySample>();

            var result = new UcsiTelemetrySample[resultCount];
            CopyTo(result, 0, first, resultCount);
            return result;
        }
    }

    private int FindFirstAfter(long sequence)
    {
        int low = 0;
        int high = _count;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            if (GetAtOffset(middle).LiveSequence <= sequence)
                low = middle + 1;
            else
                high = middle;
        }
        return low;
    }

    private void CopyTo(
        UcsiTelemetrySample[] destination,
        int destinationIndex,
        int sourceOffset,
        int count)
    {
        for (int index = 0; index < count; index++)
            destination[destinationIndex + index] = GetAtOffset(sourceOffset + index);
    }

    private UcsiTelemetrySample GetAtOffset(int offset) =>
        _samples[(_head + offset) % _samples.Length];
}
