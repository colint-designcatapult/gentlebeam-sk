using Xcc.Infra.Persistence.DataAccess.gRPC;

namespace Xcc.Test.Xcc.Application.Protos
{
    internal class ProtoTypesConverterTests
    {
        [Test]
        public void DateInLocalTime_ConversionTest()
        {
            DateTime dateTimeNow = DateTime.Now;
            var timestamp = ProtoTypesConverter.ToTimestamp(dateTimeNow);

            DateTime dateTimeBackUTC = ProtoTypesConverter.FromTimestamp(timestamp);
            Assert.Multiple(() =>
            {
                // Back conversion is always in UTC
                Assert.That(dateTimeBackUTC.Kind, Is.EqualTo(DateTimeKind.Local));
                Assert.That(dateTimeBackUTC.ToLocalTime(), Is.EqualTo(dateTimeNow));
            });
        }

        [Test]
        public void DateBeforeUnix_ConversionTest()
        {
            DateTime beforeUnix = new DateTime(1950, 12, 31);
            var timestamp = ProtoTypesConverter.ToTimestamp(beforeUnix);

            DateTime dateTimeBack = ProtoTypesConverter.FromTimestamp(timestamp);
            Assert.Multiple(() =>
            {
                Assert.That(dateTimeBack.Kind, Is.EqualTo(DateTimeKind.Local));
                Assert.That(dateTimeBack.ToLocalTime(), Is.EqualTo(beforeUnix));
            });
        }

        [Test]
        public void DateOfBirth_TimeChangeLine_ConversionTest()
        {
            DateOnly dob = new DateOnly(1950, 12, 31);

            // Get a local timezone from UTC-* ones to imitate ToLocalTime conversion in such zone:
            TimeSpan offset = new TimeSpan(hours: -6, minutes: 0, seconds: 0);
            var timezone = TimeZoneInfo.GetSystemTimeZones().First(tz => tz.BaseUtcOffset < offset);

            var timestamp = ProtoTypesConverter.ToTimestamp(dob);

            // Now convert back and check what's there:
            DateTime dobTimeLocalZone = TimeZoneInfo.ConvertTimeFromUtc(timestamp.ToDateTime(), timezone);
            DateOnly dobLocalZone = DateOnly.FromDateTime(dobTimeLocalZone);

            DateOnly dobBack = ProtoTypesConverter.DateFromTimestamp(timestamp);

            Assert.Multiple(() =>
            {
                Assert.That(dobLocalZone, Is.Not.EqualTo(dob)); // UTC-N timezone conversion is wrong
                Assert.That(dobBack, Is.EqualTo(dob)); // while our should be fine
            });
        }
    }
}
