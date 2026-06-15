using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Logging;

namespace Xcc.Infra.Networking.Udp
{
    public class UdpConnectionLoggingProxy : IUdpClientConnection
    {
        private readonly IUdpClientConnection _connection;
        private readonly ILogWriter _logWriter;
        private readonly LoggedPackets _loggedPackets;
        private readonly string _workingFolder;
        private readonly string _filenamePrefix;
        private readonly long _recordingTimeout;
        private DateTime _lastDateTime;
        private DateTime _lastFileCreationTime;
        private readonly NewFileEvery _newFileEvery;
        private readonly object _fileLock = new object();

        private StreamWriter? _logFile = null!;
        public StreamWriter? LogFile
        {
            get => _logFile;
            private set
            {
                if (_logFile != value)
                {
                    _logFile?.Close();
                    _logFile?.Dispose();
                }
                _logFile = value;
            }
        }

        public enum LoggedPackets : int
        {
            Send = 1,
            Receive = 2,
            All = 3
        }

        public enum NewFileEvery
        {
            Never = -1,
            Day = 0,
            Hour = 1,
        }

        public UdpConnectionLoggingProxy(
            IUdpClientConnection connection,
            ILogWriter logWriter,
            LoggedPackets loggedPackets,
            string workingFolder,
            string filenamePrefix,
            long timeoutMs = 0,
            NewFileEvery newFileEvery = NewFileEvery.Day)
        {
            _connection = connection;
            _logWriter = logWriter;
            _loggedPackets = loggedPackets;
            _workingFolder = workingFolder;
            _filenamePrefix = filenamePrefix;
            _recordingTimeout = timeoutMs;
            _newFileEvery = newFileEvery;

            _lastDateTime = DateTime.Now.AddDays(-2);
            _lastFileCreationTime = DateTime.Now.AddDays(-2);

            if (!Directory.Exists(workingFolder))
            {
                Directory.CreateDirectory(workingFolder);
            }
        }

        public async Task<byte[]> ReceiveAsync(CancellationToken cancellationToken)
        {
            var response = await _connection.ReceiveAsync(cancellationToken);
            _ = OnReceiveAsync(response);
            return response;
        }


        public Task<int> SendAsync(byte[] data)
        {
            _ = OnSendAsync(data);
            return _connection.SendAsync(data);
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _logFile?.Dispose();
        }

        public void SetEndpoint(string hostAddress, int hostPort)
        {
            _connection?.SetEndpoint(hostAddress, hostPort);
        }

        protected virtual bool IsLoggingNeeded(DateTime timestamp, LoggedPackets direction, byte[] packet)
        {
            return RecordingTimeoutExceeded(timestamp);
        }

        #region private methods

        private async Task OnReceiveAsync(byte[] response)
        {
            if (_loggedPackets == LoggedPackets.Receive || _loggedPackets == LoggedPackets.All)
            {
                await LogPacket(LoggedPackets.Receive, response);
            }
        }

        private async Task OnSendAsync(byte[] data)
        {
            if (_loggedPackets == LoggedPackets.Send || _loggedPackets == LoggedPackets.All)
            {
                await LogPacket(LoggedPackets.Send, data);
            }
        }


        private async Task LogPacket(LoggedPackets direction, byte[] packet)
        {
            try
            {
                var timestamp = DateTime.Now;
                if (!IsLoggingNeeded(timestamp, direction, packet))
                {
                    //Debug.WriteLine($"Skip event at: {timestamp}");
                    return; // Skip this event
                }

                //var binaryTimestamp = BitConverter.GetBytes(timestamp.ToBinary());
                //var binaryDirection = BitConverter.GetBytes((int)direction);
                //var record = ByteArrayUtils.JoinByteArrays([binaryTimestamp, binaryDirection, packet]);
                await WriteToFile(timestamp, direction, packet);
            }
            catch (Exception ex)
            {
                _ = _logWriter.LogAsync(
                    $"UdpConnectionLoggingProxy packet logging error: {ex.Message}",
                    Core.Enums.LogRecordSeverity.Error,
                    Core.Enums.LogRecordType.System);
            }

        }

        private bool RecordingTimeoutExceeded(DateTime timestamp)
        {
            return _lastDateTime.AddMilliseconds(_recordingTimeout) < timestamp;
        }

        private async Task WriteToFile(DateTime timestamp, LoggedPackets direction, byte[] record)
        {
            await Task.Run(() =>
            {
                lock (_fileLock)
                {
                    if (LogFile is null || TimeToCreateNewFile(timestamp))
                    {
                        CreateNewTelemetryFile(timestamp);
                        _lastFileCreationTime = timestamp;
                    }
                    _lastDateTime = timestamp;

                    var hexRecord = Convert.ToHexString(record);
                    if (_loggedPackets == LoggedPackets.All)
                    {
                        // Specify direction to distinguish the message directions:
                        LogFile?.WriteLine($"{timestamp:yyy-MM-dd HH:mm:ss.ffffff} {direction}: {hexRecord}");
                    }
                    else
                    {
                        LogFile?.WriteLine($"{timestamp:yyy-MM-dd HH:mm:ss.ffffff} {hexRecord}");
                    }
                    LogFile?.Flush();
                }
            });
        }

        private bool TimeToCreateNewFile(DateTime timestamp)
        {
            var deadline = _newFileEvery switch
            {
                NewFileEvery.Day => _lastFileCreationTime.AddDays(1),
                NewFileEvery.Hour => _lastFileCreationTime.AddHours(1),
                _ => timestamp // if we didn't specify it, we never create a new file

            };
            return deadline < timestamp;
        }

        private void CreateNewTelemetryFile(DateTime timestamp)
        {
            LogFile = null; // close & dispose previous, just in case if this is the same file
            string filepath = Path.Join(_workingFolder, $"{_filenamePrefix}_{timestamp.ToString("yyyy-MM-dd_HH-mm-ss")}.txt");

            LogFile = File.Exists(filepath) ? File.AppendText(filepath) : File.CreateText(filepath);
        }

        #endregion private methods
    }
}
