using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Heracles.Ucsi.Models;
using Xcc.Core.Domain.GryphonBoard;

namespace Heracles.Ucsi.Services;

/// <summary>
/// Service for exporting live session telemetry data to CSV format.
/// Converts in-memory telemetry samples to CSV with all parameter columns.
/// </summary>
public sealed class SessionDataExportService(
    TelemetryParameterCatalog catalog)
{
    private readonly TelemetryParameterCatalog _catalog = catalog;
    private long _exportSequence;

    /// <summary>
    /// Export live session data to a timestamped CSV file in the application directory.
    /// </summary>
    /// <param name="samples">Telemetry samples to export (typically last 5 minutes)</param>
    /// <returns>Full path to the created CSV file</returns>
    /// <exception cref="IOException">If file creation or writing fails</exception>
    public string ExportToCsv(IReadOnlyList<UcsiTelemetrySample> samples)
    {
        if (samples.Count == 0)
            throw new InvalidOperationException("No telemetry samples to export.");

        return ExportToCsv(samples, "session-data-export");
    }

    public string ExportFaultToCsv(
        IReadOnlyList<UcsiTelemetrySample> samples,
        FaultEntry fault)
    {
        ArgumentNullException.ThrowIfNull(fault);
        return ExportToCsv(samples, $"fault-session-data-export-{fault.FaultType}");
    }

    private string ExportToCsv(
        IReadOnlyList<UcsiTelemetrySample> samples,
        string filenamePrefix)
    {
        long sequence = Interlocked.Increment(ref _exportSequence);
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        string filename = $"{filenamePrefix}-{timestamp}-{sequence:D6}.csv";
        string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export");
        Directory.CreateDirectory(exportDir);
        string outputPath = Path.Combine(exportDir, filename);

        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8, 65536))
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            csv.WriteField("Timestamp (UTC)");
            csv.WriteField("Elapsed Time (s)");
            csv.WriteField("Sequence");

            foreach (TelemetryParameterDescriptor descriptor in _catalog.All)
                csv.WriteField(descriptor.DisplayName);

            csv.NextRecord();

            foreach (UcsiTelemetrySample sample in samples)
            {
                csv.WriteField(sample.ReceivedAtUtc.ToString("O"));
                double elapsedSeconds = sample.LiveElapsedTicks / (double)TimeSpan.TicksPerSecond;
                csv.WriteField(elapsedSeconds.ToString("F3", CultureInfo.InvariantCulture));
                csv.WriteField(sample.LiveSequence);

                foreach (TelemetryParameterDescriptor descriptor in _catalog.All)
                {
                    object? value = descriptor.GetValue(sample);
                    csv.WriteField(value?.ToString() ?? string.Empty);
                }

                csv.NextRecord();
            }

            csv.Flush();
        }

        return outputPath;
    }
}
