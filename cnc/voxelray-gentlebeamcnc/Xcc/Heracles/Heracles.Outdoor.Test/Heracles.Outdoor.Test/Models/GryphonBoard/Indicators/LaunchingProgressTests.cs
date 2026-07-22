using Moq;
using NUnit.Framework;
using Xcc.Application.Domain.GryphonBoard.Model.Indicators;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Helpers;
using Xcc.Infra.GryphonBoard;

namespace Heracles.Outdoor.Test.Models.GryphonBoard.Indicators
{
    internal class LaunchingProgressTests
    {
        private const float FILAMENT_SETPOINT_VALUE = 3500.0f;
        private static GcbEmissionPlan emissionPlan = MakeTestPlan(points: 3, filamentSetpoint: FILAMENT_SETPOINT_VALUE);
        private Mock<IMainBoardModel> mockMainBoardModel;
        public LaunchingProgress LaunchingProgress { get; private set; }

        private static GcbEmissionPlan MakeTestPlan(int points, float filamentSetpoint)
        {
            GcbEmissionPlan plan = new GcbEmissionPlan();
            for (int i = 0; i < points; i++)
            {
                plan.AddPoint(new GcbOperationalPoint { FilamentSetpoint = filamentSetpoint, PointIndex = i });
            }
            return plan;
        }

        private static ISystemTelemetry MakeSystemTelemetry(GcbStateNew state, int pointIndex, float filamentFeedback) =>
            new SystemNormalTelemetry
            {
                ControlBoardState = state,
                CurrentOperationalPoint = pointIndex,
                HeaterCurrentFeedback = filamentFeedback,
            };

        [SetUp]
        public void Setup()
        {
            mockMainBoardModel = new();
            mockMainBoardModel.SetupGet(m => m.CurrentPlan).Returns(emissionPlan);
            LaunchingProgress = new LaunchingProgress(mockMainBoardModel.Object);
        }

        [Test]
        public void NoLaunchingTelemetryTest()
        {
            LaunchingProgress.OnSystemTelemetryChanged(null);
            Assert.That(LaunchingProgress.Value, Is.EqualTo(0));

            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Cold, 0, 0));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void LaunchingTelemetryZeroSetpointTest()
        {
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, 0));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(0));
        }

        [Test]
        public void LaunchingTelemetryPositiveIncrementTest()
        {
            float initialSetpoint = 2500;
            float setpoint50percent = initialSetpoint + (FILAMENT_SETPOINT_VALUE - initialSetpoint) / 2;

            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(0));

            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint + 100));
            Assert.That(LaunchingProgress.Value, Is.GreaterThan(0));

            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, setpoint50percent));
            Assert.That(Math.Abs(LaunchingProgress.Value - 50), Is.LessThan(1)); // value is close to 50%
        }

        [Test]
        public void RetainsCompletedProgressThroughTerminationTest()
        {
            float initialSetpoint = 2500;

            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint));
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint + 100));
            Assert.That(LaunchingProgress.Value, Is.GreaterThan(0));

            // Doesn't reset in emission state
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Emission, 0, initialSetpoint + 100));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(100));

            // Termination preserves completed progress while hardware discharges
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Termination, 0, initialSetpoint + 100));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(100));

        }

        [Test]
        public void LaunchingTelemetryOverflowTest()
        {
            float initialSetpoint = 2500;
            float setpoint50percent = initialSetpoint + (FILAMENT_SETPOINT_VALUE - initialSetpoint) / 2;

            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint));
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, FILAMENT_SETPOINT_VALUE * 2));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(100));
        }

        [Test]
        public void LaunchingTelemetryPointSwitchTest()
        {
            float initialSetpoint = 2500;
            float setpoint50percent = initialSetpoint + (FILAMENT_SETPOINT_VALUE - initialSetpoint) / 2;

            // Set and update progress for point #0
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint));
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 0, initialSetpoint + 100));
            Assert.That(LaunchingProgress.Value, Is.GreaterThan(0));

            // Set progress for point #1
            LaunchingProgress.OnSystemTelemetryChanged(MakeSystemTelemetry(GcbStateNew.Launching, 1, initialSetpoint));
            Assert.That(LaunchingProgress.Value, Is.EqualTo(0));
        }
    }
}