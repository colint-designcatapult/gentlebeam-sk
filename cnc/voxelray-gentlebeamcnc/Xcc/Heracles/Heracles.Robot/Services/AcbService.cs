using Empyrean.Common.Infra.Networking.Udp;

using Heracles.Core.Enums;
using Heracles.Core.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Heracles.Robot.Services
{
    public class AcbSafeService : AcbService, IAcbService, IDisposable
    {
        public AcbSafeService(IAppGlobals appGlobals, IAcbMessageConverter messageGenerator, IAcbCommunicationService communicationService, ILogRepository logWriter)
            : base(appGlobals, messageGenerator, communicationService, logWriter)
        {
            _logWriter = logWriter;
        }

        public new async Task<bool> SendCommand(AcbActuatorId actuatorId, AcbActuatorCommand actuatorCommand)
        {
            // if unlock
            // check is safe, if not return
            // do
            if (isSafe(actuatorId, actuatorCommand) == false)
            {
                _ = _logWriter.LogAsync($"AcbSafeService.SendCommand: actuatorId={actuatorId.ToString()}, actuatorCommand={actuatorCommand.ToString()} ignored for safety reasons", Xcc.Core.Enums.LogRecordSeverity.Warn, Xcc.Core.Enums.LogRecordType.System);
                return false;
            }
            return await base.SendCommand(actuatorId, actuatorCommand);
        }
        // isSafe()
        bool isSafe(AcbActuatorId actuatorId, AcbActuatorCommand actuatorCommand)
        {
            if (actuatorId != AcbActuatorId.Robot || actuatorCommand != AcbActuatorCommand.Unlock)
            {
                return true;
            }

            var imageLightSensorState = ImageActuator.LightSensorState;
            var imageProxySensorState = ImageActuator.ProxySensorState;
            var treatmentLightSensorState = TreatmentActuator.LightSensorState;
            var treatmentProxySensorState = TreatmentActuator.ProxySensorState;

            bool isItSafe =
                (imageLightSensorState == AcbLightSensorState.NotInterrpupt && treatmentLightSensorState == AcbLightSensorState.NotInterrpupt) // No head mounted
                ||
                (imageLightSensorState == AcbLightSensorState.Interrupt && imageProxySensorState == AcbProxySensorState.Detected) // Imaging head mounted & on cardle
                ||
                (treatmentLightSensorState == AcbLightSensorState.Interrupt && treatmentProxySensorState == AcbProxySensorState.Detected); // Treatment head mounted & on cardle

            return isItSafe;

        }

        ILogRepository _logWriter;
    }

    public class AcbActuatorStatusResponseFiltered
    {
        const int MEDIAN_FILTER_APPERTURE = 5;

        public AcbActuatorStatusResponse? AcbActuatorStatusResponse
        {
            get
            {
                lock (_lockObject)
                {
                    if (_statuses.Count < MEDIAN_FILTER_APPERTURE)
                    {
                        return null;
                    }
                    AcbActuatorStatusResponse status = new AcbActuatorStatusResponse();
                    // Get timestamp from the oldest
                    status.Timestamp = _statuses.First().Timestamp;
                    // Apply median filter for responces for each actuator
                    status.ActuatorStates[AcbActuatorId.Robot] = GetActuatorStateInfoFiltered(AcbActuatorId.Robot);
                    status.ActuatorStates[AcbActuatorId.Image] = GetActuatorStateInfoFiltered(AcbActuatorId.Image);
                    status.ActuatorStates[AcbActuatorId.Treatment] = GetActuatorStateInfoFiltered(AcbActuatorId.Treatment);
                    return status;
                }
            }
            set
            {
                if (value.HasValue)
                {
                    lock (_lockObject)
                    {
                        _statuses.Enqueue(value.Value);
                        if (_statuses.Count > MEDIAN_FILTER_APPERTURE)
                        {
                            _ = _statuses.Dequeue();
                        }
                    }
                }
            }
        }
        private ActuatorStateInfo GetActuatorStateInfoFiltered(AcbActuatorId acbActuatorId)
        {
            var actuatorStates = new List<AcbActuatorState>();
            var lightSensorStates = new List<AcbLightSensorState>();
            var proxySensorStates = new List<AcbProxySensorState>();
            var footPedalState = new List<AcbFootPedalState>();
            foreach (var item in _statuses)
            {
                var actuator = item.ActuatorStates[acbActuatorId];
                actuatorStates.Add(actuator.ActuatorState);
                lightSensorStates.Add(actuator.LightSensorState);
                proxySensorStates.Add(actuator.ProxySensorState);
                footPedalState.Add(actuator.FootPedalState);
            }
            actuatorStates.Sort();
            lightSensorStates.Sort();
            proxySensorStates.Sort();
            footPedalState.Sort();

            return new ActuatorStateInfo()
            {
                ActuatorState = actuatorStates[actuatorStates.Count / 2],
                LightSensorState = lightSensorStates[lightSensorStates.Count / 2],
                ProxySensorState = proxySensorStates[proxySensorStates.Count / 2],
                FootPedalState = footPedalState[footPedalState.Count / 2]
            };
        }

        private Queue<AcbActuatorStatusResponse> _statuses = new Queue<AcbActuatorStatusResponse>();
        private Object _lockObject = new();
    }


    /// <summary>
    /// Actuator communication board service
    /// </summary>
    public class AcbService : IAcbService, IDisposable
    {
        const int ACB_POLLING_INTERVAL = 50;
        const int ACB_STATUS_EXPIRATION_INTERVAL = 2000; // > ACB_SEND_REQUEST_TIMEOUT_MS*ACB_SEND_ATTEMPTS + ACB_POLLING_INTERVAL
        const int ACB_SEND_REQUEST_TIMEOUT_MS = 250;
        const int ACB_COMMAND_EXECUTION_TIMEOUT = 15000;
        const int ACB_SEND_COMMAND_ATTEMPTS = 3; // Actuator Lock/Unlock 

        private Task PollingProcess { get; set; }
        private CancellationToken GlobalToken { get; }
        private CancellationTokenSource PollingProcessCts { get; set; } = new();
        private AcbActuatorStatusResponseFiltered AcbActuatorStatusResponseFiltered = new();

        AcbActuatorStatusResponse? acbActuatorStatusResponse = null;
        //AcbActuatorStatusResponse? acbActuatorStatusResponse
        //{
        //    get => AcbActuatorStatusResponseFiltered.AcbActuatorStatusResponse;
        //    set => AcbActuatorStatusResponseFiltered.AcbActuatorStatusResponse = value;
        //}
        public AcbActuatorState RobotActuator
        {
            get => GetActuatorInfo(acbActuatorStatusResponse, AcbActuatorId.Robot).State;
        }
        public ActuatorWithSensorsInfo ImageActuator
        {
            get => GetActuatorInfo(acbActuatorStatusResponse, AcbActuatorId.Image);
        }
        public ActuatorWithSensorsInfo TreatmentActuator
        {
            get => GetActuatorInfo(acbActuatorStatusResponse, AcbActuatorId.Treatment);
        }
        public AcbFootPedalState PedalState
        {
            get => GetPedalInfo(acbActuatorStatusResponse);
        }

        public AcbActuatorState GetActuatorState(AcbActuatorId actuatorId)
        {
            return actuatorId switch
            {
                AcbActuatorId.Robot => RobotActuator,
                AcbActuatorId.Image => ImageActuator.State,
                AcbActuatorId.Treatment => TreatmentActuator.State,
                _ => throw new ArgumentException(actuatorId.ToString())
            };
        }
        public AcbLightSensorState GetLightSensorState(AcbActuatorId actuatorId)
        {
            return actuatorId switch
            {
                AcbActuatorId.Image => ImageActuator.LightSensorState,
                AcbActuatorId.Treatment => TreatmentActuator.LightSensorState,
                _ => throw new ArgumentException(actuatorId.ToString())
            };
        }
        public AcbProxySensorState GetProxySensorState(AcbActuatorId actuatorId)
        {
            return actuatorId switch
            {
                AcbActuatorId.Image => ImageActuator.ProxySensorState,
                AcbActuatorId.Treatment => TreatmentActuator.ProxySensorState,
                _ => throw new ArgumentException(actuatorId.ToString())
            };
        }

        public AcbService(
            IAppGlobals appGlobals,
            IAcbMessageConverter messageGenerator,
            IAcbCommunicationService communicationService,
            ILogRepository logWriter)
        {
            GlobalToken = appGlobals.AppCancellationTokenSource.Token;
            MessageGenerator = messageGenerator;
            CommunicationService = communicationService;
            _logWriter = logWriter;

            CommunicationService.UdpReceiveErrorEvent += (s, e) =>
            {
                string msg = e.Exception?.Message ?? string.Empty;
                msg += e.Message;

                _ = _logWriter.LogAsync(msg, LogRecordSeverity.Error, LogRecordType.System);
            };
        }

        public IAcbMessageConverter MessageGenerator { get; }
        public IAcbCommunicationService CommunicationService { get; }

        bool isExpectedEqualsCurrent(AcbActuatorId actuatorId, AcbActuatorCommand actuatorCommand)
        {
            AcbActuatorState expectedState = (actuatorCommand == AcbActuatorCommand.Lock) ? AcbActuatorState.Lock : AcbActuatorState.Unlock;
            var currentState = actuatorId switch
            {
                AcbActuatorId.Image => ImageActuator.State,
                AcbActuatorId.Treatment => TreatmentActuator.State,
                AcbActuatorId.Robot => RobotActuator,
                _ => throw new InvalidOperationException("Unknown actuator Id")
            };
            return expectedState == currentState;
        }

        public async Task<bool> SendCommand(AcbActuatorId actuatorId, AcbActuatorCommand actuatorCommand)
        {
            if (isExpectedEqualsCurrent(actuatorId, actuatorCommand))
            {
                return true;
            }

            var msg = MessageGenerator.GenerateActuatorCommandMessage(actuatorId, actuatorCommand);
            // We dont send request directly to avoid races between this command and polling, so just enqueue request.
            // It will be processed in polling loop
            QueuedMessage request = new QueuedMessage { Message = msg, Status = QueuedMessageStatus.Awaiting };

            _requestsQueue.Enqueue(request);

            // We run a task to wait for necessary actuator status:
            return await Task.Run(async () =>
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                while (stopwatch.ElapsedMilliseconds < ACB_COMMAND_EXECUTION_TIMEOUT)
                {
                    if (isExpectedEqualsCurrent(actuatorId, actuatorCommand))
                    {
                        return true;
                    }
                    else if (request.Status == QueuedMessageStatus.FailedToDeliver)
                    {
                        return false;
                    }

                    await Task.Delay(50);
                }
                return false;
            });
        }


        public async Task<AcbActuatorStatusResponse?> SendPollRequest()
        {
            var msg = MessageGenerator.GenerateActuatorStatusPollMessage();

            var response = await TrySendRequestAsync(msg, 1);

            return (response == null) ? null : MessageGenerator.ParseStatusPollResponse(response);
        }

        private Task RunStatusPollingProcess(CancellationToken cancellationToken)
        {
            return Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // User commands have top priority, so we check request queue first
                    if (_requestsQueue.TryDequeue(out var request))
                    {
                        var result = await TrySendRequestAsync(request.Message, ACB_SEND_COMMAND_ATTEMPTS);
                        request.Status = (result is null) ? QueuedMessageStatus.FailedToDeliver : QueuedMessageStatus.Complete;
                    }
                    else
                    {
                        // If there is no user command, send polling
                        var response = await SendPollRequest();
                        if (response != null)
                        {
                            acbActuatorStatusResponse = response;
                        }
                    }
                    // Wait timeout before send the new request
                    await Task.Delay(ACB_POLLING_INTERVAL, cancellationToken);
                    Updated?.Invoke(this, null);
                }
            });
        }

        public void StartListening()
        {
            CommunicationService.Start();

            if (PollingProcess is not null && !PollingProcess.IsCompleted)
            {
                throw new InvalidOperationException("Cannot start ACB Service, it is already running");
            }

            _ = _logWriter.LogAsync("Acb communication service: Start", LogRecordSeverity.Info, LogRecordType.System);

            PollingProcessCts = CancellationTokenSource.CreateLinkedTokenSource(GlobalToken);
            PollingProcess = RunStatusPollingProcess(PollingProcessCts.Token);
        }

        public void StopListening()
        {
            PollingProcessCts?.Cancel();
            CommunicationService.Stop();
        }

        public Task<bool> PingAsync()
        {
            return CommunicationService.PingAsync();
        }

        public event EventHandler Updated;

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    PollingProcessCts.Cancel();
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

        private ActuatorWithSensorsInfo GetActuatorInfo(AcbActuatorStatusResponse? status, AcbActuatorId actuatorId)
        {
            if (AcbActuatorStatusResponse.IsNullOrExpired(status, ACB_STATUS_EXPIRATION_INTERVAL))
            {
                return new ActuatorWithSensorsInfo();
            }
            else
            {
                return new ActuatorWithSensorsInfo(status.Value.ActuatorStates[actuatorId]);
            }
        }

        private AcbFootPedalState GetPedalInfo(AcbActuatorStatusResponse? status)
        {
            if (AcbActuatorStatusResponse.IsNullOrExpired(status, ACB_STATUS_EXPIRATION_INTERVAL))
            {
                return AcbFootPedalState.Unknown;
            }
            else
            {
                return status.Value.ActuatorStates[AcbActuatorId.Image].FootPedalState;
            }
        }

        private async Task<byte[]> TrySendRequestAsync(byte[] request, int attempts)
        {
            byte[]? response = null;

            for (int i = 1; i <= attempts; ++i)
            {
                try
                {
                    response = await CommunicationService.SendRequestAsync(request, ACB_SEND_REQUEST_TIMEOUT_MS);
                }
                catch (UdpException ex) // added according to UdpClient logic changes
                {
                    response = null;
                }

                if (response != null)
                {
                    _noConnection = false;
                    return response;
                }

                if (!_noConnection)
                {
                    _ = _logWriter.LogAsync(
                        $"AcbService.TrySendRequestAsync: send request failed, request {BitConverter.ToString(request)}, attempt {i.ToString()} of {attempts.ToString()}",
                        Xcc.Core.Enums.LogRecordSeverity.Error,
                        Xcc.Core.Enums.LogRecordType.System);
                }
            }

            if (!_noConnection)
            {
                _ = _logWriter.LogAsync($"AcbService: No connection",
                    Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.System);
                _noConnection = true; // We failed to deliver the request or get the response completely, so mark as no connection
            }
            return null;
        }

        private enum QueuedMessageStatus
        {
            Awaiting = 0,
            Complete = 1,
            FailedToDeliver = 2,
        }
        private class QueuedMessage
        {
            public byte[] Message { get; set; }
            public QueuedMessageStatus Status { get; set; }
        }
        private ConcurrentQueue<QueuedMessage> _requestsQueue = new();
        ILogRepository _logWriter;
        private bool _noConnection;
    }
}
