using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    internal class GcbOperationalPointTests
    {
        private GcbOperationalPoint operationalPoint;

        [SetUp]
        public void Setup()
        {
            operationalPoint = new GcbOperationalPoint
            {
                TotalPointTime = 1,
                RemainingPointTime = 1,
                SetpointKv = 50,
                TargetMA = 1.0f,
                FilamentSetpoint = 3700,
                XCoilSetpoint = 10,
                YCoilSetpoint = 20,
                FocusCoilSetpoint = 30
            };
        }

        [Test]
        public void EqualsIncludesRemainingTime()
        {
            Assert.That(operationalPoint.Equals(operationalPoint), Is.True);

            var changed = operationalPoint;
            changed.RemainingPointTime -= 0.5f;

            Assert.That(operationalPoint.Equals(changed), Is.False);
        }

        [Test]
        public void IsSamePointIgnoresRuntimeAndDeflectionValues()
        {
            var changed = operationalPoint;
            changed.RemainingPointTime -= 0.5f;
            changed.XCoilSetpoint += 1;
            changed.YCoilSetpoint += 1;

            Assert.That(operationalPoint.IsSamePoint(changed), Is.True);
        }

        [Test]
        public void IsSamePointRejectsDifferentSetpoint()
        {
            var changed = operationalPoint;
            changed.SetpointKv += 1;

            Assert.That(operationalPoint.IsSamePoint(changed), Is.False);
        }

        [Test]
        public void ActualDurationIsElapsedTime()
        {
            var point = operationalPoint;
            point.RemainingPointTime = 0.5f;

            Assert.That(point.ActualDuration, Is.EqualTo(0.5f));
        }

        [Test]
        public void ScalarPropertiesRoundTrip()
        {
            var sut = new GcbOperationalPoint
            {
                TotalPointTime = 0.1f,
                InitialRemainingPointTime = 0.2f,
                RemainingPointTime = 0.3f,
                SetpointKv = 0.4f,
                TargetMA = 0.5f,
                FilamentSetpoint = 0.6f,
                XCoilSetpoint = 0.7f,
                YCoilSetpoint = 0.8f,
                FocusCoilSetpoint = 0.9f
            };

            Assert.Multiple(() =>
            {
                Assert.That(sut.TotalPointTime, Is.EqualTo(0.1f).Within(G.Precision));
                Assert.That(sut.InitialRemainingPointTime, Is.EqualTo(0.2f).Within(G.Precision));
                Assert.That(sut.RemainingPointTime, Is.EqualTo(0.3f).Within(G.Precision));
                Assert.That(sut.SetpointKv, Is.EqualTo(0.4f).Within(G.Precision));
                Assert.That(sut.TargetMA, Is.EqualTo(0.5f).Within(G.Precision));
                Assert.That(sut.FilamentSetpoint, Is.EqualTo(0.6f).Within(G.Precision));
                Assert.That(sut.XCoilSetpoint, Is.EqualTo(0.7f).Within(G.Precision));
                Assert.That(sut.YCoilSetpoint, Is.EqualTo(0.8f).Within(G.Precision));
                Assert.That(sut.FocusCoilSetpoint, Is.EqualTo(0.9f).Within(G.Precision));
            });
        }
    }
}
