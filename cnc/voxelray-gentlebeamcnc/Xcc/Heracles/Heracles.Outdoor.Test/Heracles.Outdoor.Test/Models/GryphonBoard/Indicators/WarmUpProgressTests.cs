using Empyrean.Common.Infra.Networking.Udp;
using Moq;
using NUnit.Framework;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Outdoor.Test.Models.GryphonBoard.Indicators
{
    internal class WarmUpProgressTests
    {
        private const float WARMUP_SETPOINT_VALUE = 3500.0f;
        private Mock<INotifyWarmupEvent> mockWarmupEventSource;
        private Mock<ILogWriter> mockLogWriter;

        public WarmUpProgress WarmUpProgress { get; private set; }

        private ISystemTelemetry MakeSystemTelemetry(GcbStateNew state, float filamentSetpoint)
        {
            var packet = new UdpPacket(
                packetType: (uint)GCBPacketType.TelemetryResponse,
                packetCounter: 0,
                payloadLength: (uint)GCBTelemetryResponseField.PayloadFields);
            packet[(int)GCBTelemetryResponseField.SystemState] = (int)state;
            packet[(int)GCBTelemetryResponseField.FilamentSetpoint] = filamentSetpoint;
            packet.UpdateCRC();
            return SystemTelemetry.Parse(packet.Buffer);
        }
        private GcbStateNew GetWarmupState(WarmupType warmupType)
        {
            return warmupType switch {
                WarmupType.Fast => GcbStateNew.Warmup,
                WarmupType.Full => GcbStateNew.DailyWarmup,
                _ => throw new NotImplementedException()
            };
        }


        [SetUp]
        public void Setup()
        {
            mockWarmupEventSource = new();
            mockLogWriter = new();
            WarmUpProgress = new WarmUpProgress(mockWarmupEventSource.Object, mockLogWriter.Object);
        }

        [Test]
        public void ConstructorTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(WarmUpProgress.WarmupType, Is.EqualTo(WarmupType.Fast));
                Assert.That(WarmUpProgress.WarmupSetpoint, Is.EqualTo(0));
                Assert.That(WarmUpProgress.Value, Is.EqualTo(0));
            });
        }

        [Test]
        public void NoWarmupTelemetryTest()
        {
            WarmUpProgress.Reset(WarmupParameters.FastWarmup(WARMUP_SETPOINT_VALUE));

            WarmUpProgress.OnSystemTelemetryChanged(null);
            Assert.That(WarmUpProgress.Value, Is.EqualTo(0));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Cold, 0));
            Assert.That(WarmUpProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void LaunchingTelemetryZeroSetpointTest()
        {
            WarmUpProgress.Reset(WarmupParameters.FastWarmup(WARMUP_SETPOINT_VALUE));
            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Warmup, 0));
            Assert.That(WarmUpProgress.Value, Is.EqualTo(0));
        }

        [TestCase(WarmupType.Fast)]
        [TestCase(WarmupType.Full)]
        public void ProgressIncrementTest(WarmupType warmupType)
        {
            float initialSetpoint = 1000;
            float setpoint50percent = initialSetpoint + (WARMUP_SETPOINT_VALUE - initialSetpoint) / 2;
            var warmupState = GetWarmupState(warmupType);

            WarmUpProgress.Reset(warmupType == WarmupType.Fast
                ? WarmupParameters.FastWarmup(WARMUP_SETPOINT_VALUE)
                : WarmupParameters.Conditioning(WARMUP_SETPOINT_VALUE));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(warmupState, initialSetpoint));
            Assert.That(WarmUpProgress.Value, Is.EqualTo(0));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(warmupState, initialSetpoint + 100));
            Assert.That(WarmUpProgress.Value, Is.GreaterThan(0));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(warmupState, setpoint50percent));
            Assert.That(Math.Abs(WarmUpProgress.Value - 50), Is.LessThan(1)); // value is close to 50%
        }

        [Test]
        public void ResetAfterWarmupTest()
        {
            float initialSetpoint = 1500;
            float setpoint50percent = initialSetpoint + (WARMUP_SETPOINT_VALUE - initialSetpoint) / 2;
            
            WarmUpProgress.Reset(WarmupParameters.FastWarmup(WARMUP_SETPOINT_VALUE));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Warmup, initialSetpoint));
            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Warmup, initialSetpoint + 100));
            Assert.That(WarmUpProgress.Value, Is.GreaterThan(0));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Primed, initialSetpoint + 100));
            Assert.That(WarmUpProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void WarmupTelemetryOverflowTest()
        {
            float initialSetpoint = 1000;

            WarmUpProgress.Reset(WarmupParameters.FastWarmup(WARMUP_SETPOINT_VALUE));

            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Warmup, initialSetpoint));
            WarmUpProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Warmup, WARMUP_SETPOINT_VALUE * 2));
            Assert.That(WarmUpProgress.Value, Is.EqualTo(100));
        }

    }
}