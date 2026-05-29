using Xcc.Infra.Persistence.CSV;
using Xcc.Infra.Persistence.CSV.Types;

namespace Xcc.Test.Xcc.Infra.Persistence
{
    internal class CsvReaderTests
    {
        MemoryStream _memoryStream;
        StreamReader _streamReader;
        CsvReader _csvReader;

        internal class TestObject
        {
            public string StringValue { get; set; } = "Value1";
            public string EmptyStringValue { get; set; } = string.Empty;
            public double DoubleValue { get; set; } = 1.0;
            public CsvBool BoolValue { get; set; } = new();

            public override string ToString()
            {
                return $"{StringValue.ToString()},{EmptyStringValue.ToString()},{DoubleValue.ToString()},{BoolValue.ToString()}";
            }
        }

        [SetUp]
        public void Setup()
        {
            _memoryStream = new();
            _streamReader = new(_memoryStream);
            _csvReader = new CsvReader(_streamReader);
        }

        [TearDown]
        public void Teardown()
        {
            _csvReader.Dispose();
            _streamReader.Dispose();
            _memoryStream.Dispose();
        }

        [Test]
        public void SeekTableNameTest()
        {
            const string tableName = "TestTable";
            using (var streamWriter = new StreamWriter(_memoryStream))
            {
                streamWriter.WriteLine($"{tableName} Table:");
                streamWriter.Flush();

                _memoryStream.Seek(0, SeekOrigin.Begin);

                string readName = null!;
                Assert.DoesNotThrow(() => readName = _csvReader.SeekTableName());
                Assert.That(readName, Is.EqualTo(tableName));
            }
        }

        [Test]
        public void SeekTableName_AfterIrrelevantLines_Test()
        {
            const string tableName = "TestTable";
            using (var streamWriter = new StreamWriter(_memoryStream))
            {
                streamWriter.WriteLine($"some,irrelevant,line");
                streamWriter.WriteLine($"some more data");
                streamWriter.WriteLine($"{tableName} Table:");
                streamWriter.Flush();

                _memoryStream.Seek(0, SeekOrigin.Begin);
                string readName = null!;
                Assert.DoesNotThrow(() => readName = _csvReader.SeekTableName());
                Assert.That(readName, Is.EqualTo(tableName));
            }
        }

        [Test]
        public void SeekTableName_InvalidFormatTest()
        {
            using (var streamWriter = new StreamWriter(_memoryStream))
            {
                streamWriter.WriteLine($"Some invalid,comma separated string");
                streamWriter.WriteLine($"Some Table without proper format");
                streamWriter.Flush();

                _memoryStream.Seek(0, SeekOrigin.Begin);
                string readName = null!;
                Assert.DoesNotThrow(() => readName = _csvReader.SeekTableName());
                Assert.That(readName, Is.Null);
            }
        }

        [Test]
        public void ReadRecord_PositiveTest()
        {
            var obj = new TestObject();
            using (var streamWriter = new StreamWriter(_memoryStream))
            {
                streamWriter.WriteLine(obj.ToString());
                streamWriter.Flush();
            }
        }

        [TestCaseSource(nameof(ReadRecordInvalidTestCases))]
        public void ReadRecord_InvalidFormatTest((string recordLine, Type exceptionType) data)
        {
            var obj = new TestObject();
            var propertyList = typeof(TestObject).GetProperties().Select(prop => prop.Name).ToList();

            using (var streamWriter = new StreamWriter(_memoryStream))
            {
                streamWriter.WriteLine(data.recordLine);
                streamWriter.Flush();
                
                _memoryStream.Seek(0, SeekOrigin.Begin);
                Assert.Throws(data.exceptionType, () => _csvReader.ReadRecord<TestObject>(propertyList));
            }
        }

        [Test]
        public void ReadTableTest()
        {
            IList<TestObject> objects = new List<TestObject> { new TestObject(), new TestObject { DoubleValue = 2.0 } };
            var propertyList = typeof(TestObject).GetProperties().Select(prop => prop.Name).ToList();

            using (var streamWriter = new StreamWriter(_memoryStream))
            {
                streamWriter.WriteLine(string.Join(',', propertyList));
                foreach (var obj in objects) {
                    streamWriter.WriteLine(obj.ToString());
                }
                streamWriter.Flush();

                _memoryStream.Seek(0, SeekOrigin.Begin);
                ICollection<TestObject> objRead = null!;
                Assert.DoesNotThrow(() => objRead = _csvReader.ReadTable<TestObject>());
                Assert.That(objRead, Is.Not.Null);
                Assert.That(objRead, Is.Not.Empty);
                Assert.That(objRead.First().StringValue, Is.EqualTo(objects.First().StringValue));
                Assert.That(objRead.First().DoubleValue, Is.EqualTo(objects.First().DoubleValue));
                Assert.That(objRead.Last().StringValue, Is.EqualTo(objects.Last().StringValue));
                Assert.That(objRead.Last().DoubleValue, Is.EqualTo(objects.Last().DoubleValue));
            }
        }

        static IEnumerable<(string, Type)> ReadRecordInvalidTestCases()
        {
            var obj = new TestObject();
            yield return ($"{obj.StringValue.ToString()},{obj.EmptyStringValue.ToString()},someStringIsteadOfDouble,{obj.BoolValue.ToString()}", typeof(FormatException));
            yield return (obj.ToString() + ",someExtraValue", typeof(CsvWrongFormatException));
            yield return ($"{obj.StringValue.ToString()},{obj.EmptyStringValue.ToString()}", typeof(CsvWrongFormatException));
        }

        [Test]
        public void DisposeTest()
        {
            _csvReader.Dispose();
            Assert.That(_csvReader.Stream, Is.Null);
        }
    }
}
