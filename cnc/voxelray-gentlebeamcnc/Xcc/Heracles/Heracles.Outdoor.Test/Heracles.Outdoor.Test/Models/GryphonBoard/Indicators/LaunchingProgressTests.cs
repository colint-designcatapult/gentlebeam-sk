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
        private const float FilamentSetpoint = 3500.0f;
        private LaunchingProgress progress;

        private static ISystemTelemetry MakeSystemTelemetry(
            GcbStateNew state,
            float filamentFeedback) =>
            new SystemNormalTelemetry
            {
                ControlBoardState = state,
                HeaterCurrentFeedback = filamentFeedback,
            };

        [SetUp]
        public void Setup()
        {
            var mainBoard = new Mock<IMainBoardModel>();
            mainBoard.SetupGet(model => model.CurrentEmission)
                .Returns(new GcbOperationalPoint
                {
                    FilamentSetpoint = FilamentSetpoint
                });
            progress = new LaunchingProgress(mainBoard.Object);
        }

        [Test]
        public void IgnoresMissingAndNonLaunchingTelemetry()
        {
            progress.OnSystemTelemetryChanged(null);
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Cold, 0));

            Assert.That(progress.Value, Is.Zero);
        }

        [Test]
        public void ReportsScalarEmissionLaunchingProgress()
        {
            const float initial = 2500;
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Launching, initial));
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(
                    GcbStateNew.Launching,
                    initial + (FilamentSetpoint - initial) / 2));

            Assert.That(progress.Value, Is.EqualTo(50).Within(1));
        }

        [Test]
        public void RetainsCompletedProgressThroughDischarge()
        {
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Launching, 2500));
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Emission, 2600));
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Discharge, 2600));

            Assert.That(progress.Value, Is.EqualTo(100));
        }
    }
}