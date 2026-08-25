using System.IO;
using Heracles.Ucsi.Models;
using Parquet;
using Parquet.Schema;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Ucsi.Storage;


public sealed class ParquetTelemetrySessionReader(
    TelemetryParameterCatalog catalog)
{
    internal async Task<ParquetReplaySession> LoadAsync(string path, CancellationToken cancellationToken)
    {
        string fullPath = Path.GetFullPath(path);
        if (!string.Equals(Path.GetExtension(fullPath), ".parquet", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Select a completed .parquet telemetry session.");

        ParquetReader? reader = null;
        try
        {
            reader = await ParquetReader.CreateAsync(
                fullPath,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            ValidateMetadata(reader);
            Dictionary<string, DataField> fields = ValidateSchema(reader);
            IReadOnlyList<ReplayRowGroupIndex> index = await BuildIndexAsync(
                reader,
                fields,
                cancellationToken).ConfigureAwait(false);
            if (index.Count == 0 || index[^1].LastRowIndex < 0)
                throw new InvalidDataException("The telemetry session contains no samples.");

            var session = new ParquetReplaySession(fullPath, reader, fields, index, catalog);
            reader = null;
            return session;
        }
        catch
        {
            if (reader is not null)
                await reader.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private void ValidateMetadata(ParquetReader reader)
    {
        if (reader.CustomMetadata is null
            || !reader.CustomMetadata.TryGetValue("ucsi.schema.version", out string? schemaVersion)
            || !string.Equals(schemaVersion, ParquetTelemetrySessionWriter.SchemaVersion, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Unsupported or missing UCSI telemetry schema version.");
        }
    }

    private Dictionary<string, DataField> ValidateSchema(ParquetReader reader)
    {
        var expected = new List<DataField>
        {
            new DataField<long>("Sequence", false),
            new DataField<long>("ReceivedAtUtcTicks", false),
            new DataField<long>("ElapsedTicks", false),
            new DataField<string>("SourceKind", false),
            new DataField<byte[]>("RawDatagram", true),
        };
        expected.AddRange(catalog.All.Select(descriptor => descriptor.CreateParquetField()));

        DataField[] actualFields = reader.Schema.GetDataFields();
        var actual = actualFields.ToDictionary(field => field.Name, StringComparer.Ordinal);
        if (actual.Count != expected.Count)
            throw new InvalidDataException("The telemetry schema column count does not match version 1.");

        foreach (DataField expectedField in expected)
        {
            if (!actual.TryGetValue(expectedField.Name, out DataField? actualField))
                throw new InvalidDataException($"The telemetry session is missing column '{expectedField.Name}'.");
            if (actualField.ClrType != expectedField.ClrType || actualField.IsNullable != expectedField.IsNullable)
            {
                throw new InvalidDataException(
                    $"Column '{expectedField.Name}' has an incompatible type or nullability.");
            }
        }
        return actual;
    }

    private static async Task<IReadOnlyList<ReplayRowGroupIndex>> BuildIndexAsync(
        ParquetReader reader,
        IReadOnlyDictionary<string, DataField> fields,
        CancellationToken cancellationToken)
    {
        var index = new List<ReplayRowGroupIndex>(reader.RowGroupCount);
        long expectedSequence = 0;
        long previousElapsedTicks = long.MinValue;
        for (int groupIndex = 0; groupIndex < reader.RowGroupCount; groupIndex++)
        {
            using ParquetRowGroupReader group = reader.OpenRowGroupReader(groupIndex);
            int count = checked((int)group.RowCount);
            if (count <= 0)
                throw new InvalidDataException($"Telemetry row group {groupIndex} is empty.");

            var sequences = new long[count];
            var elapsed = new long[count];
            var received = new long[count];
            var sourceKinds = new string[count];
            await group.ReadAsync<long>(fields["Sequence"], sequences, cancellationToken: cancellationToken).ConfigureAwait(false);
            await group.ReadAsync<long>(fields["ElapsedTicks"], elapsed, cancellationToken: cancellationToken).ConfigureAwait(false);
            await group.ReadAsync<long>(fields["ReceivedAtUtcTicks"], received, cancellationToken: cancellationToken).ConfigureAwait(false);
            await group.ReadAsync(fields["SourceKind"], sourceKinds, cancellationToken: cancellationToken).ConfigureAwait(false);

            for (int row = 0; row < count; row++)
            {
                if (sequences[row] != expectedSequence)
                    throw new InvalidDataException($"Telemetry sequence is not contiguous at row {expectedSequence}.");
                if (elapsed[row] < previousElapsedTicks)
                    throw new InvalidDataException($"Telemetry elapsed time decreases at row {expectedSequence}.");
                if (received[row] < DateTimeOffset.MinValue.UtcTicks || received[row] > DateTimeOffset.MaxValue.UtcTicks)
                    throw new InvalidDataException($"Telemetry timestamp is invalid at row {expectedSequence}.");
                if (!Enum.TryParse(sourceKinds[row], ignoreCase: false, out TelemetrySourceKind _))
                    throw new InvalidDataException($"Telemetry source kind is invalid at row {expectedSequence}.");
                expectedSequence++;
                previousElapsedTicks = elapsed[row];
            }

            index.Add(new ReplayRowGroupIndex(
                groupIndex,
                expectedSequence - count,
                count,
                elapsed[0],
                elapsed[^1]));
        }
        return index;
    }
}

internal sealed class ParquetReplaySession : IAsyncDisposable
{
    private const int CacheCapacity = 3;

    private readonly ParquetReader _reader;
    private readonly IReadOnlyDictionary<string, DataField> _fields;
    private readonly IReadOnlyList<ReplayRowGroupIndex> _index;
    private readonly TelemetryParameterCatalog _catalog;
    private readonly SemaphoreSlim _readerLock = new(1, 1);
    private readonly Dictionary<int, CachedReplayGroup> _cache = [];
    private readonly LinkedList<int> _lru = [];

    public ParquetReplaySession(
        string path,
        ParquetReader reader,
        IReadOnlyDictionary<string, DataField> fields,
        IReadOnlyList<ReplayRowGroupIndex> index,
        TelemetryParameterCatalog catalog)
    {
        Path = path;
        _reader = reader;
        _fields = fields;
        _index = index;
        _catalog = catalog;
        RowCount = index[^1].LastRowIndex + 1;
        TotalElapsedTicks = index[^1].LastElapsedTicks;
    }

    public string Path { get; }
    public long RowCount { get; }
    public long TotalElapsedTicks { get; }

    public async Task<ReplayFrame> ReadAtOrBeforeAsync(long elapsedTicks, CancellationToken cancellationToken)
    {
        int groupIndex = FindGroupForElapsed(elapsedTicks);
        LoadedReplayGroup group = await GetGroupAsync(groupIndex, cancellationToken).ConfigureAwait(false);
        int row = FindRowAtOrBefore(group.ElapsedTicks, elapsedTicks);
        return group.CreateFrame(row, _catalog);
    }

    public async Task<ReplayFrame> ReadRowAsync(long rowIndex, CancellationToken cancellationToken)
    {
        if (rowIndex < 0 || rowIndex >= RowCount)
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        int groupIndex = FindGroupForRow(rowIndex);
        LoadedReplayGroup group = await GetGroupAsync(groupIndex, cancellationToken).ConfigureAwait(false);
        return group.CreateFrame(checked((int)(rowIndex - group.FirstRowIndex)), _catalog);
    }

    public async Task<IReadOnlyList<ReplayGraphSeries>> ReadGraphSeriesAsync(
        IReadOnlyList<string> parameterIds,
        long startElapsedTicks,
        long endElapsedTicks,
        int maximumPointsPerSeries,
        CancellationToken cancellationToken)
    {
        if (maximumPointsPerSeries < 4)
            throw new ArgumentOutOfRangeException(nameof(maximumPointsPerSeries));
        TelemetryParameterDescriptor[] descriptors = parameterIds
            .Distinct(StringComparer.Ordinal)
            .Select(_catalog.GetRequired)
            .ToArray();
        var points = descriptors.ToDictionary(
            descriptor => descriptor.Id,
            _ => new List<TelemetryGraphPoint>(),
            StringComparer.Ordinal);
        var categories = descriptors.ToDictionary(
            descriptor => descriptor.Id,
            _ => (IDictionary<string, double>)new Dictionary<string, double>(StringComparer.Ordinal),
            StringComparer.Ordinal);

        await _readerLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            foreach (ReplayRowGroupIndex groupIndex in _index)
            {
                if (groupIndex.LastElapsedTicks < startElapsedTicks || groupIndex.FirstElapsedTicks > endElapsedTicks)
                    continue;

                using ParquetRowGroupReader group = _reader.OpenRowGroupReader(groupIndex.GroupIndex);
                var elapsed = new long[groupIndex.RowCount];
                await group.ReadAsync<long>(_fields["ElapsedTicks"], elapsed, cancellationToken: cancellationToken).ConfigureAwait(false);
                foreach (TelemetryParameterDescriptor descriptor in descriptors)
                {
                    IParquetColumnReader column = ParquetColumnReaderFactory.Create(
                        descriptor,
                        _fields[descriptor.ParquetColumnName]);
                    await column.ReadAsync(group, groupIndex.RowCount, cancellationToken).ConfigureAwait(false);
                    List<TelemetryGraphPoint> destination = points[descriptor.Id];
                    IDictionary<string, double> descriptorCategories = categories[descriptor.Id];
                    for (int row = 0; row < elapsed.Length; row++)
                    {
                        if (elapsed[row] < startElapsedTicks || elapsed[row] > endElapsedTicks)
                            continue;
                        destination.Add(new TelemetryGraphPoint(
                            elapsed[row],
                            descriptor.ProjectValue(column.GetValue(row), descriptorCategories)));
                    }
                }
            }
        }
        finally
        {
            _readerLock.Release();
        }

        return descriptors
            .Select(descriptor => new ReplayGraphSeries(
                descriptor.Id,
                Downsample(points[descriptor.Id], maximumPointsPerSeries)))
            .ToArray();
    }

    public async ValueTask DisposeAsync()
    {
        await _readerLock.WaitAsync().ConfigureAwait(false);
        try
        {
            _cache.Clear();
            _lru.Clear();
            await _reader.DisposeAsync().ConfigureAwait(false);
        }
        finally
        {
            _readerLock.Release();
            _readerLock.Dispose();
        }
    }

    private async Task<LoadedReplayGroup> GetGroupAsync(int groupIndex, CancellationToken cancellationToken)
    {
        await _readerLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_cache.TryGetValue(groupIndex, out CachedReplayGroup? cached))
            {
                _lru.Remove(cached.Node);
                _lru.AddFirst(cached.Node);
                return cached.Group;
            }

            ReplayRowGroupIndex metadata = _index[groupIndex];
            using ParquetRowGroupReader reader = _reader.OpenRowGroupReader(groupIndex);
            LoadedReplayGroup loaded = await LoadedReplayGroup.LoadAsync(
                reader,
                metadata,
                _fields,
                _catalog,
                cancellationToken).ConfigureAwait(false);
            LinkedListNode<int> node = _lru.AddFirst(groupIndex);
            _cache.Add(groupIndex, new CachedReplayGroup(loaded, node));
            if (_cache.Count > CacheCapacity)
            {
                LinkedListNode<int> oldest = _lru.Last!;
                _lru.RemoveLast();
                _cache.Remove(oldest.Value);
            }
            return loaded;
        }
        finally
        {
            _readerLock.Release();
        }
    }

    private int FindGroupForElapsed(long elapsedTicks)
    {
        int low = 0;
        int high = _index.Count;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            if (_index[middle].FirstElapsedTicks <= elapsedTicks)
                low = middle + 1;
            else
                high = middle;
        }
        return Math.Max(0, low - 1);
    }

    private int FindGroupForRow(long rowIndex)
    {
        int low = 0;
        int high = _index.Count - 1;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            if (_index[middle].LastRowIndex < rowIndex)
                low = middle + 1;
            else
                high = middle;
        }
        return low;
    }

    private static int FindRowAtOrBefore(long[] elapsedTicks, long target)
    {
        if (target <= elapsedTicks[0])
            return 0;
        int low = 0;
        int high = elapsedTicks.Length;
        while (low < high)
        {
            int middle = low + ((high - low) / 2);
            if (elapsedTicks[middle] <= target)
                low = middle + 1;
            else
                high = middle;
        }
        return Math.Max(0, low - 1);
    }

    private static IReadOnlyList<TelemetryGraphPoint> Downsample(
        IReadOnlyList<TelemetryGraphPoint> source,
        int maximumPoints)
    {
        if (source.Count <= maximumPoints)
            return source;

        int bucketCount = Math.Max(1, maximumPoints / 5);
        var result = new List<(int Index, TelemetryGraphPoint Point)>(maximumPoints);
        for (int bucket = 0; bucket < bucketCount; bucket++)
        {
            int start = (int)((long)bucket * source.Count / bucketCount);
            int end = (int)((long)(bucket + 1) * source.Count / bucketCount);
            if (end <= start)
                continue;

            AddCandidate(result, start, source[start]);
            AddCandidate(result, end - 1, source[end - 1]);
            int? minimum = null;
            int? maximum = null;
            int? nullPoint = null;
            for (int index = start; index < end; index++)
            {
                double? value = source[index].Value;
                if (!value.HasValue)
                {
                    nullPoint ??= index;
                    continue;
                }
                if (!minimum.HasValue || value.Value < source[minimum.Value].Value)
                    minimum = index;
                if (!maximum.HasValue || value.Value > source[maximum.Value].Value)
                    maximum = index;
            }
            if (minimum.HasValue)
                AddCandidate(result, minimum.Value, source[minimum.Value]);
            if (maximum.HasValue)
                AddCandidate(result, maximum.Value, source[maximum.Value]);
            if (nullPoint.HasValue)
                AddCandidate(result, nullPoint.Value, source[nullPoint.Value]);
        }

        return result
            .OrderBy(item => item.Index)
            .DistinctBy(item => item.Index)
            .Take(maximumPoints)
            .Select(item => item.Point)
            .ToArray();
    }

    private static void AddCandidate(
        ICollection<(int Index, TelemetryGraphPoint Point)> destination,
        int index,
        TelemetryGraphPoint point) => destination.Add((index, point));

    private sealed record CachedReplayGroup(LoadedReplayGroup Group, LinkedListNode<int> Node);
}

internal sealed class LoadedReplayGroup
{
    private readonly long[] _sequence;
    private readonly long[] _receivedAtUtcTicks;
    private readonly string[] _sourceKinds;
    private readonly IReadOnlyDictionary<string, IParquetColumnReader> _columns;

    private LoadedReplayGroup(
        long firstRowIndex,
        long[] sequence,
        long[] receivedAtUtcTicks,
        long[] elapsedTicks,
        string[] sourceKinds,
        IReadOnlyDictionary<string, IParquetColumnReader> columns)
    {
        FirstRowIndex = firstRowIndex;
        _sequence = sequence;
        _receivedAtUtcTicks = receivedAtUtcTicks;
        ElapsedTicks = elapsedTicks;
        _sourceKinds = sourceKinds;
        _columns = columns;
    }

    public long FirstRowIndex { get; }
    public long[] ElapsedTicks { get; }

    public static async Task<LoadedReplayGroup> LoadAsync(
        ParquetRowGroupReader reader,
        ReplayRowGroupIndex metadata,
        IReadOnlyDictionary<string, DataField> fields,
        TelemetryParameterCatalog catalog,
        CancellationToken cancellationToken)
    {
        var sequence = new long[metadata.RowCount];
        var received = new long[metadata.RowCount];
        var elapsed = new long[metadata.RowCount];
        var source = new string[metadata.RowCount];
        await reader.ReadAsync<long>(fields["Sequence"], sequence, cancellationToken: cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync<long>(fields["ReceivedAtUtcTicks"], received, cancellationToken: cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync<long>(fields["ElapsedTicks"], elapsed, cancellationToken: cancellationToken).ConfigureAwait(false);
        await reader.ReadAsync(fields["SourceKind"], source, cancellationToken: cancellationToken).ConfigureAwait(false);

        var columns = new Dictionary<string, IParquetColumnReader>(catalog.All.Count, StringComparer.Ordinal);
        foreach (TelemetryParameterDescriptor descriptor in catalog.All)
        {
            IParquetColumnReader column = ParquetColumnReaderFactory.Create(
                descriptor,
                fields[descriptor.ParquetColumnName]);
            await column.ReadAsync(reader, metadata.RowCount, cancellationToken).ConfigureAwait(false);
            columns.Add(descriptor.Id, column);
        }
        return new LoadedReplayGroup(metadata.FirstRowIndex, sequence, received, elapsed, source, columns);
    }

    public ReplayFrame CreateFrame(int row, TelemetryParameterCatalog catalog)
    {
        var values = new Dictionary<string, object?>(catalog.All.Count, StringComparer.Ordinal);
        foreach (TelemetryParameterDescriptor descriptor in catalog.All)
            values.Add(descriptor.Id, _columns[descriptor.Id].GetValue(row));

        var telemetry = new RecordedSystemTelemetry(values);
        IReadOnlyList<FaultEntry> faults = BuildFaults(values);
        var sample = new UcsiTelemetrySample(
            _sequence[row],
            new DateTimeOffset(_receivedAtUtcTicks[row], TimeSpan.Zero),
            ElapsedTicks[row],
            telemetry,
            faults);
        return new ReplayFrame(FirstRowIndex + row, ElapsedTicks[row], sample);
    }

    private static IReadOnlyList<FaultEntry> BuildFaults(IReadOnlyDictionary<string, object?> values)
    {
        int count = Convert.ToInt32(values["faults.Count"]);
        var faults = new List<FaultEntry>(Math.Min(count, 4));
        for (int slot = 0; slot < Math.Min(count, 4); slot++)
        {
            string prefix = $"faults.{slot}";
            if (values[$"{prefix}.FaultType"] is not SystemFault type)
                continue;
            faults.Add(new FaultEntry(
                type,
                Convert.ToUInt32(values[$"{prefix}.FormatHash"]),
                (GcbStateNew)values[$"{prefix}.CapturedState"]!,
                Convert.ToUInt32(values[$"{prefix}.CapturedRuntime"]),
                (string?)values[$"{prefix}.Format"] ?? string.Empty,
                (string?)values[$"{prefix}.Message"] ?? string.Empty));
        }
        return faults;
    }
}

internal readonly record struct ReplayRowGroupIndex(
    int GroupIndex,
    long FirstRowIndex,
    int RowCount,
    long FirstElapsedTicks,
    long LastElapsedTicks)
{
    public long LastRowIndex => FirstRowIndex + RowCount - 1;
}
