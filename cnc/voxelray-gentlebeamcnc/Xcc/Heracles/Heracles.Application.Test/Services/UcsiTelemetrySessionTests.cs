using System.Reflection;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.Storage;
using Heracles.Ucsi.ViewModels;
using Moq;
using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Services;

internal sealed class UcsiTelemetrySessionTests
{
    [Test]
    public void Catalog_CoversEveryTelemetryPropertyAndNestedState()
    {
        var catalog = new TelemetryParameterCatalog();

        string[] rootContainers =
        [
            nameof(ISystemTelemetry.Faults),
            nameof(ISystemTelemetry.Interlocks),
            nameof(ISystemTelemetry.Hvps),
            nameof(ISystemTelemetry.Mag1),
            nameof(ISystemTelemetry.Mag2),
        ];
        foreach (PropertyInfo property in typeof(ISystemTelemetry).GetProperties())
        {
            if (!rootContainers.Contains(property.Name, StringComparer.Ordinal))
                Assert.That(catalog.ById, Contains.Key($"system.{property.Name}"));
        }
        foreach (PropertyInfo property in typeof(SystemFaults).GetProperties())
            Assert.That(catalog.ById, Contains.Key($"system.Faults.{property.Name}"));
        foreach (SystemFault fault in Enum.GetValues<SystemFault>())
            Assert.That(catalog.ById, Contains.Key($"system.Faults.{fault}"));
        foreach (PropertyInfo property in typeof(SystemInterlocks).GetProperties())
            Assert.That(catalog.ById, Contains.Key($"system.Interlocks.{property.Name}"));
        foreach (SystemInterlock interlock in Enum.GetValues<SystemInterlock>())
            Assert.That(catalog.ById, Contains.Key($"system.Interlocks.{interlock}.Required"));
        foreach (PropertyInfo property in typeof(HvpsTelemetryStatus).GetProperties())
            Assert.That(catalog.ById, Contains.Key($"system.Hvps.{property.Name}"));
        foreach (string vector in new[] { nameof(ISystemTelemetry.Mag1), nameof(ISystemTelemetry.Mag2) })
        foreach (string component in new[] { nameof(TelemetryVector3.X), nameof(TelemetryVector3.Y), nameof(TelemetryVector3.Z) })
            Assert.That(catalog.ById, Contains.Key($"system.{vector}.{component}"));

        Assert.That(catalog.All, Has.Some.Matches<TelemetryParameterDescriptor>(parameter => parameter.IsMock));
        Assert.That(catalog.All.Select(parameter => parameter.Id), Is.Unique);
        Assert.That(catalog.All.Select(parameter => parameter.ParquetColumnName), Is.Unique);
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void GraphPane_SupportsArbitraryMultiSeriesSelectionAndRemoval()
    {
        var catalog = new TelemetryParameterCatalog();
        GraphPaneViewModel? removed = null;
        var graph = new GraphPaneViewModel(
            1,
            catalog,
            value => removed = value,
            "system.KvFeedback",
            "system.EmissionCurrent");

        Assert.Multiple(() =>
        {
            Assert.That(graph.SelectedParameterIds, Is.EquivalentTo(new[]
            {
                "system.KvFeedback",
                "system.EmissionCurrent",
            }));
            Assert.That(graph.SelectionSummary, Is.EqualTo("2 series"));
        });

        graph.ParameterOptions.Single(option => option.Id == "system.HeaterCurrentFeedback").IsSelected = true;
        graph.RemoveCommand.Execute();

        Assert.Multiple(() =>
        {
            Assert.That(graph.SelectedParameterIds, Has.Count.EqualTo(3));
            Assert.That(graph.SelectionSummary, Is.EqualTo("3 series"));
            Assert.That(removed, Is.SameAs(graph));
        });
    }

    [Test]
    public async Task Recording_RoundTripsAllSamplesAndSupportsReplaySeekingAndGraphing()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"Ucsi-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "session.parquet");
        TelemetrySessionCoordinator coordinator = CreateCoordinator(out DecodedTelemetryFrameHub hub);
        try
        {
            coordinator.Start();
            coordinator.StartRecording(path);
            DateTimeOffset start = DateTimeOffset.UtcNow;
            for (int index = 0; index < 125; index++)
            {
                ISystemTelemetry telemetry = CreateTelemetry(index);
                hub.Publish(new DecodedTelemetryFrame(
                    start.AddMilliseconds(index * 10),
                    telemetry,
                    new byte[] { 0x47, (byte)index, 0x42 }));
            }

            await coordinator.StopRecordingAsync();

            Assert.Multiple(() =>
            {
                Assert.That(coordinator.AcceptedRecordingSamples, Is.EqualTo(125));
                Assert.That(coordinator.WrittenRecordingSamples, Is.EqualTo(125));
                Assert.That(File.Exists(path), Is.True);
                Assert.That(File.Exists(path + ".partial"), Is.False);
            });

            await coordinator.LoadReplayAsync(path);
            Assert.Multiple(() =>
            {
                Assert.That(coordinator.Mode, Is.EqualTo(UcsiMode.Replay));
                Assert.That(coordinator.TransportState, Is.EqualTo(SessionTransportState.Paused));
                Assert.That(coordinator.SessionRowCount, Is.EqualTo(125));
                Assert.That(coordinator.CurrentSample?.Telemetry.KvFeedback, Is.EqualTo(0));
            });

            IReadOnlyList<ReplayGraphSeries> graph = await coordinator.ReadReplayGraphSeriesAsync(
                ["system.KvFeedback", "system.EmissionCurrent"],
                0,
                coordinator.TotalElapsedTicks,
                50);
            Assert.Multiple(() =>
            {
                Assert.That(graph, Has.Count.EqualTo(2));
                Assert.That(graph[0].Points, Has.Count.LessThanOrEqualTo(50));
                Assert.That(graph[1].Points, Has.Count.LessThanOrEqualTo(50));
                Assert.That(graph[0].Points.First().Value, Is.EqualTo(0));
                Assert.That(graph[0].Points.Last().Value, Is.EqualTo(124));
            });

            await coordinator.SeekAsync(coordinator.TotalElapsedTicks);
            Assert.Multiple(() =>
            {
                Assert.That(coordinator.CurrentSample?.Telemetry.KvFeedback, Is.EqualTo(124));
                Assert.That(coordinator.CurrentSample?.Telemetry.EmissionCurrent, Is.EqualTo(12.4f).Within(0.001));
                Assert.That(coordinator.CurrentSample?.Telemetry.Interlocks.IsRequired(SystemInterlock.DoorClosed), Is.True);
            });

            await coordinator.ReturnToLiveAsync();
            Assert.That(coordinator.Mode, Is.EqualTo(UcsiMode.Live));
        }
        finally
        {
            await coordinator.DisposeAsync();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task Replay_SeeksToPreviousSampleAcrossRowGroupTimeGap()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"Ucsi-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "multi-group.parquet");
        TelemetrySessionCoordinator coordinator = CreateCoordinator(out DecodedTelemetryFrameHub hub);
        try
        {
            coordinator.Start();
            coordinator.StartRecording(path);
            ISystemTelemetry telemetry = CreateTelemetry(7);
            DateTimeOffset receivedAt = DateTimeOffset.UtcNow;
            byte[] datagram = [0x47, 0x42];
            for (int index = 0; index < 10_000; index++)
            {
                hub.Publish(new DecodedTelemetryFrame(
                    receivedAt.AddMilliseconds(index),
                    telemetry,
                    datagram));
            }
            await Task.Delay(100);
            hub.Publish(new DecodedTelemetryFrame(
                receivedAt.AddMilliseconds(10_100),
                telemetry,
                datagram));

            await coordinator.StopRecordingAsync();
            await coordinator.LoadReplayAsync(path);
            IReadOnlyList<ReplayGraphSeries> graph = await coordinator.ReadReplayGraphSeriesAsync(
                ["system.KvFeedback"],
                0,
                coordinator.TotalElapsedTicks,
                10_001);
            IReadOnlyList<TelemetryGraphPoint> points = graph.Single().Points;
            long gapTarget = points[9_999].ElapsedTicks
                + ((points[10_000].ElapsedTicks - points[9_999].ElapsedTicks) / 2);

            await coordinator.SeekAsync(gapTarget);

            Assert.Multiple(() =>
            {
                Assert.That(coordinator.SessionRowCount, Is.EqualTo(10_001));
                Assert.That(points, Has.Count.EqualTo(10_001));
                Assert.That(coordinator.CurrentSample?.LiveSequence, Is.EqualTo(9_999));
            });
        }
        finally
        {
            await coordinator.DisposeAsync();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    [Test]
    public async Task EmptyRecording_DoesNotPublishACompletedOrPartialSession()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"Ucsi-{Guid.NewGuid():N}");
        string path = Path.Combine(directory, "empty.parquet");
        TelemetrySessionCoordinator coordinator = CreateCoordinator(out _);
        try
        {
            coordinator.Start();
            coordinator.StartRecording(path);

            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(coordinator.StopRecordingAsync)!;

            Assert.That(exception.Message, Does.Contain("No full-rate telemetry samples"));
            Assert.That(File.Exists(path), Is.False);
            Assert.That(File.Exists(path + ".partial"), Is.False);
            Assert.That(coordinator.TransportState, Is.EqualTo(SessionTransportState.Idle));
        }
        finally
        {
            await coordinator.DisposeAsync();
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
    }

    private static TelemetrySessionCoordinator CreateCoordinator(out DecodedTelemetryFrameHub hub)
    {
        var catalog = new TelemetryParameterCatalog();
        hub = new DecodedTelemetryFrameHub();
        var dataStore = new GCBDataStore(new EventAggregator());
        return new TelemetrySessionCoordinator(
            hub,
            dataStore,
            new ParquetTelemetrySessionWriter(catalog),
            new ParquetTelemetrySessionReader(catalog),
            new TelemetryHistoryBuffer());
    }

    private static ISystemTelemetry CreateTelemetry(int index)
    {
        ulong doorMask = 1UL << (int)SystemInterlock.DoorClosed;
        var interlocks = new SystemInterlocks(0, 0, doorMask, doorMask, doorMask);
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.FirmwareMode).Returns(FirmwareMode.Normal);
        telemetry.SetupGet(value => value.ControlBoardState).Returns(GcbStateNew.StandBy);
        telemetry.SetupGet(value => value.SystemRuntime).Returns(index * 10);
        telemetry.SetupGet(value => value.Faults).Returns(default(SystemFaults));
        telemetry.SetupGet(value => value.Interlocks).Returns(interlocks);
        telemetry.SetupGet(value => value.Hvps).Returns(default(HvpsTelemetryStatus));
        telemetry.SetupGet(value => value.KvFeedback).Returns(index);
        telemetry.SetupGet(value => value.EmissionCurrent).Returns(index / 10f);
        telemetry.SetupGet(value => value.HeaterCurrentFeedback).Returns(index / 20f);
        telemetry.SetupGet(value => value.Mag1).Returns(new TelemetryVector3(index, index + 1, index + 2));
        telemetry.Setup(value => value.IsFaultState()).Returns(false);
        telemetry.Setup(value => value.IsEmissionState()).Returns(false);
        return telemetry.Object;
    }
}
