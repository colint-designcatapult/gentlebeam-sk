using System;

namespace Xcc.Application.Helpers
{
    public static class DateTimeHelper
    {
        public static bool IsSameUtcDay(this DateTime a, DateTime b)
        {
            // Moses stores datetime in UTC format, so we need to compare dates in UTC format
            var aUtcDate = a.ToUniversalTime().Date;
            var bUtcDate = b.ToUniversalTime().Date;

            return aUtcDate.CompareTo(bUtcDate) == 0;
        }
    }
}
