using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Test.Xcc.Infra.Services.XRayServices
{
    [NonParallelizable]
    internal class GcbXRayCommandOperatorTests
    {
        TestGcbXRayCommandOperator oldCommandOperator = new();
        GcbXRayCommandOperator commandOperator = new();

        [SetUp]
        public void SetUp()
        {
            oldCommandOperator = new();
            commandOperator = new();
        }

        [Test]
        public void GcbVersionInfoRequest_PacketCorrectnessTest()
        {
            var packet = commandOperator.GenerateVersionInfoRequestCmd();
            var referencePacket = oldCommandOperator.GenerateVersionInfoRequestCmd();

            Assert.That(packet, Is.EqualTo(referencePacket));
        }

        [Test]
        public void GcbFaultInfoQuery_EncodesRequestedIndex()
        {
            const uint requestedIndex = 3;
            var packet = commandOperator.GenerateFaultInfoRequestCmd(requestedIndex);
            var referencePacket = oldCommandOperator.GenerateFaultInfoRequestCmd(requestedIndex);
            var response = new UdpPacket(packet);

            Assert.Multiple(() =>
            {
                Assert.That(packet, Is.EqualTo(referencePacket));
                Assert.That(response.PayloadLength, Is.EqualTo(1));
                Assert.That((uint)response[0], Is.EqualTo(requestedIndex));
            });
        }

        [Test]
        public void GcbDirective_PacketCorrectnessTest([Values] GCBDirectiveCommandNew commandId)
        {
            var packet = commandOperator.GenerateDirectiveCmd(commandId);
            var referencePacket = oldCommandOperator.GenerateDirectiveCmd(commandId);

            Assert.That(packet, Is.EqualTo(referencePacket));
        }


        [Test]
        public void GcbTelemetryRequest_PacketCorrectnessTest()
        {
            var packet = commandOperator.GenerateTelemetryRequestCmd();
            var referencePacket = oldCommandOperator.GenerateTelemetryRequestCmd();
            
            Assert.That(packet, Is.EqualTo(referencePacket));
        }

        [Test]
        public void GcbConditioningCmd_PacketCorrectnessTest()
        {
            float filamentSetpoint = 2500.0f;

            var packet = commandOperator.GenerateConditioningCmd(filamentSetpoint);
            var referencePacket = oldCommandOperator.GenerateConditioningCmd(filamentSetpoint);

            Assert.That(packet, Is.EqualTo(referencePacket));
        }

        [Test]
        public void GcbWarmupCmd_PacketCorrectnessTest()
        {
            float filamentSetpoint = 2500.0f;
            var packet = commandOperator.GenerateWarmupCmd(filamentSetpoint);
            var referencePacket = oldCommandOperator.GenerateWarmupCmd(filamentSetpoint);

            Assert.That(packet, Is.EqualTo(referencePacket));
        }

        [Test]
        public void GcbNewSessionCmd_HasNoPayload()
        {
            var packet = commandOperator.GenerateNewSessionCmd();
            var referencePacket = oldCommandOperator.GenerateNewSessionCmd();
            var decoded = new UdpPacket(packet);

            Assert.Multiple(() =>
            {
                Assert.That(packet, Is.EqualTo(referencePacket));
                Assert.That(decoded.PayloadLength, Is.Zero);
            });
        }

        [TestCase(GCBPacketType.OperationalPointLoadingCmd)]
        [TestCase(GCBPacketType.OperationalPointConfirmationCmd)]
        public void GcbOperationPoint_PacketCorrectnessTest(GCBPacketType packetType)
        {
            GcbOperationalPoint op = new GcbOperationalPoint
            {
                TotalPointTime = 2.0f,
                RemainingPointTime = 1.0f,
                SetpointKv = 50.0f,
                FilamentSetpoint = 3500.0f,
                TargetMA = 2.0f,
                XCoilSetpoint = 0.1f,
                YCoilSetpoint = 0.2f,
                FocusCoilSetpoint = 2000.0f,
            };

            uint sessionId = 42;
            var sessionKey = new GcbSecretKeySessionAuthentication(new GcbSession(sessionId));
            var packet = commandOperator.GenerateOperationalPointCmd(packetType, op, sessionKey);
            var referencePacket = oldCommandOperator.GenerateOperationalPointCmd(packetType, op, sessionId);

            var decoded = new UdpPacket(packet);
            Assert.Multiple(() =>
            {
                Assert.That(decoded.PayloadLength, Is.EqualTo(9));
                Assert.That(
                    packet.Take(packet.Length - 8),
                    Is.EqualTo(referencePacket.Take(referencePacket.Length - 8)));
            });
        }

        [Test]
        public void GcbOperationPointQuery_HasNoPayload()
        {
            var packet = commandOperator.GenerateOperationalPointQueryCmd();
            var referencePacket = oldCommandOperator.GenerateOperationalPointQueryCmd();
            var decoded = new UdpPacket(packet);

            Assert.Multiple(() =>
            {
                Assert.That(packet, Is.EqualTo(referencePacket));
                Assert.That(decoded.PayloadLength, Is.Zero);
            });
        }

        [Test]
        public void GcbQcbPingCmd_EncodesFirmwarePacket()
        {
            var packet = new UdpPacket(commandOperator.GenerateQcbPingCmd());

            Assert.Multiple(() =>
            {
                Assert.That(packet.PacketType, Is.EqualTo((uint)GCBPacketType.QcbPing));
                Assert.That(packet.PayloadLength, Is.EqualTo(2));
                Assert.That((uint)packet[0], Is.Zero);
                Assert.That((uint)packet[1], Is.Zero);
            });
        }

        [Test]
        public void GcbQcbReadingsCmd_EncodesCommandAndSamplingWindow()
        {
            var packet = new UdpPacket(commandOperator.GenerateQcbReadingsCmd(1, 50));

            Assert.Multiple(() =>
            {
                Assert.That(packet.PacketType, Is.EqualTo((uint)GCBPacketType.QcbReadingsCommand));
                Assert.That(packet.PayloadLength, Is.EqualTo(2));
                Assert.That((uint)packet[0], Is.EqualTo(1));
                Assert.That((int)packet[1], Is.EqualTo(50));
            });
        }

        [Test]
        public void GcbReleaseTreatmentPlan_PacketCorrectnessTest([Values] GCBReleaseCommandScope scope)
        {
            uint sessionId = 42;
            var sessionKey = new GcbSecretKeySessionAuthentication(new GcbSession(sessionId));

            var packet = commandOperator.GenerateReleaseTreatmentPlanCmd(scope, sessionKey);
            var referencePacket = oldCommandOperator.GenerateReleaseTreatmentPlanCmd(scope, sessionId);

            // Authentication code doesn't fit, so we can't verify it and CRC now, just check all the rest:
            Assert.That(packet.Take(packet.Length - 8), Is.EqualTo(referencePacket.Take(referencePacket.Length - 8)));

        }
    }

    /// <summary>
    /// Test class borrowed from the previous working version of command builder implementation
    /// </summary>
    public class TestGcbXRayCommandOperator
    {
        protected uint txPacketCounter = 0;
        public uint PacketCounter { get => txPacketCounter; }

        #region Public methods
        /// <summary>
        /// This command is used to obtain the version information of the firmware. 
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateVersionInfoRequestCmd()
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.VersionInfo, 1);
            bytesToSend = JoinByteArrays(bytesToSend, new byte[] { 0x00, 0x00, 0x00, 0x00 });// reserved
            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            return JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.
        }

        /// <summary>
        /// This command is used to obtain information about the fault which trigger the system to enter into a fault state. 
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateFaultInfoRequestCmd(uint index)
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.FaultInfo, 1);
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(index));
            byte[] crc = GetCRC(bytesToSend);
            return JoinByteArrays(bytesToSend, crc);
        }

        /// <summary>
        /// This command is used to request that the board perform a conditioning cycle, to a desired setpoint.
        /// </summary>
        /// <param name="filamentSetpoint">[mA] Target setpoint for the conditioning process</param>
        /// <returns></returns>
        public byte[] GenerateConditioningCmd(float filamentSetpoint)
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.ConditioningCmd, 2);

            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(filamentSetpoint));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(0));// reserved 
            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            return JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.
        }

        /// <summary>
        /// This command is used to request that the board perform a warmup, to a desired setpoint.
        /// </summary>
        /// <param name="filamentSetpoint">[mA] Target setpoint for the warmup process</param>
        /// <returns></returns>
        public byte[] GenerateWarmupCmd(float filamentSetpoint)
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.WarmupCmd, 1);

            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(filamentSetpoint));
            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            return JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.
        }

        /// <summary>
        /// Starts authorization for one scalar emission.
        /// </summary>
        public byte[] GenerateNewSessionCmd()
        {
            byte[] bytesToSend = GenerateInitialArrayToSend(
                (byte)GCBPacketType.NewSessionCmd,
                0);
            byte[] crc = GetCRC(bytesToSend);
            return JoinByteArrays(bytesToSend, crc);
        }


        /// <summary>
        /// Queries the scalar staged emission.
        /// </summary>
        public byte[] GenerateOperationalPointQueryCmd()
        {
            byte[] bytesToSend = GenerateInitialArrayToSend(
                (byte)GCBPacketType.OperationalPointQueryCmd,
                0);
            byte[] crc = GetCRC(bytesToSend);
            return JoinByteArrays(bytesToSend, crc);
        }

        public byte[] GenerateReleaseTreatmentPlanCmd(GCBReleaseCommandScope scope, uint sessionId)
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.ReleaseTreatmentPlan, 2);

            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)scope));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(sessionId));

            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            return JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.
        }

        #endregion

        /// <summary>
        /// This command is used to direct the boards to perform or begin a specific operation	
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public byte[] GenerateDirectiveCmd(GCBDirectiveCommandNew command)
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.DirectiveCmd, 2);

            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)command));// actual command  
            uint bit = 0x01;
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)bit << ((int)command)));// confirmation  - is 1 shifted left by 'the value of the command' times 
            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            return JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.
        }


        /// <summary>
        /// Loading or Confirmation OP command
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="op"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public byte[] GenerateOperationalPointCmd(
            GCBPacketType packetType,
            GcbOperationalPoint op,
            uint sessionId)
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)packetType, 9);

            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.TotalPointTime));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.RemainingPointTime));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.SetpointKv));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.TargetMA));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.FilamentSetpoint));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.XCoilSetpoint));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.YCoilSetpoint));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(op.FocusCoilSetpoint));
            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(sessionId));
            byte[] crc = GetCRC(bytesToSend);
            return JoinByteArrays(bytesToSend, crc);
        }

        public GcbProcessingStatus ParseStatusResponse(byte[] data, GCBPacketType expectedPacketType, int expectedFieldsCount = 1)
        {
            var packetType = ParsePacketType(data);
            if (packetType != expectedPacketType)
            {
                throw new Exception($"GCB packet type {packetType.ToString()} does not match expected one {expectedPacketType.ToString()}");
            }

            int position = 16;
            byte[] tempBytes = data.Skip(position).Take(4).ToArray();
            int fieldsCount = BitConverter.ToInt32(tempBytes, 0);
            if (fieldsCount != expectedFieldsCount)
                throw new Exception($"Invalid GCB response format: fields count {fieldsCount}, but expected {expectedFieldsCount}");

            tempBytes = data.Skip(position += 4).Take(4).ToArray();

            return (GcbProcessingStatus)BitConverter.ToInt32(tempBytes, 0);
        }

        /// <summary>
        /// This command is used to request the current status of the system.
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateTelemetryRequestCmd()
        {
            byte[] bytesToSend = GenerateInitialArrayToSend((byte)GCBPacketType.TelemetryRequest, 1);

            bytesToSend = JoinByteArrays(bytesToSend, BitConverter.GetBytes(0));// reserved 
            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            return JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.
        }

        private GCBPacketType ParsePacketType(byte[] data)
        {
            if (data == null)
                throw new ArgumentNullException($"Failed to parse GCB response: {nameof(data)} is null");

            int position = 8; // data start at this position and followed by skips/steps of 4 bytes

            // take first  4 bytes - net data - after skipping 8 bytes (sync)
            byte[] tempBytes = data.Skip(position).Take(4).ToArray();

            return (GCBPacketType)BitConverter.ToInt32(tempBytes, 0);
        }

        protected byte[] GenerateInitialArrayToSend(byte packetType, byte fieldsCount)
        {
            byte[] bytesToSend = {
                0xFF, 0xFF, 0xFF, 0xFF,// sync
                0xFF, 0xFF, 0xFF, 0xFF,// sync
                packetType, 0x00, 0x00 ,0x00, // type
                0x11, 0x11, 0x11 ,0x11,// ID
                fieldsCount, 0x00, 0x00 ,0x00,// # of fields
            };
            UpdatePacketID(bytesToSend);

            return bytesToSend;
        }


        public byte[] GenerateReadMagnetometerCmd()
        {
            byte[] bytesToSend = {
                0xFF, 0xFF, 0xFF, 0xFF,// sync
                0xFF, 0xFF, 0xFF, 0xFF,// sync
                0x0D, 0x00, 0x00 ,0x00,// type = 13
                0x33, 0x33, 0x33 ,0x33,// ID
                0x01, 0x00, 0x00 ,0x00,// # of fields = 1
            };
            UpdatePacketID(bytesToSend);

            bytesToSend = JoinByteArrays(bytesToSend, new byte[] { 0x00, 0x00, 0x00, 0x00 });// reserved for future use
            byte[] crc = GetCRC(bytesToSend);// calculate CRC
            bytesToSend = JoinByteArrays(bytesToSend, crc);// add the calculated crc to the buffer.

            return bytesToSend;
        }

        protected void UpdatePacketID(byte[] txBuffer)
        {
            byte[] packetCounterBytes = BitConverter.GetBytes(++txPacketCounter);
            txBuffer[12] = packetCounterBytes[0];
            txBuffer[13] = packetCounterBytes[1];
            txBuffer[14] = packetCounterBytes[2];
            txBuffer[15] = packetCounterBytes[3];
        }

        protected static byte[] JoinByteArrays(byte[] array1, byte[] array2)
        {
            byte[] rv = new byte[array1.Length + array2.Length];

            Buffer.BlockCopy(array1, 0, rv, 0, array1.Length);
            Buffer.BlockCopy(array2, 0, rv, array1.Length, array2.Length);

            return rv;
        }

        protected static byte[] GetCRC(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        protected static uint ComputeChecksum(byte[] bytes)
        {
            uint[] crcTable = CrcUtils.Table;
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = (uint)((crc >> 8) ^ crcTable[index]);
            }
            return ~crc;
        }
    }
}
