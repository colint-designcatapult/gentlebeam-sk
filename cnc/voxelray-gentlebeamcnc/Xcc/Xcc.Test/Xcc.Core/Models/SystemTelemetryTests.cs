using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Application.Helpers;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models
{
    internal class SystemTelemetryTests
    {
        private static UdpPacket GetRandomTelemetryPacket()
        {
            var packet = new UdpPacket(
                packetType: (uint)GCBPacketType.TelemetryResponse,
                packetCounter: 0,
                payloadLength: (uint)GCBTelemetryResponseField.PayloadFields);
            for (int n = 0; n < packet.PayloadLength; ++n)
            {
                packet[n] = Random.Shared.Next();
            }
            // Set some valid enums:
            packet[(int)GCBTelemetryResponseField.SystemState] = (int)GcbStateNew.Cold;
            packet[(int)GCBTelemetryResponseField.RingLedState] = (int)RingLedState.TBD2;
            packet[(int)GCBTelemetryResponseField.BaseLedState] = (int)BaseLedState.TBD2;

            packet.UpdateCRC();
            return packet;
        }

        [Test]
        public void NewVsOldParseTest()
        {
            var randomPacket = GetRandomTelemetryPacket();

            SystemTelemetry parsedData = SystemTelemetry.Parse(randomPacket.Buffer);
            SystemTelemetry parsedDataOld = TestSystemTelemetry.ParseOld(randomPacket.Buffer);

            // We should compare them entirely, but it'd be exhaustive, so we just check some
            Assert.That(parsedData.SystemRuntime, Is.EqualTo(parsedDataOld.SystemRuntime));
            Assert.That(parsedData.CollimatorSerial, Is.EqualTo(parsedDataOld.CollimatorSerial));
            Assert.That(parsedData.XCoilCurrent, Is.EqualTo(parsedDataOld.XCoilCurrent));
            Assert.That(parsedData.Applicator, Is.EqualTo(parsedDataOld.Applicator));
        }


        /// <summary>
        /// We refactor the telemetry package building and parsing, so here we use old implementation to test against
        /// </summary>
        class TestSystemTelemetry : SystemTelemetry
        {
            public static SystemTelemetry ParseOld(byte[] data)
            {
                TestSystemTelemetry telemetry = new();

                //skip header
                int position = 20;// data start at this position and followed by skips/steps of 4 bytes

                var tempBytes = data.Skip(position).Take(4).ToArray();
                telemetry.ControlBoardState = (GcbStateNew)BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.SystemRuntime = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.FaultFlags = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.InterlockFlags = BitConverter.ToUInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.RingLedState = (RingLedState)BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.BaseLedState = (BaseLedState)BitConverter.ToInt32(tempBytes, 0);

                //tempBytes = data.Skip(position += 4).Take(4).ToArray();
                //var collimatorIdPart1 = tempBytes.Reverse().ToArray();
                var collimatorIdPart1 = data.Skip(position += 4).Take(4).ToArray();
                telemetry.CollimatorId1 = BitConverter.ToUInt32(collimatorIdPart1, 0);

                var collimatorIdPart2 = data.Skip(position += 4).Take(4).ToArray();
                //collimatorIdPart2 = collimatorIdPart2.Reverse().ToArray();
                telemetry.CollimatorId2 = BitConverter.ToUInt32(collimatorIdPart2, 0);

                telemetry.CollimatorSerial = BitConverter.ToUInt64(ByteArrayUtils.JoinByteArrays(collimatorIdPart1, collimatorIdPart2), 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.ButtonsState = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.CurrentOperationalPoint = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.TotalOperationalPoints = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.InternalTimerState = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.PrimaryTimerValue = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.Timer1State = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.SecondaryTimer1Value = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.Timer2State = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.SecondaryTimer2Value = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.RuntimeCounterHVPS = BitConverter.ToInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.HvpsIOStatus = BitConverter.ToUInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.HvpsFlagStatus = BitConverter.ToUInt32(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.KvFeedback = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.EmissionCurrent = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.HeaterCurrentSetpoint = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.HeaterCurrentFeedback = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.GridSetpoint = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.GridVoltage = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.XCoilCurrent = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.YCoilCurrent = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.FocusCurrent = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.IonPumpFeedback = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.WaterPressure = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.WaterFlowRate = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.WaterTemperature = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.HeatSinkTemperature = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.PeltierTemperature = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.CabinetTemperature = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                var mag1x = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                var mag1y = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                var mag1z = BitConverter.ToSingle(tempBytes, 0);

                telemetry.Mag1 = [mag1x, mag1y, mag1z];

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                var mag2x = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                var mag2y = BitConverter.ToSingle(tempBytes, 0);

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                var mag2z = BitConverter.ToSingle(tempBytes, 0);

                telemetry.Mag2 = [mag2x, mag2y, mag2z];

                tempBytes = data.Skip(position += 4).Take(4).ToArray();
                telemetry.Applicator = BitConverter.ToUInt32(tempBytes);

                return telemetry;
            }
        }
    }
}
