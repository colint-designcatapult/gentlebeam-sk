using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using Heracles.Ucsi.Models;
using Parquet;
using Parquet.Schema;

namespace Heracles.Ucsi.Storage;

internal sealed record ParquetWriteResult(long WrittenCount, bool FooterCompleted);

internal sealed class ParquetSessionWriteException(
    string message,
    long durableRowCount,
    Exception innerException) : IOException(message, innerException)
{
    public long DurableRowCount { get; } = durableRowCount;
}


public sealed class ParquetTelemetrySessionWriter(
    TelemetryParameterCatalog catalog)
{
    internal const int RowGroupSize = 10_000;
    internal const string SchemaVersion = "1";

    private readonly TelemetryParameterCatalog _catalog = catalog;

    internal async Task<ParquetWriteResult> WriteAsync(
        string partialPath,
        DateTimeOffset captureStartUtc,
        ChannelReader<RecordingEnvelope> source,
        CancellationToken cancellationToken)
    {
        long durableRows = 0;
        await using var stream = new FileStream(
            partialPath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.Read,
            1 << 20,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        try
        {
            if (!await source.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
                return new ParquetWriteResult(0, false);

            DataField<long> sequenceField = new("Sequence", false);
            DataField<long> receivedAtUtcTicksField = new("ReceivedAtUtcTicks", false);
            DataField<long> elapsedTicksField = new("ElapsedTicks", false);
            DataField<string> sourceKindField = new("SourceKind", false);
            DataField<byte[]> rawDatagramField = new("RawDatagram", true);
            IParquetColumnBuffer[] parameterBuffers = _catalog.All
                .Select(descriptor => descriptor.CreateParquetBuffer(RowGroupSize))
                .ToArray();
            DataField[] parameterFields = parameterBuffers
                .Select(buffer => buffer.Field)
                .ToArray();
            var schemaFields = new List<Field>(5 + parameterFields.Length)
            {
                sequenceField,
                receivedAtUtcTicksField,
                elapsedTicksField,
                sourceKindField,
                rawDatagramField,
            };
            schemaFields.AddRange(parameterFields);
            var schema = new ParquetSchema(schemaFields);

            var options = new ParquetOptions
            {
                CompressionMethod = CompressionMethod.Zstd,
                CompressionLevel = CompressionLevel.SmallestSize,
            };
            options.ColumnEncodingHints[sequenceField.Path.ToString()] = EncodingHint.DeltaBinaryPacked;
            options.ColumnEncodingHints[receivedAtUtcTicksField.Path.ToString()] = EncodingHint.DeltaBinaryPacked;
            options.ColumnEncodingHints[elapsedTicksField.Path.ToString()] = EncodingHint.DeltaBinaryPacked;
            options.ColumnEncodingHints[sourceKindField.Path.ToString()] = EncodingHint.Dictionary;
            foreach (TelemetryParameterDescriptor descriptor in _catalog.All)
            {
                Type actualType = Nullable.GetUnderlyingType(descriptor.ValueType) ?? descriptor.ValueType;
                if (actualType == typeof(string))
                    options.ColumnEncodingHints[descriptor.ParquetColumnName] = EncodingHint.Dictionary;
                else if (actualType == typeof(int) || actualType == typeof(long) || actualType == typeof(uint))
                    options.ColumnEncodingHints[descriptor.ParquetColumnName] = EncodingHint.DeltaBinaryPacked;
            }

            await using ParquetWriter writer = await ParquetWriter.CreateAsync(
                schema,
                stream,
                options: options,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            writer.CustomMetadata = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["ucsi.schema.version"] = SchemaVersion,
                ["ucsi.capture.startUtc"] = captureStartUtc.ToString("O"),
                ["ucsi.capture.rawDatagram"] = "true",
            };

            var sequences = new long[RowGroupSize];
            var receivedTicks = new long[RowGroupSize];
            var elapsedTicks = new long[RowGroupSize];
            var sourceKinds = new string[RowGroupSize];
            var rawDatagrams = new byte[]?[RowGroupSize];

            int bufferedRows = 0;
            while (await source.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (source.TryRead(out RecordingEnvelope envelope))
                {
                    sequences[bufferedRows] = durableRows + bufferedRows;
                    receivedTicks[bufferedRows] = envelope.Sample.ReceivedAtUtc.UtcTicks;
                    elapsedTicks[bufferedRows] = envelope.ElapsedTicks;
                    sourceKinds[bufferedRows] = envelope.SourceKind.ToString();
                    rawDatagrams[bufferedRows] = GetOwnedDatagram(envelope.RawDatagram);
                    foreach (IParquetColumnBuffer parameterBuffer in parameterBuffers)
                        parameterBuffer.Append(envelope.Sample);

                    bufferedRows++;
                    if (bufferedRows == RowGroupSize)
                    {
                        await FlushRowGroupAsync(
                            writer,
                            sequenceField,
                            receivedAtUtcTicksField,
                            elapsedTicksField,
                            sourceKindField,
                            rawDatagramField,
                            sequences,
                            receivedTicks,
                            elapsedTicks,
                            sourceKinds,
                            rawDatagrams,
                            parameterBuffers,
                            bufferedRows,
                            cancellationToken).ConfigureAwait(false);
                        durableRows += bufferedRows;
                        bufferedRows = 0;
                    }
                }
            }

            if (bufferedRows > 0)
            {
                await FlushRowGroupAsync(
                    writer,
                    sequenceField,
                    receivedAtUtcTicksField,
                    elapsedTicksField,
                    sourceKindField,
                    rawDatagramField,
                    sequences,
                    receivedTicks,
                    elapsedTicks,
                    sourceKinds,
                    rawDatagrams,
                    parameterBuffers,
                    bufferedRows,
                    cancellationToken).ConfigureAwait(false);
                durableRows += bufferedRows;
            }

            return new ParquetWriteResult(durableRows, true);
        }
        catch (Exception exception) when (exception is not ParquetSessionWriteException)
        {
            throw new ParquetSessionWriteException(
                "Failed to write the telemetry recording.",
                durableRows,
                exception);
        }
    }

    private static async Task FlushRowGroupAsync(
        ParquetWriter writer,
        DataField<long> sequenceField,
        DataField<long> receivedAtUtcTicksField,
        DataField<long> elapsedTicksField,
        DataField<string> sourceKindField,
        DataField<byte[]> rawDatagramField,
        long[] sequences,
        long[] receivedTicks,
        long[] elapsedTicks,
        string[] sourceKinds,
        byte[]?[] rawDatagrams,
        IParquetColumnBuffer[] parameterBuffers,
        int count,
        CancellationToken cancellationToken)
    {
        using ParquetRowGroupWriter group = writer.CreateRowGroup();
        await group.WriteAsync<long>(sequenceField, new ReadOnlyMemory<long>(sequences, 0, count), cancellationToken: cancellationToken).ConfigureAwait(false);
        await group.WriteAsync<long>(receivedAtUtcTicksField, new ReadOnlyMemory<long>(receivedTicks, 0, count), cancellationToken: cancellationToken).ConfigureAwait(false);
        await group.WriteAsync<long>(elapsedTicksField, new ReadOnlyMemory<long>(elapsedTicks, 0, count), cancellationToken: cancellationToken).ConfigureAwait(false);
        IReadOnlyCollection<string?> sourceValues = count == sourceKinds.Length
            ? sourceKinds
            : new ArraySegment<string>(sourceKinds, 0, count);
        await group.WriteAsync(sourceKindField, sourceValues).ConfigureAwait(false);
        IReadOnlyCollection<byte[]?> rawValues = count == rawDatagrams.Length
            ? rawDatagrams
            : new ArraySegment<byte[]?>(rawDatagrams, 0, count);
        await group.WriteAsync(rawDatagramField, rawValues).ConfigureAwait(false);
        foreach (IParquetColumnBuffer parameterBuffer in parameterBuffers)
        {
            await parameterBuffer.WriteAsync(group, cancellationToken).ConfigureAwait(false);
            parameterBuffer.Clear();
        }

        Array.Clear(sourceKinds, 0, count);
        Array.Clear(rawDatagrams, 0, count);
    }

    private static byte[]? GetOwnedDatagram(ReadOnlyMemory<byte> rawDatagram)
    {
        if (rawDatagram.IsEmpty)
            return null;

        if (MemoryMarshal.TryGetArray(rawDatagram, out ArraySegment<byte> segment)
            && segment.Offset == 0
            && segment.Count == segment.Array!.Length)
        {
            return segment.Array;
        }

        return rawDatagram.ToArray();
    }
}
