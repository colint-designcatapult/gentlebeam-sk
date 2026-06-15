using Google.Protobuf.WellKnownTypes;
using System;

namespace Xcc.Infra.Persistence.DataAccess.gRPC
{
    public class ProtoTypesConverter
    {
        /// <summary>
        /// Converts calendar dates to UTC timestamps
        /// Should not be mixed with regular ToTimestamp conversion, 
        /// as its aim is to ensure that the date is passed as is, with no date change due to timezone issues
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static Timestamp ToTimestamp(DateOnly date)
        {
            var datetimeTreatedAsUtc = date.ToDateTime(new TimeOnly(), DateTimeKind.Utc);
            return Timestamp.FromDateTime(datetimeTreatedAsUtc);
        }

        /// <summary>
        /// Converts timestamps to Google Protobuf format.
        /// Ensures that any local time will be converted to UTC kind first
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public static Timestamp ToTimestamp(DateTime datetime)
        {
            var utcDatetime = datetime.ToUniversalTime();
            return Timestamp.FromDateTime(utcDatetime);
        }

        /// <summary>
        /// Converts Google Protobuf timestamp to local DateTime
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime FromTimestamp(Timestamp timestamp)
        {
            var utcTime = timestamp.ToDateTime();
            return utcTime.ToLocalTime();
        }

        public static DateOnly DateFromTimestamp(Timestamp timestamp)
        {
            return DateOnly.FromDateTime(timestamp.ToDateTime());
        }
    }
}
