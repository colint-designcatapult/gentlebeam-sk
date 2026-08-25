using System.Collections.ObjectModel;
using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;
using Microsoft.Extensions.Configuration;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Core.Services;

namespace Heracles.Ucsi.Services;

public sealed class UcsiStandaloneTelemetryOptions
{
    public UcsiStandaloneTelemetryOptions(IConfiguration configuration)
    {
        RemoteAddress = configuration["Ucsi:Telemetry:RemoteAddress"] ?? "172.31.1.100";
        RemotePort = configuration.GetValue("Ucsi:Telemetry:RemotePort", 20);
        ListenerPort = configuration.GetValue("Ucsi:Telemetry:ListenerPort", 40_020);
        if (RemotePort is < 1 or > 65_535)
            throw new InvalidOperationException("Ucsi:Telemetry:RemotePort must be between 1 and 65535.");
        if (ListenerPort is < 0 or > 65_535)
            throw new InvalidOperationException("Ucsi:Telemetry:ListenerPort must be between 0 and 65535.");
    }

    public string RemoteAddress { get; }
    public int RemotePort { get; }
    public int ListenerPort { get; }
}

public sealed class UcsiStandaloneCommandOptions
{
    public UcsiStandaloneCommandOptions(IConfiguration configuration)
    {
        RemoteAddress = configuration["Ucsi:Commands:RemoteAddress"] ?? "172.31.1.100";
        RemotePort = configuration.GetValue("Ucsi:Commands:RemotePort", 20);
        if (RemotePort is < 1 or > 65_535)
            throw new InvalidOperationException("Ucsi:Commands:RemotePort must be between 1 and 65535.");
    }

    public string RemoteAddress { get; }
    public int RemotePort { get; }
}

public sealed class StandaloneTelemetryConnectionFactory(
    UcsiStandaloneTelemetryOptions options) : IGcbTelemetryConnectionFactory
{
    public IAsyncClientConnection GetGcbTelemetryConnection()
    {
        int listenerPort = options.ListenerPort > 0 ? options.ListenerPort : options.RemotePort;
        return new UdpClientConnection(
            options.RemoteAddress,
            options.RemotePort,
            listenerPort,
            reusePort: options.ListenerPort == 0);
    }
}

public sealed class StandaloneSystemTelemetrySink(
    IGCBDataStore dataStore) : ISystemTelemetryChanged
{
    public void OnSystemTelemetryChanged(ISystemTelemetry? systemTelemetry) =>
        dataStore.SystemTelemetry = systemTelemetry;
}

public sealed record UcsiLogEntry(
    DateTimeOffset Timestamp,
    string Message,
    LogRecordSeverity Severity,
    LogRecordType Type);

public sealed class UcsiLogBuffer : ILogWriter
{
    private const int MaximumEntries = 2_000;
    private readonly object _gate = new();
    private readonly Queue<UcsiLogEntry> _entries = new(MaximumEntries);

    public void Log(string message, LogRecordSeverity severity, LogRecordType type)
    {
        lock (_gate)
        {
            if (_entries.Count == MaximumEntries)
                _entries.Dequeue();
            _entries.Enqueue(new UcsiLogEntry(DateTimeOffset.Now, message, severity, type));
        }
    }

    public Task LogAsync(string message, LogRecordSeverity messageType, LogRecordType type)
    {
        Log(message, messageType, type);
        return Task.CompletedTask;
    }

    public IReadOnlyList<UcsiLogEntry> Snapshot()
    {
        lock (_gate)
            return new ReadOnlyCollection<UcsiLogEntry>(_entries.ToArray());
    }
}

public sealed class StandaloneUcsiLifecycle(
    ITelemetrySessionCoordinator coordinator,
    ITelemetryService telemetryService,
    IAppGlobals appGlobals) : IAsyncDisposable
{
    private bool _started;
    private bool _disposed;

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_started)
            return;
        coordinator.Start();
        telemetryService.Start();
        _started = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (_started)
            telemetryService.Stop();
        await coordinator.DisposeAsync().ConfigureAwait(false);
        telemetryService.Dispose();
        appGlobals.AppCancellationTokenSource.Cancel();
        appGlobals.AppCancellationTokenSource.Dispose();
    }
}
