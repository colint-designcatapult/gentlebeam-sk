using System;
using System.Collections.Generic;
using System.Linq;

namespace Xcc.Core.Domain.GryphonBoard
{
    public class GcbEmissionPlan
    {
        private IList<GcbOperationalPoint> _points;
        public IEnumerable<GcbOperationalPoint> Points { get => _points; }
        public int TotalPoints { get => _points.Count; }

        public double TotalTime => Points.Sum(pt => pt.TotalPointTime);
        public double RemainingTime => Points.Sum(pt => pt.RemainingPointTime);

        public float FilamentSetpoint => Points.First().FilamentSetpoint;

        public event EventHandler<GcbOperationalPoint> OperationalPointChanged = null!;

        public GcbEmissionPlan()
        {
            _points = new List<GcbOperationalPoint>();
        }

        public GcbEmissionPlan(IEnumerable<GcbOperationalPoint> points)
            : this()
        {
            foreach (var point in points)
            {
                AddPoint(point);
            }
        }

        public GcbOperationalPoint this[int index]
        {
            get
            {
                return _points[index];
            }
        }

        public int AddPoint(GcbOperationalPoint point)
        {
            if (point.PointIndex != TotalPoints)
            {
                throw new ArgumentException(
                    $"GcbEmissionPlan.AddPoint: Invalid operational point index: {point.PointIndex}, expected: {TotalPoints}");
            }
            // Workaround for proper remaining point time update from the timers:
            // ensure that at startup the point has initial remaining time equal to the remaining time specified from outside.
            point.InitialRemainingPointTime = point.RemainingPointTime;
            _points.Add(point);
            return point.PointIndex;
        }

        public void UpdatePoint(GcbOperationalPoint point)
        {
            if (point.PointIndex >= TotalPoints)
            {
                throw new ArgumentException($"GcbEmissionPlan.UpdatePoint error. Invalid point index: {point.PointIndex} out of {TotalPoints}");
            }
            var pointToChange = _points[point.PointIndex];
            if (point.IsSamePoint(pointToChange))
            {
                _points[point.PointIndex] = point;
                OperationalPointChanged?.Invoke(this, point);
            }
            else
            {
                throw new ArgumentException("GcbEmissionPlan.UpdatePoint error. Cannot update the point value: point is not the same");
            }
        }

        public bool IsSameAs(GcbEmissionPlan otherPlan)
        {
            if (otherPlan is null || otherPlan.TotalPoints != TotalPoints)
                return false;

            for (int i = 0; i < TotalPoints; i++)
            {
                var point = otherPlan[i];

                if (this[i].IsSamePoint(point) == false)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsCompleted(float fieldRemainingTimeThreshold)
        {
            return _points.Max(f => f.RemainingPointTime) < fieldRemainingTimeThreshold;
        }
    }
}
