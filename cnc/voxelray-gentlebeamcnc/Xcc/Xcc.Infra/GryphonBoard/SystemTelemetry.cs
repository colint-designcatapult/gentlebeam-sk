using System;
using System.Linq;
using System.Text;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard
{
    public class SystemTelemetry : ISystemTelemetry
    {
        public GcbStateNew ControlBoardState { get; protected set; }
        public int SystemRuntime { get; protected set; }
        public int FaultFlags { get; protected set; }
        public uint InterlockFlags { get; protected set; }
        public RingLedState RingLedState { get; protected set; }
        public BaseLedState BaseLedState { get; protected set; }
        public uint CollimatorId1 { get; protected set; }
        public uint CollimatorId2 { get; protected set; }
        public ulong CollimatorSerial { get; set; }
        public int ButtonsState { get; protected set; }
        public int CurrentOperationalPoint { get; protected set; }
        public int TotalOperationalPoints { get; protected set; }
        public int InternalTimerState { get; protected set; }
        public float PrimaryTimerValue { get; protected set; }
        public int Timer1State { get; protected set; }
        public float SecondaryTimer1Value { get; protected set; }
        public int Timer2State { get; protected set; }
        public float SecondaryTimer2Value { get; protected set; }
        public int RuntimeCounterHVPS { get; protected set; }
        public uint HvpsIOStatus { get; protected set; }
        public uint HvpsFlagStatus { get; protected set; }
        public float KvSetpoint { get; protected set; }
        public float KvFeedback { get; protected set; }
        public float EmissionCurrent { get; protected set; }
        public float HeaterCurrentSetpoint { get; protected set; }
        public float HeaterCurrentFeedback { get; protected set; }
        public float EmissionCurrentLimit { get; private set; }
        public float HvpsPowerSetpoint { get; private set; }
        public float GridSetpoint { get; protected set; }
        public float GridVoltage { get; protected set; }
        public float XCoilCurrent { get; protected set; }
        public float YCoilCurrent { get; protected set; }
        public float FocusCurrent { get; protected set; }
        public float IonPumpFeedback { get; protected set; }
        public float WaterPressure { get; protected set; }
        public float WaterFlowRate { get; protected set; }
        public float WaterTemperature { get; protected set; }
        public float HeatSinkTemperature { get; protected set; }
        public float PeltierTemperature { get; protected set; }
        public float CabinetTemperature { get; protected set; }
        public float[] Mag1 { get; protected set; } = null!;
        public float[] Mag2 { get; protected set; } = null!;
        public uint Applicator { get; protected set; } = 0;

        public override string ToString()
        {
            var properties = GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(float) || property.PropertyType == typeof(double))
                {
                    var value = property.GetValue(this);
                    sb.AppendLine($"{property.Name}: {string.Format("{0:F3}", value)}");
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(uint) || property.PropertyType == typeof(ulong))
                {
                    var value = property.GetValue(this);
                    sb.AppendLine($"{property.Name}: {string.Format("0x{0:X}", value)}");
                }
                else if (property.PropertyType.IsArray)
                {
                    var value = (float[])property.GetValue(this)!;
                    var values = string.Join(", ", value.Select(x => string.Format("{0:F}", x)));
                    sb.AppendLine($"{property.Name}: [{values}]");
                }
                else
                {
                    var value = property.GetValue(this);
                    sb.AppendLine($"{property.Name}: {value}");
                }
            }
            return sb.ToString();
        }

        public string GetVerticallyFormattedString()
        {
            var properties = GetType().GetProperties();

            // Determine the maximum property name length for alignment
            int maxNameLength = properties.Max(p => p.Name.Length);

            var sb = new StringBuilder();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(float) || property.PropertyType == typeof(double))
                {
                    var value = property.GetValue(this);
                    sb.AppendLine($"{property.Name.PadRight(maxNameLength)}: {string.Format("{0:F}", value)}");
                }
                else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(uint) || property.PropertyType == typeof(ulong))
                {
                    var value = property.GetValue(this);
                    sb.AppendLine($"{property.Name.PadRight(maxNameLength)}: {string.Format("0x{0:X}", value)}");
                }
                else if (property.PropertyType.IsArray)
                {
                    var value = (float[])property.GetValue(this)!;
                    var values = string.Join(", ", value.Select(x => string.Format("{0:F}", x)));
                    sb.AppendLine($"{property.Name.PadRight(maxNameLength)}: [{values}]");
                }
                else
                {
                    var value = property.GetValue(this);
                    sb.AppendLine($"{property.Name.PadRight(maxNameLength)}: {value}");
                }
            }
            return sb.ToString();

        }

        public static SystemTelemetry Parse(byte[] data)
        {
            SystemTelemetry telemetry = new();

            UdpPacket packet = new UdpPacket(data);
            if (packet.PacketType != (uint)GCBPacketType.TelemetryResponse
                || packet.PayloadLength < (uint)GCBTelemetryResponseField.kVSetpoint) // we have mandatory fields and new (optional) fields
            {
                throw new ArgumentException("Invalid telemetry packet");
            }

            UdpPacketIterator packetIterator = new(packet);

            telemetry.ControlBoardState = (GcbStateNew)(int)packetIterator.First();
            telemetry.SystemRuntime = packetIterator.Next();
            telemetry.FaultFlags = packetIterator.Next();
            telemetry.InterlockFlags = packetIterator.Next();
            telemetry.RingLedState = (RingLedState)(int)packetIterator.Next();
            telemetry.BaseLedState = (BaseLedState)(int)packetIterator.Next();

            telemetry.CollimatorId1 = packetIterator.Next();
            telemetry.CollimatorId2 = packetIterator.Next();
            telemetry.CollimatorSerial = (ulong)(telemetry.CollimatorId2) << 32 | telemetry.CollimatorId1;

            telemetry.ButtonsState = packetIterator.Next();
            telemetry.CurrentOperationalPoint = packetIterator.Next();
            telemetry.TotalOperationalPoints = packetIterator.Next();
            telemetry.InternalTimerState = packetIterator.Next();
            telemetry.PrimaryTimerValue = packetIterator.Next();

            telemetry.Timer1State = packetIterator.Next();
            telemetry.SecondaryTimer1Value = packetIterator.Next();
            telemetry.Timer2State = packetIterator.Next();
            telemetry.SecondaryTimer2Value = packetIterator.Next();
            telemetry.RuntimeCounterHVPS = packetIterator.Next();
            telemetry.HvpsIOStatus = packetIterator.Next();
            telemetry.HvpsFlagStatus = packetIterator.Next();
            telemetry.KvFeedback = packetIterator.Next();
            telemetry.EmissionCurrent = packetIterator.Next();
            telemetry.HeaterCurrentSetpoint = packetIterator.Next();
            telemetry.HeaterCurrentFeedback = packetIterator.Next();
            telemetry.GridSetpoint = packetIterator.Next();
            telemetry.GridVoltage = packetIterator.Next();
            telemetry.XCoilCurrent = packetIterator.Next();
            telemetry.YCoilCurrent = packetIterator.Next();
            telemetry.FocusCurrent = packetIterator.Next();
            telemetry.IonPumpFeedback = packetIterator.Next();
            telemetry.WaterPressure = packetIterator.Next();
            telemetry.WaterFlowRate = packetIterator.Next();
            telemetry.WaterTemperature = packetIterator.Next();
            telemetry.HeatSinkTemperature = packetIterator.Next();
            telemetry.PeltierTemperature = packetIterator.Next();
            telemetry.CabinetTemperature = packetIterator.Next();

            var mag1x = packetIterator.Next();
            var mag1y = packetIterator.Next();
            var mag1z = packetIterator.Next();
            telemetry.Mag1 = [mag1x, mag1y, mag1z];

            var mag2x = packetIterator.Next();
            var mag2y = packetIterator.Next();
            var mag2z = packetIterator.Next();
            telemetry.Mag2 = [mag2x, mag2y, mag2z];

            telemetry.Applicator = packetIterator.Next();
            if (packet.PayloadLength > (int)GCBTelemetryResponseField.kVSetpoint)
            {
                telemetry.KvSetpoint = packetIterator.Next();
            }
            if (packet.PayloadLength > (int)GCBTelemetryResponseField.emissionCurrentLimit)
            {
                telemetry.EmissionCurrentLimit = packetIterator.Next();
            }
            if (packet.PayloadLength > (int)GCBTelemetryResponseField.hvpsPowerSetpoint)
            {
                telemetry.HvpsPowerSetpoint = packetIterator.Next();
            }
            // Debug.WriteLine($"Applicator = {telemetry.Applicator.ToString("X")}");
            return telemetry;
        }

        public static bool IsFaultState(GcbStateNew? state)
        {
            return state is 
                GcbStateNew.Fault or
                GcbStateNew.ColdFault or 
                GcbStateNew.WarmupFault;
        }

        public static bool IsEmissionState(GcbStateNew? state)
        {
            return state is 
                GcbStateNew.Emission or
                GcbStateNew.Imaging;
        }

        public bool IsFaultState()
        {
            return IsFaultState(ControlBoardState);
        }

        public bool IsEmissionState()
        {
            return IsEmissionState(ControlBoardState);
        }
    }
}
