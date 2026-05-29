using Empyrean.Common.Infra.Networking.Udp;
using NUnit.Framework;
using Xcc.Application.Domain.GryphonBoard.Model.OperationGuards;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Outdoor.Test.Models.GryphonBoard.OperationGuards
{
    internal class ClearFaultsEnergyGuardTests
    {
        ClearFaultsEnergyGuard Watchdog { get; set; }

        [SetUp]
        public void Setup()
        {
            Watchdog = new ClearFaultsEnergyGuard();
        }

        [Test]
        public void CannotClearOnOtherStates()
        {
            Watchdog.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Staged, energy: 0));
            Assert.That(Watchdog.CanClearErrors, Is.False);
        }

        [Test]
        public void CanClearOnFault()
        {
            Watchdog.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Fault, energy: Watchdog.SAFE_WARM_FAULT_KV_THRESHOLD - 1));
            Assert.That(Watchdog.CanClearErrors, Is.True);
        }

        [Test]
        public void CannotClearOnFaultWithHighKv()
        {
            Watchdog.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Fault, energy: Watchdog.SAFE_WARM_FAULT_KV_THRESHOLD + 1));
            Assert.That(Watchdog.CanClearErrors, Is.False);
        }

        private ISystemTelemetry MakeSystemTelemetry(GcbStateNew state, float energy)
        {
            var packet = new UdpPacket(
                packetType: (uint)GCBPacketType.TelemetryResponse,
                packetCounter: 0,
                payloadLength: (uint)GCBTelemetryResponseField.PayloadFields);
            packet[(int)GCBTelemetryResponseField.SystemState] = (int)state;
            packet[(int)GCBTelemetryResponseField.kVFeedback] = energy;
            packet.UpdateCRC();
            return SystemTelemetry.Parse(packet.Buffer);
        }
    }
}
