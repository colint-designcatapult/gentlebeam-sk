using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Core.Enums;
using Xcc.Core.Models;

namespace Heracles.Ucsi.Services;

public interface IUcsiKeepaliveService
{
    void Start();
}

public sealed class UcsiKeepaliveService(
    IGcbXRayCommandOperator commandOperator,
    IGcbCommunicationService communicationService,
    IAppGlobals appGlobals,
    UcsiLogBuffer logBuffer) : IUcsiKeepaliveService, IAsyncDisposable
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
    private readonly object _gate = new();
    private CancellationTokenSource? _cancellation;
    private Task? _loopTask;

    public void Start()
    {
        lock(_gate)
        {
            if(_loopTask is not null)
                return;

            _cancellation = CancellationTokenSource.CreateLinkedTokenSource(
                appGlobals.AppCancellationTokenSource.Token);
            _loopTask = RunAsync(_cancellation.Token);
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(Interval);
        bool failureLogged = false;

        try
        {
            while(await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    byte[] packet = commandOperator.GenerateVersionInfoRequestCmd();
                    await communicationService.SendRequestAsync(packet, timeoutMs: 500).ConfigureAwait(false);
                    failureLogged = false;
                }
                catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                catch(Exception exception)
                {
                    if(!failureLogged)
                    {
                        logBuffer.Log(
                            $"UCSI keepalive failed: {exception.Message}",
                            LogRecordSeverity.Warn,
                            LogRecordType.System);
                        failureLogged = true;
                    }
                }
            }
        }
        catch(OperationCanceledException) when(cancellationToken.IsCancellationRequested)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        Task? loopTask;
        CancellationTokenSource? cancellation;
        lock(_gate)
        {
            loopTask = _loopTask;
            cancellation = _cancellation;
            _loopTask = null;
            _cancellation = null;
        }

        if(cancellation is null)
            return;

        cancellation.Cancel();
        if(loopTask is not null)
            await loopTask.ConfigureAwait(false);
        cancellation.Dispose();
    }
}
