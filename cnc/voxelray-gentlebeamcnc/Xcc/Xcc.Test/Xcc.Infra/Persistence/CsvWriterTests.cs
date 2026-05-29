using Xcc.Infra.Persistence.CSV;

namespace Xcc.Test.Xcc.Infra.Persistence
{
    internal class CsvWriterTests
    {
        MemoryStream _memoryStream;
        StreamWriter _streamWriter;
        CsvWriter _csvWriter;

        internal class TestObject
        {
            public string StringValue { get; set; } = "Value1";
            public string EmptyStringValue { get; set; } = string.Empty;
            public double DoubleValue { get; set; } = 1.0;
        }

        [SetUp]
        public void Setup()
        {
            _memoryStream = new();
            _streamWriter = new(_memoryStream);
            _csvWriter = new CsvWriter(_streamWriter);
        }

        [TearDown]
        public void Teardown()
        {
            _csvWriter.Dispose();
            _streamWriter.Dispose();
            _memoryStream.Dispose();
        }

        [Test]
        public void WriteTableTest()
        {
            IList<TestObject> objects = new List<TestObject>{ new TestObject(), new TestObject { DoubleValue = 2.0 } };
            const string tableName = "TestObjects";
            _csvWriter.WriteTable(tableName, objects);

            string writtenData = null!;
            _streamWriter.Flush();
            _memoryStream.Seek(0, SeekOrigin.Begin);
            using (var reader = new StreamReader(_memoryStream))
            {
                writtenData = reader.ReadToEnd();
            }
            Assert.That(writtenData, Is.Not.Null);
            var lines = writtenData.Split(Environment.NewLine);
            Assert.That(lines.First(), Is.EqualTo($"{tableName} Table:"));
            Assert.That(lines.Count, Is.EqualTo(5));
        }

        [Test]
        public void DisposeTest()
        {
            _csvWriter.Dispose();
            Assert.That(_csvWriter.Stream, Is.Null);
        }
    }
}