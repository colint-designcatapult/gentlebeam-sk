using Grpc.Core;

namespace Heracles.Indoor.SqliteGrpcServer.Infrastructure;

/// <summary>Pauses whole unary RPCs, rather than individual repository connections.</summary>
internal sealed class DatabaseMaintenanceGate
{
    private readonly object _sync = new();
    private int _active;
    private bool _paused;
    private bool _stopped;
    private TaskCompletionSource? _drained;

    public IDisposable EnterUnary()
    {
        lock (_sync)
        {
            ThrowIfUnavailable();
            _active++;
            return new Lease(ExitUnary);
        }
    }

    public void CheckStreamingAdmission()
    {
        lock (_sync)
            ThrowIfUnavailable();
    }

    public async Task<IDisposable> PauseAsync(CancellationToken cancellationToken)
    {
        Task drain;
        lock (_sync)
        {
            ThrowIfUnavailable();
            _paused = true;
            drain = _active == 0
                ? Task.CompletedTask
                : (_drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
        }

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            await drain.WaitAsync(timeout.Token).ConfigureAwait(false);
            lock (_sync)
            {
                if (_stopped)
                    throw Unavailable();
            }
            return new Lease(Resume);
        }
        catch
        {
            Resume();
            throw;
        }
    }

    public void Stop()
    {
        lock (_sync)
        {
            _stopped = true;
            _paused = true;
        }
    }

    private void ExitUnary()
    {
        lock (_sync)
        {
            if (--_active == 0)
                _drained?.TrySetResult();
        }
    }

    private void Resume()
    {
        lock (_sync)
        {
            _paused = _stopped;
            _drained = null;
        }
    }

    private void ThrowIfUnavailable()
    {
        if (_paused || _stopped)
            throw Unavailable();
    }

    private static RpcException Unavailable() =>
        new(new Status(StatusCode.Unavailable, "Database maintenance is in progress."));

    private sealed class Lease(Action release) : IDisposable
    {
        private Action? _release = release;
        public void Dispose() => Interlocked.Exchange(ref _release, null)?.Invoke();
    }
}
