using Heracles.Application.Services;
using Heracles.Core.Models;
using System;
using System.IO;
using Empyrean.Common.Infra.Networking;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Core.Logging;
using Xcc.Infra.Networking.Udp;

namespace Heracles.Application.Models.Supervision
{
    public class NetworkConnectionSupervisor 
        : IGcbTelemetryConnectionFactory
        , IGcbCommandConnectionFactory
        , IQcbCommConnectionFactory
    {
        private readonly ISystemSettingsStore settingsStore;
        private readonly ILogWriter _logWriter;
        private readonly IDebugSettings _debugSettings;
        private IEndPointsConfiguration endPointsConfiguration;
        private readonly object connectionLockObject = new object();
        // Managed connections:
        private IUdpClientConnection gcbTelemetryConnection;
        private IUdpClientConnection gcbCommandConnection;
        private IUdpClientConnection qcbCommConnection;
        private const bool recordTelemetryData = true;
        private const bool recordGcbCommandsData = true;
        private const bool recordQcbCommandsData = true;

        // We predefine these values now, as there's an issue with GCB/QCB network stack,
        // putting them in trouble when a new client connects - they can serve one connection only.
        // So we want to have the same port numbers on app restart,
        // to not mess with the boards with random values assigned by OS
        private const int DebugQcbCommandClientPort = 22223;

        public NetworkConnectionSupervisor(
            ISystemSettingsStore settingsStore,
            ILogWriter logWriter,
            IDebugSettings debugSettings)
        {
            this.settingsStore = settingsStore;
            this._logWriter = logWriter;
            _debugSettings = debugSettings;
            endPointsConfiguration = settingsStore.Settings?.EndPointsConfiguration;
            settingsStore.PropertyChanged += (s, e) => OnSettingsUpdate();
        }

        public IAsyncClientConnection GetGcbTelemetryConnection()
        {
            lock (connectionLockObject)
            {
                if (gcbTelemetryConnection == null && endPointsConfiguration?.GCBTelemetryEndPoint != null)
                {
                    // Since this is a pure listener connection, we do not need to worry about a loop
                    // Just listen to the telemetry endpoint

                    var telemetryEndpoint = endPointsConfiguration.GCBTelemetryEndPoint;
                    int configuredListenerPort = _debugSettings.GcbTelemetryListenerPort;
                    int telemetryClientPort = configuredListenerPort > 0
                        ? configuredListenerPort
                        : telemetryEndpoint.Port.Value;

                    gcbTelemetryConnection = new UdpClientConnection(
                        telemetryEndpoint.Ip(),
                        telemetryEndpoint.Port.Value,
                        telemetryClientPort,
                        reusePort: configuredListenerPort == 0);
                }

                if (recordTelemetryData)
                {
                    gcbTelemetryConnection = AddLoggingProxy(
                        gcbTelemetryConnection, 
                        subfolderName: "GcbTelemetry", 
                        timeoutMs:0, 
                        newFileEvery: UdpConnectionLoggingProxy.NewFileEvery.Hour);
                }

                return gcbTelemetryConnection;
            }
        }

        public IAsyncClientConnection GetGcbCommandConnection()
        {
            lock (connectionLockObject)
            {
                if (gcbCommandConnection == null && endPointsConfiguration?.GCBCommandsEndPoint != null)
                {
                    gcbCommandConnection = CreateConnection(
                        endPointsConfiguration.GCBCommandsEndPoint, 
                        clientPort: endPointsConfiguration.GCBCommandsEndPoint.Port!.Value,
                        reusePort: true);

                    if (recordGcbCommandsData)
                    {
                        gcbCommandConnection = AddLoggingProxy(gcbCommandConnection, subfolderName: "GcbCommands");
                    }
                }

                return gcbCommandConnection;
            }
        }
        
        public IAsyncClientConnection GetQcbCommConnection()
        {
            lock (connectionLockObject)
            {
                if (qcbCommConnection == null && endPointsConfiguration?.QcbCommandsEndPoint != null)
                {
                    // we need a different client port in case of dummy services
                    int clientPort = _debugSettings.UseDummyServices ? 
                        DebugQcbCommandClientPort :
                        endPointsConfiguration.QcbCommandsEndPoint.Port!.Value;

                    qcbCommConnection = CreateConnection(
                        endPointsConfiguration.QcbCommandsEndPoint,
                        clientPort: clientPort);

                    _ = _logWriter.LogAsync(
                        $"Establish QCB connection to {endPointsConfiguration.QcbCommandsEndPoint.Address()}",
                        Xcc.Core.Enums.LogRecordSeverity.Info,
                        Xcc.Core.Enums.LogRecordType.System);

                    if (recordQcbCommandsData)
                    {
                        qcbCommConnection = AddLoggingProxy(qcbCommConnection, subfolderName: "QcbCommands");
                    }
                }
            }


            return qcbCommConnection;
        }

        /// <summary>
        /// Verifies that with addresses from the localhost subnet, the port is not the same,
        /// to prevent UDP packets travelling in a loop.
        /// Currently, checks for localhost subnet only.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="clientPort"></param>
        /// <returns>True if endpoint and client port form a loop</returns>
        private static bool EndpointCausesLoop(Xcc.Core.Models.ISystemEndPoint endpoint, int clientPort)
        {
            return (endpoint.IPAddressPart1 == 127 && endpoint.Port == clientPort);
        }

        private IUdpClientConnection CreateConnection(Xcc.Core.Models.ISystemEndPoint endpoint, int clientPort, bool reusePort = false)
        {
            // We don't want for a connection to run on localhost with port loop,
            // so in case of a loop we just initialize with a new client port assigned by OS
            if (!EndpointCausesLoop(endpoint, clientPort))
            {
                return new UdpClientConnection(
                    endpoint.Ip(), endpoint.Port.Value, clientPort, reusePort);
            }
            else
            {
                _logWriter.Log(
                    $"UDP loop prevention: switch to system-allocated client port for endpoint={endpoint.Address()}",
                    Xcc.Core.Enums.LogRecordSeverity.Warn,
                    Xcc.Core.Enums.LogRecordType.System);
                return new UdpClientConnection(
                    endpoint.Ip(), endpoint.Port.Value);
            }
        }

        private void OnSettingsUpdate()
        {
            var oldConfig = endPointsConfiguration;
            var newConfig = endPointsConfiguration = settingsStore.Settings.EndPointsConfiguration;

            lock (connectionLockObject)
            {
                // Update telemetry service endpoint:
                if (gcbTelemetryConnection is not null && !newConfig.GCBTelemetryEndPoint.Equals(oldConfig.GCBTelemetryEndPoint))
                {
                    gcbTelemetryConnection.SetEndpoint(newConfig.GCBTelemetryEndPoint.Ip(), newConfig.GCBTelemetryEndPoint.Port.Value);
                }

                // Update command service endpoint
                if (gcbCommandConnection is not null && !newConfig.GCBCommandsEndPoint.Equals(oldConfig.GCBCommandsEndPoint))
                {
                    gcbCommandConnection.SetEndpoint(newConfig.GCBCommandsEndPoint.Ip(), newConfig.GCBCommandsEndPoint.Port.Value);
                }


                // Update qc command service endpoint
                if (qcbCommConnection is not null && !newConfig.QcbCommandsEndPoint.Equals(oldConfig.QcbCommandsEndPoint))
                {
                    qcbCommConnection.SetEndpoint(newConfig.QcbCommandsEndPoint.Ip(), newConfig.QcbCommandsEndPoint.Port.Value);
                }
            }
        }

        private IUdpClientConnection AddLoggingProxy(
            IUdpClientConnection connection,
            string subfolderName,
            int timeoutMs = 0,
            UdpConnectionLoggingProxy.NewFileEvery newFileEvery = UdpConnectionLoggingProxy.NewFileEvery.Day)
        {
            string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            // First we create a common folder in user's Desktop folder where we'll put all the logs:
            var gcbLogsFolder = Path.Join(rootPath, "HeraclesCommLogs");
            if (!Directory.Exists(gcbLogsFolder))
            {
                Directory.CreateDirectory(gcbLogsFolder);
            }
            // Now we create a logging proxy writing into GcbTelemetry subfolder:
            var commandsLogFolder = Path.Join(gcbLogsFolder, subfolderName);
            return new UdpConnectionLoggingProxy(
                connection, _logWriter,
                UdpConnectionLoggingProxy.LoggedPackets.All,
                commandsLogFolder, 
                filenamePrefix: subfolderName,
                newFileEvery: newFileEvery, 
                timeoutMs: timeoutMs);

        }
    }
}
