using System.IO;
using System.Threading.Channels;
using Heracles.Ucsi.Models;
using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Heracles.Ucsi.Services;

public sealed class FaultTelemetryExportService(
    IEventAggregator eventAggregator,
    TelemetryHistoryBuffer liveHistory,
    SessionDataExportService exportService,
    UcsiLogBuffer logBuffer) : IAsyncDisposable
{
    private static readonly TimeSpan ExportWindow = TimeSpan.FromMinutes(5);

    private readonly object _gate = new();
    private readonly HashSet<FaultEntry> _activeFaults = [];
    private readonly Channel<FaultExportRequest> _requests =
        Channel.CreateUnbounded<FaultExportRequest>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

    private SubscriptionToken? _subscription;
    private Task? _worker;
    private bool _accepting;
    private bool _disposed;

    public void Start()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_accepting)
                return;

            _worker = Task.Run(ProcessExportsAsync);
            _subscription = eventAggregator
                .GetEvent<FaultsChangedEvent>()
                .Subscribe(OnFaultsChanged);
            _accepting = true;
        }
    }

    private void OnFaultsChanged(IReadOnlyList<FaultEntry> faults)
    {
        lock (_gate)
        {
            if (!_accepting)
                return;

            FaultEntry[] newFaults = faults
                .Where(fault => !_activeFaults.Contains(fault))
                .ToArray();

            _activeFaults.Clear();
            _activeFaults.UnionWith(faults);

            if (newFaults.Length == 0)
                return;

            DateTimeOffset reportedAtUtc = DateTimeOffset.UtcNow;
            IReadOnlyList<UcsiTelemetrySample> samples =
                liveHistory.GetSince(reportedAtUtc - ExportWindow);

            foreach (FaultEntry fault in newFaults)
            {
                if (!_requests.Writer.TryWrite(new FaultExportRequest(fault, samples)))
                {
                    logBuffer.Log(
                        $"Could not queue the automatic telemetry export for fault: {fault}",
                        LogRecordSeverity.Error,
                        LogRecordType.System);
                }
            }
        }
    }

    private async Task ProcessExportsAsync()
    {
        await foreach (FaultExportRequest request in _requests.Reader.ReadAllAsync())
        {
            try
            {
                string outputPath = exportService.ExportFaultToCsv(request.Samples, request.Fault);
                logBuffer.Log(
                    $"Automatically exported {request.Samples.Count} telemetry samples for fault " +
                    $"{request.Fault.FaultType}: {Path.GetFileName(outputPath)}",
                    LogRecordSeverity.Info,
                    LogRecordType.System);
            }
            catch (Exception exception)
            {
                logBuffer.Log(
                    $"Automatic telemetry export failed for fault {request.Fault}: {exception.Message}",
                    LogRecordSeverity.Error,
                    LogRecordType.System);
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task? worker;
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _accepting = false;
            _subscription?.Dispose();
            _subscription = null;
            _requests.Writer.TryComplete();
            worker = _worker;
        }

        if (worker is not null)
            await worker.ConfigureAwait(false);
    }

    private readonly record struct FaultExportRequest(
        FaultEntry Fault,
        IReadOnlyList<UcsiTelemetrySample> Samples);
}
