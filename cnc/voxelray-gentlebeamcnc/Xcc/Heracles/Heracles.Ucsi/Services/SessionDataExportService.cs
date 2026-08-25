using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Heracles.Ucsi.Models;

namespace Heracles.Ucsi.Services;

/// <summary>
/// Service for exporting live session telemetry data to CSV format.
/// Converts in-memory telemetry samples to CSV with all parameter columns.
/// </summary>
public sealed class SessionDataExportService(
    TelemetryParameterCatalog catalog)
{
    private readonly TelemetryParameterCatalog _catalog = catalog;

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

        // Generate timestamped filename: session-data-export-20260820-143052.csv
        string timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss");
        string filename = $"session-data-export-{timestamp}.csv";
        string exportDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export");
        Directory.CreateDirectory(exportDir);
        string outputPath = Path.Combine(exportDir, filename);

        // Create CSV with proper configuration
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8, 65536))
        using (var csv = new CsvWriter(writer, csvConfig))
        {
            // Write header row
            csv.WriteField("Timestamp (UTC)");
            csv.WriteField("Elapsed Time (s)");
            csv.WriteField("Sequence");

            // Write parameter column headers
            foreach (TelemetryParameterDescriptor descriptor in _catalog.All)
            {
                csv.WriteField(descriptor.DisplayName);
            }

            csv.NextRecord();

            // Write data rows
            foreach (UcsiTelemetrySample sample in samples)
            {
                // Timestamp
                csv.WriteField(sample.ReceivedAtUtc.ToString("O"));

                // Elapsed time in seconds
                double elapsedSeconds = sample.LiveElapsedTicks / (double)TimeSpan.TicksPerSecond;
                csv.WriteField(elapsedSeconds.ToString("F3", CultureInfo.InvariantCulture));

                // Sequence number
                csv.WriteField(sample.LiveSequence);

                // Parameter values (via GetValue method)
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
