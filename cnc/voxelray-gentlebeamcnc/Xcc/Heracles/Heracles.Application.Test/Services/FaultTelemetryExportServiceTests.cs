using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Moq;
using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Services;

internal sealed class FaultTelemetryExportServiceTests
{
    [Test]
    public async Task NewFaults_EachExportOnlyThePreviousFiveMinutes()
    {
        string exportDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export");
        HashSet<string> existingFiles = Directory.Exists(exportDirectory)
            ? Directory.GetFiles(exportDirectory, "fault-session-data-export-*.csv").ToHashSet()
            : [];

        var eventAggregator = new EventAggregator();
        var history = new TelemetryHistoryBuffer();
        var catalog = new TelemetryParameterCatalog();
        var logBuffer = new UcsiLogBuffer();
        var service = new FaultTelemetryExportService(
            eventAggregator,
            history,
            new SessionDataExportService(catalog),
            logBuffer);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        UcsiTelemetrySample oldSample = CreateSample(0, now.AddMinutes(-6));
        UcsiTelemetrySample recentSample = CreateSample(1, now.AddMinutes(-1));
        history.Append(oldSample);
        history.Append(recentSample);

        FaultEntry first = CreateFault(1, "First fault");
        FaultEntry second = CreateFault(2, "Second fault");

        try
        {
            service.Start();
            FaultsChangedEvent faultsChanged = eventAggregator.GetEvent<FaultsChangedEvent>();
            faultsChanged.Publish(new[] { first });
            faultsChanged.Publish(new[] { first });
            faultsChanged.Publish(new[] { first, second });
            faultsChanged.Publish(Array.Empty<FaultEntry>());
            faultsChanged.Publish(new[] { first });
            await service.DisposeAsync();

            string[] createdFiles = Directory
                .GetFiles(exportDirectory, "fault-session-data-export-*.csv")
                .Where(path => !existingFiles.Contains(path))
                .ToArray();

            Assert.That(createdFiles, Has.Length.EqualTo(3));
            foreach (string path in createdFiles)
            {
                string csv = await File.ReadAllTextAsync(path);
                Assert.Multiple(() =>
                {
                    Assert.That(csv, Does.Contain("Timestamp (UTC)"));
                    Assert.That(csv, Does.Contain(recentSample.ReceivedAtUtc.ToString("O")));
                    Assert.That(csv, Does.Not.Contain(oldSample.ReceivedAtUtc.ToString("O")));
                });
            }

            Assert.That(
                logBuffer.Snapshot().Count(entry => entry.Severity == LogRecordSeverity.Error),
                Is.Zero);
        }
        finally
        {
            await service.DisposeAsync();
            if (Directory.Exists(exportDirectory))
            {
                foreach (string path in Directory.GetFiles(exportDirectory, "fault-session-data-export-*.csv"))
                {
                    if (!existingFiles.Contains(path))
                        File.Delete(path);
                }
            }
        }
    }

    private static UcsiTelemetrySample CreateSample(long sequence, DateTimeOffset receivedAtUtc)
    {
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.Faults).Returns(default(SystemFaults));
        telemetry.SetupGet(value => value.Interlocks).Returns(default(SystemInterlocks));
        telemetry.SetupGet(value => value.Hvps).Returns(default(HvpsTelemetryStatus));
        return new UcsiTelemetrySample(
            sequence,
            receivedAtUtc,
            TimeSpan.FromSeconds(sequence).Ticks,
            telemetry.Object,
            Array.Empty<FaultEntry>());
    }

    private static FaultEntry CreateFault(uint hash, string message) =>
        new(
            SystemFault.OtherFault,
            hash,
            GcbStateNew.Ready,
            100,
            message,
            message);
}
