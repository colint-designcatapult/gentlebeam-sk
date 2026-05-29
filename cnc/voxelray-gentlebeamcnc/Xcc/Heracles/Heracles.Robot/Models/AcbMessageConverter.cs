using System;
using System.Linq;
using Empyrean.Common.Infra.Networking.Udp;
using Heracles.Core.Enums;
using Heracles.Robot.Models.Enums;

namespace Heracles.Robot.Models
{
    /// <summary>
    /// Actuator control board
    /// </summary>
    public class AcbMessageConverter : IAcbMessageConverter
    {
        public AcbMessageConverter()
        {
        }

        public byte[] GenerateActuatorCommandMessage(AcbActuatorId actuatorId, AcbActuatorCommand command)
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.Actuators,
                packetCounter: ++packetID, // packet id
                payload: [(int)actuatorId, (int)command]);
        }

        public byte[] GenerateActuatorStatusPollMessage()
        {
            return UdpPacketBuilder.BuildRawPacket(
                packetType: (uint)AcbPacketType.StatusPoll,
                packetCounter: ++packetID, // packet id
                payload: [0, 0]);

        }

        public static bool IsValidMessage(byte[] bytes)
        {
            // according to the protocol
            var crc = bytes.TakeLast(4).ToArray();
            var calculatedCrc = CrcUtils.GetCrc(bytes.Take(bytes.Length - 4).ToArray());

            //return crc.SequenceEqual(calculatedCrc);
            return BitConverter.ToInt32(crc, 0) == BitConverter.ToInt32(calculatedCrc, 0);
        }

        //public static bool IsValidMessage(byte[] bytes)
        //{
        //    try
        //    {
        //        UdpPacket packet = new(bytes);
        //        return true;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        public AcbActuatorStatusResponse ParseStatusPollResponse(byte[] response)
        {
            var packet = new UdpPacket(response);
            if (packet.PayloadLength != 3
                || (int)packet.PacketType != (int)AcbPacketType.StatusPoll) 
            {
                throw new ArgumentException("Actuator poll response error: Invalid response packet");
            }

            AcbActuatorStatusResponse result = new();
            var ids = System.Enum.GetValues<AcbActuatorId>(); 
            for (int i = 0; i < 3; ++i)
            {
                byte[] actuatorStates = packet[i];

                var info = new ActuatorStateInfo()
                {
                    ActuatorState = (AcbActuatorState)actuatorStates[0],
                    ProxySensorState = (AcbProxySensorState)actuatorStates[1],
                    LightSensorState = (AcbLightSensorState)actuatorStates[2],
                    FootPedalState = (AcbFootPedalState)actuatorStates[3]
                };
                result.ActuatorStates[ids[i]] = info;
            }

            return result;
        }

        private uint packetID;
    }
}
