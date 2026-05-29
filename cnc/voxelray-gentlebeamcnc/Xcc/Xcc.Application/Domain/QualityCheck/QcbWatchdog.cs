using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.QualityCheck;

namespace Xcc.Application.Domain.QualityCheck
{
    public class QcbWatchdog
    {
        private readonly IQcbService _qcbService;
        private Task? _watchdogTask;
        private CancellationTokenSource? _cancellationTokenSource;

        public bool? IsAlive { get; private set; }
        public QcbWatchdog(IQcbService qcbService)
        {
            _qcbService = qcbService;
        }

        public Task StartWatch(CancellationToken cancellationToken, int retryTimeoutMs, int expirationIntervalMs)
        {
            Debug.WriteLine($"Qcb watchdog is started");
            IsAlive = null;
            if (_watchdogTask != null || _cancellationTokenSource != null)
            {
                Debug.WriteLine("QcbWatchdog error: is already watching");
                throw new Exception("QcbWatchdog is already watching");
            }

            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _cancellationTokenSource.Token;
            return _watchdogTask = Task.Run(async () =>
            {
                try
                {
                    Stopwatch retryStopwatch = Stopwatch.StartNew();
                    while (!token.IsCancellationRequested)
                    {
                        // do not send Ping too often
                        while (retryStopwatch.ElapsedMilliseconds < retryTimeoutMs)
                        {
                            await Task.Delay(50, token);
                        }

                        retryStopwatch.Restart();
                        bool isAlive = await _qcbService.PingBoardAsync();

                        token.ThrowIfCancellationRequested();

                        if (isAlive)
                        {
                            IsAlive = true;
                            Debug.WriteLine($"Qcb is alive");
                        }
                        else
                        {
                            // we missed ping and expiration interval for the response was exceeded
                            IsAlive = false;
                            Debug.WriteLine($"Qcb is NOT alive");
                            break;
                        }
                    }
                }
                finally
                {
                    IsAlive = false;
                }
            }, token);
        }

        public void StopWatch()
        {
            Debug.WriteLine($"Qcb watchdog is stopped");
            IsAlive = null;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = null;
            _watchdogTask = null;
        }
    }
}
