using Empyrean.Common.Infra.Networking.Udp;
using System;
using Xcc.Core.Domain.UPS;
using Xcc.Core.Services;

namespace Xcc.Infra.UPS
{
    public class UpsTelemetryPacket : UdpPacket
    {
        public enum PayloadFields : uint
        {
            HasTelemetry = 0,
            InputVoltage,
            BatteryNotInUse,
            BatteryChargedPercent,
            EstimatedBatRuntime,
            TotalLength
        }

        public UpsTelemetryPacket(byte[] buffer) : base(buffer)
        {
            if (PayloadLength != (uint)PayloadFields.TotalLength)
            {
                throw new ArgumentException("UpsTelemetryPacket error: invalid packet size");
            }
        }

        private UpsTelemetryPacket(
            UpsType type,
            uint packetCounter)
            : base(packetType: (uint)type, 
                  packetCounter: packetCounter, 
                  payloadLength: (uint)PayloadFields.TotalLength)
        { 
        }

        public static UpsTelemetryPacket FromTelemetry(UpsType type, uint packetId, IUpsTelemetry? telemetry)
        {
            var packet = new UpsTelemetryPacket(type, packetId);
            packet[(int)PayloadFields.HasTelemetry] = (telemetry != null) ? 1 : 0;
            if (telemetry != null)
            {
                packet[(int)PayloadFields.InputVoltage] = (float)telemetry.InputVoltage;
                packet[(int)PayloadFields.BatteryNotInUse] = telemetry.BatteryNotInUse ? 1 : 0;
                packet[(int)PayloadFields.BatteryChargedPercent] = (float)telemetry.BatteryChargedPercent;
                packet[(int)PayloadFields.EstimatedBatRuntime] = telemetry.EstimatedBatRuntime;
            }
            packet.UpdateCRC();

            return packet;
        }

        public IUpsTelemetry? ToUpsTelemetry()
        {
            bool hasTelemetry = this[(int)PayloadFields.HasTelemetry] != 0;
            if (hasTelemetry)
            {
                return new UpsTelemetry
                {
                    InputVoltage = (float)this[(int)PayloadFields.InputVoltage],
                    BatteryNotInUse = this[(int)PayloadFields.BatteryNotInUse] != 0,
                    BatteryChargedPercent = (float)this[(int)PayloadFields.BatteryChargedPercent],
                    EstimatedBatRuntime = this[(int)PayloadFields.EstimatedBatRuntime],
                };
            }
            return null;
        }
    }
}
