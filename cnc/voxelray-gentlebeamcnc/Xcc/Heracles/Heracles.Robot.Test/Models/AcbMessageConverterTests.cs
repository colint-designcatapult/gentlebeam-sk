using Empyrean.Common.Infra.Networking.Udp;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;
using Xcc.Application.Helpers;

namespace Heracles.Robot.Test.Models
{
    internal class AcbMessageConverterTests
    {
        private AcbMessageConverter converter = new();
        private OldAcbMessageGenerator oldConverter = new();

        [Test]
        public void GenerateActuatorMessageTest()
        {
            var pollMsg = converter.GenerateActuatorStatusPollMessage();
            UdpPacket msgPacket = new(pollMsg);
            Assert.Multiple(() =>
            {
                Assert.That(msgPacket.PayloadLength, Is.EqualTo(2));
                Assert.That((int)msgPacket[0], Is.EqualTo(0));
                Assert.That((int)msgPacket[1], Is.EqualTo(0));
            });
        }


        [Test]
        public void ParseStatusPollResponseTest()
        {
            AcbActuatorStatusResponse actuatorStatus = new();
            actuatorStatus.ActuatorStates[AcbActuatorId.Image] = new ActuatorStateInfo()
            {
                ActuatorState = AcbActuatorState.Lock,
                ProxySensorState = AcbProxySensorState.Unknown,
                LightSensorState = AcbLightSensorState.NotInterrpupt,
                FootPedalState = AcbFootPedalState.Down,
            };
            actuatorStatus.ActuatorStates[AcbActuatorId.Treatment] = new ActuatorStateInfo()
            {
                ActuatorState = AcbActuatorState.Unlock,
                ProxySensorState = AcbProxySensorState.Detected,
                LightSensorState = AcbLightSensorState.Unknown,
                FootPedalState = AcbFootPedalState.Down,
            };
            actuatorStatus.ActuatorStates[AcbActuatorId.Robot] = new ActuatorStateInfo()
            {
                ActuatorState = AcbActuatorState.Unknown,
                ProxySensorState = AcbProxySensorState.NotDetected,
                LightSensorState = AcbLightSensorState.Interrupt,
                FootPedalState = AcbFootPedalState.Down,
            };

            var imageState = actuatorStatus.ActuatorStates[AcbActuatorId.Image];
            int imageStatusFlags = (int)imageState.ActuatorState
                | (int)imageState.ProxySensorState << 8
                | (int)imageState.LightSensorState << 16
                | (int)imageState.FootPedalState << 24;

            var treatmentState = actuatorStatus.ActuatorStates[AcbActuatorId.Treatment];
            int treatmentStatusFlags = (int)treatmentState.ActuatorState
                | (int)treatmentState.ProxySensorState << 8
                | (int)treatmentState.LightSensorState << 16
                | (int)treatmentState.FootPedalState << 24;
            var robotState = actuatorStatus.ActuatorStates[AcbActuatorId.Robot];
            int robotStatusFlags = (int)robotState.ActuatorState
                | (int)robotState.ProxySensorState << 8
                | (int)robotState.LightSensorState << 16
                | (int)robotState.FootPedalState << 24;

            var pollPacket = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.StatusPoll,
                packetCounter: (uint)AcbPacketId.Actuators, // packet id
                payload: [imageStatusFlags, treatmentStatusFlags, robotStatusFlags]);

            AcbActuatorStatusResponse? parsedStatus = null;
            Assert.DoesNotThrow(() => parsedStatus = converter.ParseStatusPollResponse(pollPacket));
            Assert.That(parsedStatus, Is.Not.Null);
            var parsedImageState = parsedStatus.Value.ActuatorStates[AcbActuatorId.Image];
            Assert.Multiple(() =>
            {
                Assert.That(parsedImageState.ActuatorState, Is.EqualTo(imageState.ActuatorState));
                Assert.That(parsedImageState.ProxySensorState, Is.EqualTo(imageState.ProxySensorState));
                Assert.That(parsedImageState.LightSensorState, Is.EqualTo(imageState.LightSensorState));
                Assert.That(parsedImageState.FootPedalState, Is.EqualTo(imageState.FootPedalState));
            });

            var parsedTreatmentState = parsedStatus.Value.ActuatorStates[AcbActuatorId.Treatment];
            Assert.Multiple(() =>
            {
                Assert.That(parsedTreatmentState.ActuatorState, Is.EqualTo(treatmentState.ActuatorState));
                Assert.That(parsedTreatmentState.ProxySensorState, Is.EqualTo(treatmentState.ProxySensorState));
                Assert.That(parsedTreatmentState.LightSensorState, Is.EqualTo(treatmentState.LightSensorState));
                Assert.That(parsedTreatmentState.FootPedalState, Is.EqualTo(treatmentState.FootPedalState));
            });

            var parsedRobotState = parsedStatus.Value.ActuatorStates[AcbActuatorId.Robot];
            Assert.Multiple(() =>
            {
                Assert.That(parsedRobotState.ActuatorState, Is.EqualTo(robotState.ActuatorState));
                Assert.That(parsedRobotState.ProxySensorState, Is.EqualTo(robotState.ProxySensorState));
                Assert.That(parsedRobotState.LightSensorState, Is.EqualTo(robotState.LightSensorState));
                Assert.That(parsedRobotState.FootPedalState, Is.EqualTo(robotState.FootPedalState));
            });
        }

        [Test]
        public void ParseStatusPollResponse_WrongPacketTest()
        {
            var actuatorType = AcbActuatorId.Image;
            var pollPacketTooSmallPayloadSize = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.StatusPoll,
                packetCounter: (uint)AcbPacketId.Actuators, // packet id
                payload: []);

            var pollPacketTooLargePayloadSize = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.StatusPoll,
                packetCounter: (uint)AcbPacketId.Actuators, // packet id
                payload: [0, 0, 0, 0]);

            var pollPacketWrongPacketType = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.System,
                packetCounter: (uint)AcbPacketId.Actuators, // packet id
                payload: [(int)actuatorType, 0]);

            var pollPacketWrongPacketId = UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.StatusPoll,
                packetCounter: (uint)AcbPacketId.Led, // packet id
                payload: [(int)actuatorType, 0]);

            Assert.Throws<ArgumentException>(() => converter.ParseStatusPollResponse(pollPacketTooSmallPayloadSize));
            Assert.Throws<ArgumentException>(() => converter.ParseStatusPollResponse(pollPacketTooLargePayloadSize));
            Assert.Throws<ArgumentException>(() => converter.ParseStatusPollResponse(pollPacketWrongPacketType));
            Assert.Throws<ArgumentException>(() => converter.ParseStatusPollResponse(pollPacketWrongPacketId));
        }

        [Test]
        public void AcbPacketCorrectnessTest([Values] AcbActuatorId actuatorId, [Values] AcbActuatorCommand actuatorCommand)
        {
            UdpPacket packet = new UdpPacket((uint)AcbPacketType.Actuators, (uint)AcbPacketId.Actuators, payloadLength: 2)
                .Set(0, (int)actuatorId)
                .Set(1, (int)actuatorCommand)
                .UpdateCRC();

            var referencePacket = GenerateActuatorMessage(actuatorId, actuatorCommand);

            Assert.That(packet.Buffer, Is.EqualTo(referencePacket));
        }
        

        private byte[] GenerateActuatorMessage(AcbActuatorId id, AcbActuatorCommand command)
        {
            byte[] bytesToSend = {
                0xFF, 0xFF, 0xFF, 0xFF,// sync
                0xFF, 0xFF, 0xFF, 0xFF,// sync
            };
            const int packetDataCount = 2;
            int packetType = (int)AcbPacketType.Actuators;
            int packetId = (int)AcbPacketId.Actuators;
            int actuatorId = (int)id;
            int actuatorCommand = (int)command;

            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes(packetType));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes(packetId));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes(packetDataCount));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes(actuatorId));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes(actuatorCommand));
            byte[] crc = CrcUtils.GetCrc(bytesToSend);
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, crc);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytesToSend);

            return bytesToSend;
        }
    }

    public class OldAcbMessageGenerator
    {

        public OldAcbMessageGenerator()
        {
        }

        public byte[] GenerateActuatorMessage(AcbActuatorId id, AcbActuatorCommand command)
        {
            return GenerateMessage(AcbPacketType.Actuators, AcbPacketId.Actuators, id, command);
        }

        private byte[] GenerateMessage(
            AcbPacketType packetType,
            AcbPacketId packetId,
            AcbActuatorId actuatorId,
            AcbActuatorCommand actuatorCommand)
        {
            byte[] bytesToSend = {
                0xFF, 0xFF, 0xFF, 0xFF,// sync
                0xFF, 0xFF, 0xFF, 0xFF,// sync
            };

            const int packetDataCount = 2;

            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)packetType));
            //bytesToSend = JoinPacketID(bytesToSend);
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)packetId));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes(packetDataCount));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)actuatorId));
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, BitConverter.GetBytes((int)actuatorCommand));
            byte[] crc = CrcUtils.GetCrc(bytesToSend);
            bytesToSend = ByteArrayUtils.JoinByteArrays(bytesToSend, crc);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(bytesToSend);

            return bytesToSend;
        }

        public static bool IsValidMessage(byte[] bytes)
        {
            // according to the protocol
            var crc = bytes.TakeLast(4).ToArray();
            var calculatedCrc = CrcUtils.GetCrc(bytes.Take(bytes.Length - 4).ToArray());

            //return crc.SequenceEqual(calculatedCrc);
            return BitConverter.ToInt32(crc, 0) == BitConverter.ToInt32(calculatedCrc, 0);
        }

        public bool StatusMessageContains(byte[] message, AcbActuatorId id, AcbActuatorCommand state)
        {
            var packetType = BitConverter.ToInt32(message.Skip(8).Take(4).ToArray(), 0);
            if (packetType != (int)AcbPacketType.StatusPoll)
                return false;

            var messageId = BitConverter.ToInt32(message.Skip(20).Take(4).ToArray(), 0);
            if (messageId != (int)id)
                return false;

            var messageState = BitConverter.ToInt32(message.Skip(24).Take(4).ToArray(), 0);
            if (messageState != (int)state)
                return false;

            return true;
        }
    }
}