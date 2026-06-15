using Xcc.Infra.Persistence.CSV.Types;

namespace Xcc.Test.Xcc.Infra.Persistence
{
    internal class CsvMappedTypeTests
    {
        private static IDictionary<int, string> IntValueToString = new Dictionary<int, string> { { 0, "0" }, { 1, "1" } };
        private static CsvValueMap<int> IntToStringValueMap = new (IntValueToString);

        [Test]
        public void ConstructorTest()
        {
            CsvMappedType<int> value = null!;
            Assert.DoesNotThrow(() => value = new CsvMappedType<int>(0, IntToStringValueMap));
            Assert.That(value.Value, Is.EqualTo(0));
        }

        [Test]
        public void ToStringTest()
        {
            var value = new CsvMappedType<int>(0, IntToStringValueMap);
            Assert.That(value.ToString(), Is.EqualTo(IntToStringValueMap.ToCsvString(value.Value)));
        }

        [Test]
        public void TryParse_PositiveTest()
        {
            var value = new CsvMappedType<int>(1, IntToStringValueMap);
            Assert.DoesNotThrow(() => value.TryParse("0"));
            Assert.That(value.Value, Is.EqualTo(0));
        }

        [Test]
        public void TryParse_NegativeTest()
        {
            var value = new CsvMappedType<int>(0, IntToStringValueMap);
            Assert.Throws<KeyNotFoundException>(() => value.TryParse("2"));
        }
    }

    internal class CsvBoolTests
    {
        [Test]
        public void ConstructorTest([Values(true, false)] bool initValue)
        {
            CsvBool value = null!;
            Assert.DoesNotThrow(() => value = new(initValue));
            Assert.That(value.Value, Is.EqualTo(initValue));
        }

        [Test]
        public void ConversionTest([Values(true, false)] bool initialValue)
        {
            CsvBool value = new CsvBool(initialValue);
            bool result = value.TryParse(value.ToString());
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(value.Value, Is.EqualTo(initialValue));
            });
        }
    }
}
