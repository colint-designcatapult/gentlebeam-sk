using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Domain.QualityCheck;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.QualityCheck.Comm;

namespace Xcc.Infra.QualityCheck
{
    enum QcbReadingsCommandType
    {
        Start = 1,
        ReportAndStop = 2,
    }

    public class QcbService : IQcbService
    {
        private const int PING_RESPONSE_TIMEOUT = 250;
        private const int QUERY_RESPONSE_TIMEOUT = 1000;
        private uint packetId;

        public ILogWriter LogWriter { get; }
        public IQcbCommunicationService QcbCommunicationService { get; }

        public QcbService(
            ILogWriter logWriter,
            IQcbCommunicationService qcbCommunicationService)
        {
            LogWriter = logWriter;
            QcbCommunicationService = qcbCommunicationService;
        }

        public void Start()
        {
            QcbCommunicationService.Start();
        }


        public void Dispose()
        {
            QcbCommunicationService.Dispose();
        }

        /// <summary>
        /// Requests QCBoard to check if it responds
        /// </summary>
        /// <returns></returns>
        public async Task<bool> PingBoardAsync()
        {
            try
            {
                var pingRequest = UdpPacketBuilder.BuildRawPacket(
                    packetType: (uint)GCBPacketType.QcbPing,
                    packetCounter: ++packetId,
                    payload: [0, 0]);

                byte[] response = await SendRequestSeveralTimes(pingRequest, PING_RESPONSE_TIMEOUT);

                return response is not null;
            }
            catch (Exception ex)
            {
                throw new QcbNoConnectionException("No response from the QC board", ex);
            }
        }

        /// <summary>
        /// Commands QCBoard to start accumulating intensity readings.
        /// Throws errors if response is missing or invalid
        /// </summary>
        /// <returns></returns>
        public async Task<QcbCommandResponseStatus> StartQCReadingsAsync(int numberOfDiodes, int samplingIntervalMs)
        {
            var readingsRequest = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.QcbReadingsCommand,
                packetCounter: ++packetId,
                payload: [(uint)QcbReadingsCommandType.Start, samplingIntervalMs]);

            byte[] response = await SendRequestSeveralTimes(readingsRequest, QUERY_RESPONSE_TIMEOUT, 5);

            if (response is null)
            {
                return QcbCommandResponseStatus.NoResponse;
            }

            UdpPacket responsePacket = new(response);

            if (responsePacket.PacketType != (uint) GCBPacketType.QcbReadingsCommandResponse)
            {
                throw new Exception($"Invalid QCBoard response: actual packet type: {responsePacket.PacketType}, expected: {(uint)GCBPacketType.QcbReadingsCommand}");
            }

            if (responsePacket.PayloadLength != numberOfDiodes)
            {
                throw new Exception($"Invalid QCBoard response: actual payload length: {responsePacket.PayloadLength}, expected: {numberOfDiodes}");
            }

            Debug.WriteLine($"QcbService.StartQCReadingsAsync - start response packed_id ={responsePacket.PacketCounter}");

            for (int i = 0; i < responsePacket.PayloadLength; ++i)
            {
                int fieldValue = responsePacket[i];
                Debug.WriteLine($"QcbService.StartQCReadingsAsync - start response field #{i}={fieldValue}");
                if (fieldValue != 0)
                    return QcbCommandResponseStatus.StartRejected;
            }

            return QcbCommandResponseStatus.StartConfirmed;
        }

        /// <summary>
        /// Queries readings from the board and signals it to stop the accumulation.
        /// Throws errors if response is missing or invalid
        /// </summary>
        /// <returns></returns>
        public async Task<QcReadings?> StopQCReadingsAsync(int numberOfDiodes)
        {
            var readingsRequest = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)GCBPacketType.QcbReadingsCommand,
                packetCounter: ++packetId,
                payload: [(uint)QcbReadingsCommandType.ReportAndStop, 0/*reserved*/]);

            byte[] response = await SendRequestSeveralTimes(readingsRequest, QUERY_RESPONSE_TIMEOUT, 5);

            if (response == null)
                return null;

            UdpPacket responsePacket = new(response);
            if (responsePacket.PacketType != (uint)GCBPacketType.QcbReadingsCommandResponse)
            {
                throw new Exception($"Invalid QCBoard response: actual packet type: {responsePacket.PacketType}, expected: {(uint)GCBPacketType.QcbReadingsCommand}");
            }
            if (responsePacket.PayloadLength != numberOfDiodes)
            {
                throw new Exception($"Invalid QCBoard response: actual payload length: {responsePacket.PayloadLength}, expected: {numberOfDiodes}");
            }

            var readings = new float[numberOfDiodes];

            for (int i = 0; i < numberOfDiodes; ++i)
            {
                // We get readings from the board multiplied by 1000 to keep precision in uint
                //uint readingValueTimes1000 = responsePacket[i];
                //readings[i] = readingValueTimes1000 / 1000.0f;


                // it was uint type previously, but now it is changed to float for Heracles.
                readings[i] = responsePacket[i];
            }

            return new QcReadings(readings);
        }
        private async Task<byte[]> SendRequestSeveralTimes(byte[] data, int timeout, int attempts = 3)
        {
            for (int i = 1; i <= attempts; i++)
            {
                try
                {
                    var bytes = await QcbCommunicationService.SendRequestAsync(data, timeout);
                    if (_sendRequestSuccess == false)
                    {
                        _ = LogWriter.LogAsync("QCB UdpService communication established", LogRecordSeverity.Info, LogRecordType.System);
                    }
                    _sendRequestSuccess = true;
                    return bytes;
                }
                catch (UdpException ex)
                {
                    //_ = LogWriter.LogAsync($"UdpService request sending error #{i}: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                    if (_sendRequestSuccess)
                    {
                        _ = LogWriter.LogAsync($"QCB UdpService communication failed: {ex.Message}", LogRecordSeverity.Error, LogRecordType.System);
                    }
                    _sendRequestSuccess = false;
                }
            }

            throw new QcbNoConnectionException("No connection. QCB does not respond");
        }

        private bool _sendRequestSuccess = true;
    }

    public class QcbNoConnectionException : Exception
    {
        public QcbNoConnectionException(string message) : base(message) { }
        public QcbNoConnectionException(string message, Exception inner) : base(message, innerException: inner) { }
    }

}
