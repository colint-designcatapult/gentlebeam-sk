using Empyrean.Common.Infra.Logging;

namespace Empyrean.Common.Test.Infra.Logging
{
    public class TextLogRecordTests
    {
        [SetUp]
        public void SetUp()
        {
            G.SetupCulture();
        }
        
        [Test]
        public void Constructor1()
        {
            string message = "Test message";
            string severity = "Error";
            string type = "Application";

            var record = new TextLogRecord(message, severity, type);

            Assert.That(record.Message, Is.EqualTo(message));
            Assert.That(record.Severity, Is.EqualTo(severity));
            Assert.That(record.Type, Is.EqualTo(type));
            Assert.That(record.TimeStamp, Is.LessThanOrEqualTo(DateTime.Now)
                                     .And.GreaterThanOrEqualTo(DateTime.Now.AddSeconds(-1)));
        }
        
        [Test]
        public void Constructor2()
        {
            string message = "Test message";
            string severity = "Info";
            string type = "System";
            DateTime timestamp = new DateTime(2025, 5, 1, 12, 0, 0);

            var record = new TextLogRecord(message, severity, type, timestamp);

            Assert.That(record.Message, Is.EqualTo(message));
            Assert.That(record.Severity, Is.EqualTo(severity));
            Assert.That(record.Type, Is.EqualTo(type));
            Assert.That(record.TimeStamp, Is.EqualTo(timestamp));
        }
        
        [Test]
        public void ToString_Format()
        {
            string message = "Test message";
            string severity = "Info";
            string type = "System";
            DateTime timestamp = new DateTime(2025, 5, 1, 12, 30, 0);
            
            var record = new TextLogRecord(message, severity, type, timestamp);

            Assert.That(record.ToString(), Is.EqualTo("5/1/2025 12:30:00 PM   Info   System   Test message"));
        }
        
        [Test]
        public void Parse()
        {
            var logText = "5/1/2025 12:30:00 PM   Info   System   Test message";
            var record = TextLogRecord.Parse(logText);

            Assert.Multiple(() =>
            {
                Assert.That(record?.Message, Is.EqualTo("Test message"));
                Assert.That(record?.Severity, Is.EqualTo("Info"));
                Assert.That(record?.Type, Is.EqualTo("System"));
                Assert.That(record?.TimeStamp, Is.EqualTo(new DateTime(2025, 5, 1, 12, 30, 0)));
            });
        }
        
        [Test]
        public void Parse_And_ToString_AreEqual()
        {
            var logText = "5/1/2025 12:30:00 PM   Info   System   Test message";
            var record = TextLogRecord.Parse(logText);
            Assert.That(record?.ToString(), Is.EqualTo(logText));
        }
        
        [Test]
        public void Parse_EmptyString_ThrowsException(
            [Values(null, "", " ", "  ", "\t", "\n")] string? logText)
        {
            var res = TextLogRecord.Parse(logText);
            Assert.That(res, Is.Null);  
        }
        
        [Test]
        public void Parse_NotEnoughFields_ThrowsException(
            [Values("01.05.2025 12:30:00",
                    "01.05.2025 12:30:00   Info",
                    "01.05.2025 12:30:00   System   Test")] string logText)
        {
            var res = TextLogRecord.Parse(logText);
            Assert.That(res, Is.Null);
        }
        
        [Test]
        public void Parse_InvalidTimestamp_ThrowsException(
            [Values("AA.05.2025 12:30:00   Info   System   Test message",
                    "1   Info   System   Test message")] string logText)
        {
            var res = TextLogRecord.Parse(logText);
            Assert.That(res, Is.Null); 
        }
    }
}