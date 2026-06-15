using Empyrean.Common.Infra.Networking;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard.Comm.Udp;

namespace Xcc.Infra.GryphonBoard.Comm
{

    public class GcbTelemetryService : ITelemetryService
    {
        private const int TELEMETRY_EXPIRATION_TIMEOUT = 1500;
        private const int TELEMETRY_SERVICE_INTERVAL = 250;

        public TelemetryServiceMode Mode { get; private set; } = TelemetryServiceMode.None;
        private IGcbXRayCommandOperator GcbXRayCommandOperator { get; }
        public ILogWriter LogWriter { get; }
        private IAsyncClientConnection Connection { get; }
        public ISystemTelemetryChanged SystemTelemetryChangedCallback { get; }
        private Task? ReceiveProcess { get; set; }
        private Task? RequestProcess { get; set; }
        private CancellationToken GlobalToken { get; }
        private CancellationTokenSource ReceiveProcessCts { get; set; } = new();
        private CancellationTokenSource RequestProcessCts { get; set; } = new();
        private Timer ExpirationTimer { get; } // telemetry resets when the timer expires

        public GcbTelemetryService(
            IAppGlobals appGlobals,
            IGcbXRayCommandOperator gcbXRayCommandOperator,
            IGcbTelemetryConnection gcbTelemetryConnection,
            ISystemTelemetryChanged systemTelemetryChangedCallback,
            ILogWriter logWriter)
        {
            GlobalToken = appGlobals.AppCancellationTokenSource.Token;
            GcbXRayCommandOperator = gcbXRayCommandOperator;
            LogWriter = logWriter;
            Connection = gcbTelemetryConnection;
            SystemTelemetryChangedCallback = systemTelemetryChangedCallback;
            ExpirationTimer = new(ResetTelemetry);
        }


        public void Start(TelemetryServiceMode mode)
        {
            StartReceiveProcess();
            if (mode == TelemetryServiceMode.Active)
            {
                StartRequestProcess();
            }
        }

        public void Stop()
        {
            RequestProcessCts?.Cancel();
            ReceiveProcessCts?.Cancel();
        }

        private void StartRequestProcess()
        {
            if (RequestProcess is not null && !RequestProcess.IsCompleted)
            {
                throw new InvalidOperationException("Cannot start Telemetery Service, it is already running");
            }

            RequestProcessCts = CancellationTokenSource.CreateLinkedTokenSource(GlobalToken);
            RequestProcess = RequestTelemetryProcess(RequestProcessCts.Token);
        }

        private void StartReceiveProcess()
        {
            if (ReceiveProcess is not null && !ReceiveProcess.IsCompleted)
            {
                throw new InvalidOperationException("Cannot start Telemetery Service, it is already running");
            }

            ReceiveProcessCts = CancellationTokenSource.CreateLinkedTokenSource(GlobalToken);
            ReceiveProcess = ReceiveTelemetryProcess(ReceiveProcessCts.Token);
        }

        private Task RequestTelemetryProcess(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                byte[] txData = GcbXRayCommandOperator.GenerateTelemetryRequestCmd();

                while (!cancellationToken.IsCancellationRequested)
                {
                    //wait timeout before send the new request
                    await Connection.SendAsync(txData);

                    await Task.Delay(TELEMETRY_SERVICE_INTERVAL, cancellationToken);
                }
            });
        }


        private Task ReceiveTelemetryProcess(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await Connection.ReceiveAsync(cancellationToken);

                    try
                    {
                        if (result.Length > 0)
                        {
                            SystemTelemetryChangedCallback.OnSystemTelemetryChanged(SystemTelemetry.Parse(result));

                            lock (_expirationTimerLock)
                            {
                                ExpirationTimer.Change(TELEMETRY_EXPIRATION_TIMEOUT, 0); //restart expiration timer
                            }
                        }
                        _storeExceptionToLog = true; // Now we got at least one acceptable response, can write exceptions again
                    }
                    catch (Exception ex)
                    {
                        if (_storeExceptionToLog)
                        {
                            _ = LogWriter.LogAsync($"GCB telemetry receive error: {ex.Message}", Core.Enums.LogRecordSeverity.Error, Core.Enums.LogRecordType.System);
                            _storeExceptionToLog = false; // To not overload the log with consequent exceptions
                        }
                    }
                }
            }, cancellationToken);
        }

        private void ResetTelemetry(object? state)
        {
            SystemTelemetryChangedCallback.OnSystemTelemetryChanged(null!);
            if (!_storeExceptionToLog || _isLogWriting)
                return;

            _isLogWriting = true;

            _ = Task.Run(async () =>
            {
                _ = LogWriter.LogAsync($"GCB telemetry service: failed to receive telemetry.", Core.Enums.LogRecordSeverity.Error, Core.Enums.LogRecordType.System);
                await Task.Delay(3000);
                _isLogWriting = false;
            });
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    ExpirationTimer.Dispose();
                    RequestProcessCts.Cancel();
                    ReceiveProcessCts.Cancel();
                    Connection.Dispose();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
        
        private readonly object _expirationTimerLock = new();
        private bool _isLogWriting;
        private bool _storeExceptionToLog = true;
    }
}
