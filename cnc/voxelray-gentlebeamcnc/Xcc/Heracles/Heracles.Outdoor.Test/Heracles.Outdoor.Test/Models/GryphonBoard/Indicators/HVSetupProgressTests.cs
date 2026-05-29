using Empyrean.Common.Infra.Networking.Udp;
using Moq;
using NUnit.Framework;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Outdoor.Test.Models.GryphonBoard.Indicators
{
    internal class HVSetupProgressTests
    {
        private const float KV_SETPOINT_VALUE = 50.0f;
        private static GcbEmissionPlan emissionPlan = MakeTestPlan(points: 3, setpointKv: KV_SETPOINT_VALUE);
        private Mock<IMainBoardModel> mockMainBoardModel;
        public HVSetupProgress HVSetupProgress { get; private set; }

        private static GcbEmissionPlan MakeTestPlan(int points, float setpointKv)
        {
            GcbEmissionPlan plan = new GcbEmissionPlan();
            for (int i = 0; i < points; i++)
            {
                plan.AddPoint(new GcbOperationalPoint { SetpointKv = setpointKv, PointIndex = i });
            }
            return plan;
        }

        private ISystemTelemetry MakeSystemTelemetry(GcbStateNew state, int pointIndex, float kvFeedback)
        {
            var packet = new UdpPacket(
                packetType: (uint)GCBPacketType.TelemetryResponse,
                packetCounter: 0,
                payloadLength: (uint)GCBTelemetryResponseField.PayloadFields);
            packet[(int)GCBTelemetryResponseField.SystemState] = (int)state;
            packet[(int)GCBTelemetryResponseField.CurrentPoint] = pointIndex;
            packet[(int)GCBTelemetryResponseField.kVFeedback] = kvFeedback;
            packet.UpdateCRC();
            return SystemTelemetry.Parse(packet.Buffer);
        }

        [SetUp]
        public void Setup()
        {
            mockMainBoardModel = new();
            mockMainBoardModel.SetupGet(m => m.CurrentPlan).Returns(emissionPlan);
            HVSetupProgress = new HVSetupProgress(mockMainBoardModel.Object);
        }

        [Test]
        public void NoLoadingTelemetryTest()
        {
            HVSetupProgress.OnSystemTelemetryChanged(null);
            Assert.That(HVSetupProgress.Value, Is.EqualTo(0));

            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Cold, 0, 0));
            Assert.That(HVSetupProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void HVSetupTelemetryZeroSetpointTest()
        {
            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 0, 0));
            Assert.That(HVSetupProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void HVSetupTelemetryPositiveIncrementTest()
        {
            float initial_kV = 0.1f;
            float setpoint50percent = KV_SETPOINT_VALUE / 2;

            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 0, initial_kV));
            Assert.That(HVSetupProgress.Value, Is.EqualTo(0));

            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 0, setpoint50percent));
            Assert.That(Math.Abs(HVSetupProgress.Value - 50), Is.LessThan(2)); // value is close to 50%
        }

        [Test]
        public void ResetAfterLaunchingTest()
        {
            float setpoint50percent = KV_SETPOINT_VALUE / 2;

            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 0, setpoint50percent));
            Assert.That(HVSetupProgress.Value, Is.GreaterThan(0));

            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Ready, 0, KV_SETPOINT_VALUE));
            Assert.That(HVSetupProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void HVSetupTelemetryPointSwitchTest()
        {
            float setpoint50percent = KV_SETPOINT_VALUE / 2;

            // Set and update progress for point #0
            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 0, 0));
            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 0, setpoint50percent));
            Assert.That(HVSetupProgress.Value, Is.GreaterThan(0));

            // Set progress for point #1
            HVSetupProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.HVSetup, 1, 0));
            Assert.That(HVSetupProgress.Value, Is.EqualTo(0));
        }
    }
}
