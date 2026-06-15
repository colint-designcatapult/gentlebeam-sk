using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    internal class GcbEmissionPlanTests
    {
        private GcbEmissionPlan plan;

        [SetUp]
        public void SetUp() 
        { 
            plan = new GcbEmissionPlan();
        }

        [Test]
        public void ConstructedEmptyTest()
        {
            Assert.Multiple(() =>
            {
                Assert.That(plan.TotalPoints, Is.EqualTo(0));
                Assert.That(plan.Points, Is.Empty);
            });
        }

        [Test]
        public void AddPoint_PositiveTest()
        {
            var point = new GcbOperationalPoint { PointIndex = 0 };
            Assert.DoesNotThrow(() => plan.AddPoint(point));
            Assert.Multiple(() =>
            {
                Assert.That(plan.TotalPoints, Is.EqualTo(1));
                Assert.That(plan[0], Is.EqualTo(point));
            });
        }

        [Test]
        public void AddPoint_NegativeTest()
        {
            var point = new GcbOperationalPoint { PointIndex = 1 };
            Assert.Throws<ArgumentException>(() => plan.AddPoint(point));
        }


        [Test]
        public void UpdatePoint_PositiveTest()
        {
            var point = new GcbOperationalPoint { PointIndex = 0 };
            plan.AddPoint(point);

            point.RemainingPointTime += 1.0f;

            Assert.DoesNotThrow(() => plan.UpdatePoint(point));
            Assert.That(plan[0], Is.EqualTo(point));
        }

        [Test]
        public void UpdatePoint_NegativeTest()
        {
            var point = new GcbOperationalPoint { PointIndex = 0 };
            plan.AddPoint(point);

            var newValueWithWrongIndex = point; 
            newValueWithWrongIndex.PointIndex += 1;
            
            Assert.Throws<ArgumentException>(() => plan.UpdatePoint(newValueWithWrongIndex));

            var notSamePoint = point;
            notSamePoint.TotalPointTime += 1.0f;
            Assert.Throws<ArgumentException>(() => plan.UpdatePoint(notSamePoint));
        }

        [Test]
        public void IsSameAs_PositiveTest()
        {
            var point = new GcbOperationalPoint { PointIndex = 0 };
            plan.AddPoint(point);

            var newPlan = new GcbEmissionPlan();
            var sameAlteredPoint = point; point.RemainingPointTime -= 0.5f;
            newPlan.AddPoint(sameAlteredPoint);

            Assert.That(plan.IsSameAs(newPlan), Is.True);
        }

        [Test]
        public void IsSameAs_NegativeTest()
        {
            var point = new GcbOperationalPoint { PointIndex = 0 };
            plan.AddPoint(point);

            Assert.That(plan.IsSameAs(null!), Is.False);

            var newPlan = new GcbEmissionPlan();
            Assert.That(plan.IsSameAs(newPlan), Is.False);

            var differentPoint = point;
            differentPoint.RemainingPointTime -= 0.5f;
            differentPoint.TotalPointTime -= 0.1f;
            newPlan.AddPoint(differentPoint);

            Assert.That(plan.IsSameAs(newPlan), Is.False);
        }

        [Test]
        public void IsCompleted_PositiveTest()
        {
            float remainingTimeThreshold = 0.1f;
            float remainingTimeBelowThreshold = remainingTimeThreshold / 2;
            plan.AddPoint(new GcbOperationalPoint { PointIndex = 0, RemainingPointTime = remainingTimeBelowThreshold });
            plan.AddPoint(new GcbOperationalPoint { PointIndex = 1, RemainingPointTime = remainingTimeBelowThreshold/2 });
            
            Assert.That(plan.IsCompleted(remainingTimeThreshold), Is.True);
        }

        [Test]
        public void IsCompleted_NegativeTest()
        {
            float remainingTimeThreshold = 0.1f;
            float remainingTimeBelowThreshold = remainingTimeThreshold / 2;
            plan.AddPoint(new GcbOperationalPoint { PointIndex = 0, RemainingPointTime = remainingTimeBelowThreshold});
            plan.AddPoint(new GcbOperationalPoint { PointIndex = 1, RemainingPointTime = remainingTimeBelowThreshold * 2 });

            Assert.That(plan.IsCompleted(remainingTimeThreshold), Is.False);
        }
        
        [Test]
        public void TotalTime()
        {
            var sut = new GcbEmissionPlan();

            sut.AddPoint(new GcbOperationalPoint { PointIndex = 0, TotalPointTime = 1.5f });
            sut.AddPoint(new GcbOperationalPoint { PointIndex = 1, TotalPointTime = 2.5f });
            sut.AddPoint(new GcbOperationalPoint { PointIndex = 2, TotalPointTime = 3.0f });

            Assert.That(sut.TotalTime, Is.EqualTo(7.0f).Within(G.Precision));
        }

        [Test]
        public void RemainingTime()
        {
            var sut = new GcbEmissionPlan();

            sut.AddPoint(new GcbOperationalPoint { PointIndex = 0, RemainingPointTime = 2.0f });
            sut.AddPoint(new GcbOperationalPoint { PointIndex = 1, RemainingPointTime = 1.5f });
            sut.AddPoint(new GcbOperationalPoint { PointIndex = 2, RemainingPointTime = 0.5f });

            Assert.That(sut.RemainingTime, Is.EqualTo(4.0).Within(G.Precision));
        }

        [Test]
        public void FilamentSetpoint()
        {
            var sut = new GcbEmissionPlan();

            sut.AddPoint(new GcbOperationalPoint { PointIndex = 0, FilamentSetpoint = 4.2f });
            sut.AddPoint(new GcbOperationalPoint { PointIndex = 1, FilamentSetpoint = 2.8f });
            sut.AddPoint(new GcbOperationalPoint { PointIndex = 2, FilamentSetpoint = 1.5f });

            Assert.That(sut.FilamentSetpoint, Is.EqualTo(4.2).Within(G.Precision));
        }
    }
}
