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
                PointIndex = 0,
                TotalPointTime = 1,
                RemainingPointTime = 1,
                SetpointKv = 50,
                TargetMA = 1.0f,
                FilamentSetpoint = 3700,
                XCoilSetpoint = 0,
                YCoilSetpoint = 0,
                FocusCoilSetpoint = 0,
                AutoExecution = false
            };
        }

        [Test]
        public void EqualsTest()
        {
            GcbOperationalPoint equalPoint = operationalPoint;

            Assert.That(operationalPoint.Equals(equalPoint), Is.True);

            GcbOperationalPoint nonEqualPoint = operationalPoint;
            nonEqualPoint.AutoExecution = !operationalPoint.AutoExecution;
            Assert.That(operationalPoint.Equals(nonEqualPoint), Is.False);
        }

        [Test]
        public void IsSame_PositiveTest()
        {
            GcbOperationalPoint samePoint = operationalPoint;

            Assert.That(operationalPoint.IsSamePoint(samePoint), Is.True);

            samePoint.RemainingPointTime = samePoint.RemainingPointTime - 0.5f;
            Assert.That(operationalPoint.IsSamePoint(samePoint), Is.True);
        }

        [Test]
        public void IsSame_NegativeTest()
        {
            GcbOperationalPoint otherPoint = operationalPoint;

            otherPoint.PointIndex += 1;
            Assert.That(operationalPoint.IsSamePoint(otherPoint), Is.False);

            otherPoint = operationalPoint;
            otherPoint.AutoExecution = !otherPoint.AutoExecution;
            Assert.That(operationalPoint.IsSamePoint(otherPoint), Is.False);
        }

        [Test]
        public void ActualDurationTest()
        {
            GcbOperationalPoint point = operationalPoint;
            float elapsedTime = 0.5f;
            point.RemainingPointTime = point.TotalPointTime - elapsedTime;
            Assert.That(point.ActualDuration, Is.EqualTo(elapsedTime));
        }
        
        [Test]
        public void GettersSetters(
            [Values(0, 1)] int pointIndex,
            [Values(0.1f, 1.1f)] float totalPointTime,
            [Values(0.2f, 1.2f)] float initialRemainingPointTime,
            [Values(0.3f, 1.3f)] float remainingPointTime,
            [Values(0.4f, 1.4f)] float setpointKv,
            [Values(0.5f, 1.5f)] float targetMA,
            [Values(0.6f, 1.6f)] float filamentSetpoint,
            [Values(0.7f, 1.7f)] float xCoilSetpoint,
            [Values(0.8f, 1.8f)] float yCoilSetpoint,
            [Values(0.9f, 1.9f)] float focusCoilSetpoint,
            [Values(false, true)] bool autoExecution)
        {
            var sut = new GcbOperationalPoint
            {
                PointIndex = pointIndex,
                TotalPointTime = totalPointTime,
                InitialRemainingPointTime = initialRemainingPointTime,
                RemainingPointTime = remainingPointTime,
                SetpointKv = setpointKv,
                TargetMA = targetMA,
                FilamentSetpoint = filamentSetpoint,
                XCoilSetpoint = xCoilSetpoint,
                YCoilSetpoint = yCoilSetpoint,
                FocusCoilSetpoint = focusCoilSetpoint,
                AutoExecution = autoExecution,
            };
            
            Assert.That(sut.PointIndex, Is.EqualTo(pointIndex));
            Assert.That(sut.TotalPointTime, Is.EqualTo(totalPointTime).Within(G.Precision));
            Assert.That(sut.InitialRemainingPointTime, Is.EqualTo(initialRemainingPointTime).Within(G.Precision));
            Assert.That(sut.RemainingPointTime, Is.EqualTo(remainingPointTime).Within(G.Precision));
            Assert.That(sut.SetpointKv, Is.EqualTo(setpointKv).Within(G.Precision));
            Assert.That(sut.TargetMA, Is.EqualTo(targetMA).Within(G.Precision));
            Assert.That(sut.FilamentSetpoint, Is.EqualTo(filamentSetpoint).Within(G.Precision));
            Assert.That(sut.XCoilSetpoint, Is.EqualTo(xCoilSetpoint).Within(G.Precision));
            Assert.That(sut.YCoilSetpoint, Is.EqualTo(yCoilSetpoint).Within(G.Precision));
            Assert.That(sut.FocusCoilSetpoint, Is.EqualTo(focusCoilSetpoint).Within(G.Precision));
            Assert.That(sut.AutoExecution, Is.EqualTo(autoExecution));
        }
    }
}
