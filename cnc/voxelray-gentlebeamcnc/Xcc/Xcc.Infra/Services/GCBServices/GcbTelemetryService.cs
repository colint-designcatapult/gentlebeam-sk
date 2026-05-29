using System;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Networking;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Core.Services;
using Xcc.Infra.GryphonBoard;
using SystemTelemetry = Xcc.Infra.GryphonBoard.SystemTelemetry;

namespace Xcc.Infra.Services.GcbServices
{
    [Obsolete]
    public interface IGcbTelemetryConnectionFactory
    {
        IAsyncClientConnection GetGcbTelemetryConnection();
    }

    [Obsolete]
    public class GcbTelemetryService : ITelemetryService
    {
        private const int TELEMETRY_EXPIRATION_TIMEOUT = 1500;
        private const int TELEMETRY_SERVICE_INTERVAL = 250;

        public TelemetryServiceMode Mode { get; private set; } = TelemetryServiceMode.None;
        private IGcbXRayCommandOperator GcbXRayCommandOperator { get; }
        public ISystemTelemetryChanged SystemTelemetryChangedCallback { get; }
        public ILogRepository LogWriter { get; }
        private IAsyncClientConnection Connection { get; }

        private Task? ReceiveProcess { get; set; }
        private Task? RequestProcess { get; set; }
        private CancellationToken GlobalToken { get; } 
        private CancellationTokenSource ReceiveProcessCts { get; set; } = new();
        private CancellationTokenSource RequestProcessCts { get; set; } = new();

        readonly object _expirationTimerLock = new();
        private Timer ExpirationTimer { get; } // telemetry resets when the timer expires

        public GcbTelemetryService(
            IAppGlobals appGlobals,
            IGcbXRayCommandOperator gcbXRayCommandOperator,
            IGcbTelemetryConnectionFactory gcbTelemetryConnectionFactory,
            ISystemTelemetryChanged systemTelemetryChangedCallback,
            ILogRepository logWriter)
        {
            GlobalToken = appGlobals.AppCancellationTokenSource.Token;
            GcbXRayCommandOperator = gcbXRayCommandOperator;
            SystemTelemetryChangedCallback = systemTelemetryChangedCallback;
            LogWriter = logWriter;
            Connection = gcbTelemetryConnectionFactory.GetGcbTelemetryConnection();
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
                throw new InvalidOperationException("Cannot start Telemetry Service, it is already running");
            }
            
            RequestProcessCts = CancellationTokenSource.CreateLinkedTokenSource(GlobalToken);
            RequestProcess = RequestTelemetryProcess(RequestProcessCts.Token);
        }

        private void StartReceiveProcess()
        {
            if (ReceiveProcess is not null && !ReceiveProcess.IsCompleted)
            {
                throw new InvalidOperationException("Cannot start Telemetry Service, it is already running");
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
                bool storeExceptionToLog = true;
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
                        storeExceptionToLog = true; // Now we got at least one acceptable response, can write exceptions again
                    }
                    catch (Exception ex)
                    {
                        if (storeExceptionToLog)
                        {
                            _ = LogWriter.LogAsync($"GCB telemetry receive error: {ex.Message}", Core.Enums.LogRecordSeverity.Error, Core.Enums.LogRecordType.System);
                            storeExceptionToLog = false; // To not overload the log with consequent exceptions
                        }
                    }
                }
            }, cancellationToken);
        }

        private void ResetTelemetry(object? state)
        {
            SystemTelemetryChangedCallback.OnSystemTelemetryChanged(null!);
            _ = LogWriter.LogAsync($"GCB telemetry service: failed to receive telemetry.", Core.Enums.LogRecordSeverity.Error, Core.Enums.LogRecordType.System);
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
    }
}
