using System;

namespace Xcc.Core.Helpers
{
    public class MathHelpers
    {
        public static double ConvertDegreesToRadians(double degrees)
        {
            return Math.PI / 180.0 * degrees;
        }
        public static double ConvertRadiansToDegrees(double radians)
        {
            return 180.0 / Math.PI * radians;
        }
    };
}
