using System;
using System.Collections.Generic;
using System.Linq;

namespace Heracles.Robot.Models.RobotArm
{
    public class JointsPosition
    {
        public JointsPosition()
        {
            JArray = new List<double>() { 0, 0, 0, 0, 0, 0 };
        }

        public bool IsEqualTo(JointsPosition otherPosition, double precision = double.Epsilon)
        {
            if (otherPosition == null) throw new ArgumentNullException(nameof(otherPosition));
            else if (otherPosition.JArray.Count != JArray.Count)
                throw new ArgumentException($"JointsPosition error. Cannot compare, array size mismatch - expected {JArray.Count} items, actual: {otherPosition.JArray.Count}");
            if (precision <= double.Epsilon) throw new ArgumentOutOfRangeException(nameof(precision));

            return JArray.Zip(otherPosition.JArray).Select(x => Math.Abs(x.First - x.Second)).All(x => x <= precision);
        }

        public JointsPosition(IList<double> jointArray)
        {
            if (jointArray == null)
            {
                throw new ArgumentNullException(nameof(jointArray));
            }
            JArray = jointArray;
        }

        public IList<double> JArray { get; private set; }

        public override string ToString()
        {
            if (JArray == null)
                return string.Empty;

            string s = string.Empty;
            for (int i = 0; i < JArray.Count; i++)
            {
                if (i > 0) s += ", ";
                s += $"{JArray[i]:0.00}";
            }

            return s;
        }
    }
}
