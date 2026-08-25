using System.Diagnostics;
using System.IO;
using System.Threading.Channels;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.Storage;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Heracles.Ucsi.Services;

public interface ITelemetrySessionCoordinator : IAsyncDisposable
{
    UcsiMode Mode { get; }
    SessionTransportState TransportState { get; }
    TelemetryHistoryBuffer LiveHistory { get; }
    UcsiTelemetrySample? CurrentSample { get; }
    long CurrentElapsedTicks { get; }
    long TotalElapsedTicks { get; }
    long SessionRowCount { get; }
    long AcceptedRecordingSamples { get; }
    long WrittenRecordingSamples { get; }
    string? CurrentReplayPath { get; }
    string? LastError { get; }
    double LiveSampleRate { get; }
    void Start();
    void StartRecording(string completedPath);
    Task StopRecordingAsync();
    Task LoadReplayAsync(string path, CancellationToken cancellationToken = default);
    Task TogglePlaybackAsync(CancellationToken cancellationToken = default);
    Task SeekAsync(long elapsedTicks, CancellationToken cancellationToken = default);
    Task ReturnToLiveAsync();
    Task AdvancePresentationAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReplayGraphSeries>> ReadReplayGraphSeriesAsync(
        IReadOnlyList<string> parameterIds,
        long startElapsedTicks,
        long endElapsedTicks,
        int maximumPointsPerSeries,
        CancellationToken cancellationToken = default);
}

public sealed class TelemetrySessionCoordinator : ITelemetrySessionCoordinator
{
    private const int RecordingChannelCapacity = 16_384;

    private readonly object _gate = new();
    private readonly IDecodedTelemetryFrameSource _frameSource;
    private readonly IGCBDataStore _gcbDataStore;
    private readonly ParquetTelemetrySessionWriter _writer;
    private readonly ParquetTelemetrySessionReader _reader;
    private readonly TelemetryHistoryBuffer _liveHistory;
    private readonly long _liveStartTimestamp = Stopwatch.GetTimestamp();
    private IDisposable? _frameSubscription;
    private RecordingContext? _recording;
    private ParquetReplaySession? _replay;
    private long _liveSequence = -1;
    private long _currentElapsedTicks;
    private long _totalElapsedTicks;
    private long _sessionRowCount;
    private long _acceptedRecordingSamples;
    private long _writtenRecordingSamples;
    private UcsiTelemetrySample? _currentSample;
    private long _playbackAnchorTimestamp;
    private long _playbackAnchorElapsedTicks;
    private bool _disposed;

    public TelemetrySessionCoordinator(
        IDecodedTelemetryFrameSource frameSource,
        IGCBDataStore gcbDataStore,
        ParquetTelemetrySessionWriter writer,
        ParquetTelemetrySessionReader reader,
        TelemetryHistoryBuffer liveHistory)
    {
        _frameSource = frameSource;
        _gcbDataStore = gcbDataStore;
        _writer = writer;
        _reader = reader;
        _liveHistory = liveHistory;
    }

    public UcsiMode Mode { get; private set; } = UcsiMode.Live;
    public SessionTransportState TransportState { get; private set; } = SessionTransportState.Idle;
    public TelemetryHistoryBuffer LiveHistory => _liveHistory;
    public UcsiTelemetrySample? CurrentSample
    {
        get
        {
            lock (_gate)
                return _currentSample;
        }
    }
    public long CurrentElapsedTicks => Interlocked.Read(ref _currentElapsedTicks);
    public long TotalElapsedTicks => Interlocked.Read(ref _totalElapsedTicks);
    public long SessionRowCount => Interlocked.Read(ref _sessionRowCount);
    public long AcceptedRecordingSamples => Interlocked.Read(ref _acceptedRecordingSamples);
    public long WrittenRecordingSamples => Interlocked.Read(ref _writtenRecordingSamples);
    public string? CurrentReplayPath => _replay?.Path;
    public string? LastError { get; private set; }
    public double LiveSampleRate
    {
        get
        {
            double seconds = Stopwatch.GetElapsedTime(_liveStartTimestamp).TotalSeconds;
            return seconds <= 0 ? 0 : Math.Max(0, Interlocked.Read(ref _liveSequence) + 1) / seconds;
        }
    }

    public void Start()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        lock (_gate)
            _frameSubscription ??= _frameSource.Subscribe(Publish);
    }

    public void Publish(DecodedTelemetryFrame frame)
    {
        long sequence = Interlocked.Increment(ref _liveSequence);
        long liveElapsedTicks = Stopwatch.GetElapsedTime(_liveStartTimestamp).Ticks;
        IReadOnlyList<FaultEntry> faults = _gcbDataStore.ActiveFaults;
        var sample = new UcsiTelemetrySample(
            sequence,
            frame.ReceivedAtUtc,
            liveElapsedTicks,
            frame.Telemetry,
            faults);
        _liveHistory.Append(sample);

        lock (_gate)
        {
            if (Mode == UcsiMode.Live)
            {
                _currentSample = sample;
                Interlocked.Exchange(ref _currentElapsedTicks, liveElapsedTicks);
                Interlocked.Exchange(ref _totalElapsedTicks, liveElapsedTicks);
                Interlocked.Exchange(ref _sessionRowCount, sequence + 1);
            }
        }

        RecordingContext? recording = Volatile.Read(ref _recording);
        if (recording is null)
            return;

        var envelope = new RecordingEnvelope(
            sample,
            frame.RawDatagram,
            recording.Stopwatch.ElapsedTicks,
            frame.RawDatagram.IsEmpty ? TelemetrySourceKind.Dummy : TelemetrySourceKind.Udp);
        if (recording.Channel.Writer.TryWrite(envelope))
        {
            Interlocked.Increment(ref _acceptedRecordingSamples);
            return;
        }

        _ = FinalizeRecordingAsync(
            recording,
            new IOException("Recording stopped because the telemetry writer could not keep up."));
    }

    public void StartRecording(string completedPath)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        string finalPath = NormalizeCompletedPath(completedPath);
        string partialPath = finalPath + ".partial";
        lock (_gate)
        {
            if (Mode != UcsiMode.Live || TransportState != SessionTransportState.Idle)
                throw new InvalidOperationException("Recording can start only while live and idle.");
            if (File.Exists(partialPath))
                throw new IOException($"Partial recording already exists: {partialPath}");

            Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
            var channel = Channel.CreateBounded<RecordingEnvelope>(new BoundedChannelOptions(RecordingChannelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
                AllowSynchronousContinuations = false,
            });
            var context = new RecordingContext(finalPath, partialPath, channel);
            _recording = context;
            Interlocked.Exchange(ref _acceptedRecordingSamples, 0);
            Interlocked.Exchange(ref _writtenRecordingSamples, 0);
            LastError = null;
            TransportState = SessionTransportState.Recording;
            context.WriterTask = _writer.WriteAsync(
                partialPath,
                DateTimeOffset.UtcNow,
                channel.Reader,
                CancellationToken.None);
            context.Stopwatch.Start();
        }
    }

    public Task StopRecordingAsync()
    {
        RecordingContext? recording = Volatile.Read(ref _recording);
        return recording is null
            ? Task.CompletedTask
            : FinalizeRecordingAsync(recording, failure: null);
    }

    public async Task LoadReplayAsync(string path, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        lock (_gate)
        {
            if (Mode != UcsiMode.Live || TransportState != SessionTransportState.Idle)
                throw new InvalidOperationException("A replay can be loaded only while live and idle.");
            TransportState = SessionTransportState.Loading;
            LastError = null;
        }

        ParquetReplaySession? loaded = null;
        try
        {
            loaded = await _reader.LoadAsync(path, cancellationToken).ConfigureAwait(false);
            ReplayFrame first = await loaded.ReadRowAsync(0, cancellationToken).ConfigureAwait(false);
            lock (_gate)
            {
                _replay = loaded;
                loaded = null;
                Mode = UcsiMode.Replay;
                TransportState = SessionTransportState.Paused;
                _currentSample = first.Sample;
                Interlocked.Exchange(ref _currentElapsedTicks, first.ElapsedTicks);
                Interlocked.Exchange(ref _totalElapsedTicks, _replay.TotalElapsedTicks);
                Interlocked.Exchange(ref _sessionRowCount, _replay.RowCount);
            }
        }
        catch (Exception exception)
        {
            if (loaded is not null)
                await loaded.DisposeAsync().ConfigureAwait(false);
            lock (_gate)
            {
                TransportState = SessionTransportState.Idle;
                LastError = exception.Message;
            }
            throw;
        }
    }

    public async Task TogglePlaybackAsync(CancellationToken cancellationToken = default)
    {
        ParquetReplaySession replay = GetReplay();
        if (TransportState == SessionTransportState.Playing)
        {
            await AdvancePresentationAsync(cancellationToken).ConfigureAwait(false);
            TransportState = SessionTransportState.Paused;
            return;
        }
        if (TransportState != SessionTransportState.Paused)
            throw new InvalidOperationException("Playback can toggle only while replay is paused or playing.");

        if (CurrentElapsedTicks >= replay.TotalElapsedTicks)
            await SeekAsync(0, cancellationToken).ConfigureAwait(false);
        _playbackAnchorElapsedTicks = CurrentElapsedTicks;
        _playbackAnchorTimestamp = Stopwatch.GetTimestamp();
        TransportState = SessionTransportState.Playing;
    }

    public async Task SeekAsync(long elapsedTicks, CancellationToken cancellationToken = default)
    {
        ParquetReplaySession replay = GetReplay();
        TransportState = SessionTransportState.Paused;
        long target = Math.Clamp(elapsedTicks, 0, replay.TotalElapsedTicks);
        ReplayFrame frame = await replay.ReadAtOrBeforeAsync(target, cancellationToken).ConfigureAwait(false);
        SetCurrentSample(frame.Sample);
        Interlocked.Exchange(ref _currentElapsedTicks, frame.ElapsedTicks);
    }

    public async Task ReturnToLiveAsync()
    {
        ParquetReplaySession? replay;
        lock (_gate)
        {
            if (Mode != UcsiMode.Replay)
                return;
            replay = _replay;
            _replay = null;
            Mode = UcsiMode.Live;
            TransportState = SessionTransportState.Idle;
            UcsiTelemetrySample? latest = _liveHistory.Latest();
            _currentSample = latest;
            long elapsed = latest?.LiveElapsedTicks ?? 0;
            Interlocked.Exchange(ref _currentElapsedTicks, elapsed);
            Interlocked.Exchange(ref _totalElapsedTicks, elapsed);
            Interlocked.Exchange(ref _sessionRowCount, (latest?.LiveSequence ?? -1) + 1);
        }
        if (replay is not null)
            await replay.DisposeAsync().ConfigureAwait(false);
    }

    public async Task AdvancePresentationAsync(CancellationToken cancellationToken = default)
    {
        if (Mode != UcsiMode.Replay || TransportState != SessionTransportState.Playing)
            return;
        ParquetReplaySession replay = GetReplay();
        long target = _playbackAnchorElapsedTicks + Stopwatch.GetElapsedTime(_playbackAnchorTimestamp).Ticks;
        if (target >= replay.TotalElapsedTicks)
        {
            ReplayFrame last = await replay.ReadRowAsync(replay.RowCount - 1, cancellationToken).ConfigureAwait(false);
            SetCurrentSample(last.Sample);
            Interlocked.Exchange(ref _currentElapsedTicks, last.ElapsedTicks);
            TransportState = SessionTransportState.Paused;
            return;
        }

        ReplayFrame frame = await replay.ReadAtOrBeforeAsync(target, cancellationToken).ConfigureAwait(false);
        SetCurrentSample(frame.Sample);
        Interlocked.Exchange(ref _currentElapsedTicks, frame.ElapsedTicks);
    }

    public Task<IReadOnlyList<ReplayGraphSeries>> ReadReplayGraphSeriesAsync(
        IReadOnlyList<string> parameterIds,
        long startElapsedTicks,
        long endElapsedTicks,
        int maximumPointsPerSeries,
        CancellationToken cancellationToken = default) =>
        GetReplay().ReadGraphSeriesAsync(
            parameterIds,
            startElapsedTicks,
            endElapsedTicks,
            maximumPointsPerSeries,
            cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        _frameSubscription?.Dispose();
        _frameSubscription = null;
        RecordingContext? recording = Volatile.Read(ref _recording);
        if (recording is not null)
            await FinalizeRecordingAsync(recording, failure: null).ConfigureAwait(false);
        ParquetReplaySession? replay = _replay;
        _replay = null;
        if (replay is not null)
            await replay.DisposeAsync().ConfigureAwait(false);
    }

    private async Task FinalizeRecordingAsync(RecordingContext context, Exception? failure)
    {
        if (Interlocked.Exchange(ref context.FinalizationStarted, 1) != 0)
        {
            await context.Completion.Task.ConfigureAwait(false);
            return;
        }

        context.Stopwatch.Stop();
        context.Channel.Writer.TryComplete(failure);
        try
        {
            ParquetWriteResult result = await context.WriterTask.ConfigureAwait(false);
            Interlocked.Exchange(ref _writtenRecordingSamples, result.WrittenCount);
            if (failure is not null)
                throw failure;
            if (!result.FooterCompleted || result.WrittenCount == 0)
                throw new InvalidOperationException("No full-rate telemetry samples were recorded.");

            File.Move(context.PartialPath, context.CompletedPath, overwrite: true);
            lock (_gate)
            {
                LastError = null;
                TransportState = SessionTransportState.Idle;
                if (ReferenceEquals(_recording, context))
                    _recording = null;
            }
            context.Completion.TrySetResult();
        }
        catch (Exception exception)
        {
            if (exception is InvalidOperationException && File.Exists(context.PartialPath) && new FileInfo(context.PartialPath).Length == 0)
                File.Delete(context.PartialPath);
            lock (_gate)
            {
                LastError = exception.Message;
                TransportState = SessionTransportState.Idle;
                if (ReferenceEquals(_recording, context))
                    _recording = null;
            }
            context.Completion.TrySetException(exception);
            if (failure is null)
                throw;
        }
    }

    private void SetCurrentSample(UcsiTelemetrySample sample)
    {
        lock (_gate)
            _currentSample = sample;
    }

    private ParquetReplaySession GetReplay() =>
        _replay ?? throw new InvalidOperationException("No replay session is loaded.");

    private static string NormalizeCompletedPath(string path)
    {
        string fullPath = Path.GetFullPath(path);
        if (!string.Equals(Path.GetExtension(fullPath), ".parquet", StringComparison.OrdinalIgnoreCase))
            fullPath += ".parquet";
        return fullPath;
    }

    private sealed class RecordingContext(
        string completedPath,
        string partialPath,
        Channel<RecordingEnvelope> channel)
    {
        public string CompletedPath { get; } = completedPath;
        public string PartialPath { get; } = partialPath;
        public Channel<RecordingEnvelope> Channel { get; } = channel;
        public Stopwatch Stopwatch { get; } = new();
        public Task<ParquetWriteResult> WriterTask { get; set; } = Task.FromResult(new ParquetWriteResult(0, false));
        public TaskCompletionSource Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int FinalizationStarted;
    }
}
