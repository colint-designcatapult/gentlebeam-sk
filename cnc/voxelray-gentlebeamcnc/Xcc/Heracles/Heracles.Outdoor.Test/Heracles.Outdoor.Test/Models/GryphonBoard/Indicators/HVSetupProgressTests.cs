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
        private const float KvSetpoint = 50.0f;
        private HVSetupProgress progress;

        private static ISystemTelemetry MakeSystemTelemetry(
            GcbStateNew state,
            float kvFeedback) =>
            new SystemNormalTelemetry
            {
                ControlBoardState = state,
                KvFeedback = kvFeedback,
            };

        [SetUp]
        public void Setup()
        {
            var mainBoard = new Mock<IMainBoardModel>();
            mainBoard.SetupGet(model => model.CurrentEmission)
                .Returns(new GcbOperationalPoint { SetpointKv = KvSetpoint });
            progress = new HVSetupProgress(mainBoard.Object);
        }

        [Test]
        public void IgnoresMissingAndNonSetupTelemetry()
        {
            progress.OnSystemTelemetryChanged(null);
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Cold, 0));

            Assert.That(progress.Value, Is.Zero);
        }

        [Test]
        public void ReportsScalarEmissionSetupProgress()
        {
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.HVSetup, KvSetpoint / 2));

            Assert.That(progress.Value, Is.EqualTo(50).Within(1));
        }

        [Test]
        public void ResetsAfterSetup()
        {
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.HVSetup, KvSetpoint / 2));
            progress.OnSystemTelemetryChanged(
                MakeSystemTelemetry(GcbStateNew.Ready, KvSetpoint));

            Assert.That(progress.Value, Is.Zero);
        }
    }
}
