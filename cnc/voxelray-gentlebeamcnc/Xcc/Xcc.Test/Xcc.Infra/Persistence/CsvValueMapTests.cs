using Xcc.Infra.Persistence.CSV.Types;

namespace Xcc.Test.Xcc.Infra.Persistence
{
    internal class CsvValueMapTests
    {
        private static IDictionary<int, string> ValueToString = new Dictionary<int, string> { { 1, "1" }, { 2, "2" } };

        [Test]
        public void PositiveConstructionTest()
        {
            Assert.DoesNotThrow(() => new CsvValueMap<int>(ValueToString));
        }

        [Test]
        public void ThrowsOnDuplicate_ConstructionTest()
        {
            var valueToStringWithDuplicates = new Dictionary<int, string> { { 1, "1" }, { 2, "2" }, { 3, "1" }, };
            Assert.Throws<ArgumentException>(() => new CsvValueMap<int>(valueToStringWithDuplicates));
        }

        [Test]
        public void ConvertToCsvTest([ValueSource(nameof(ValueToString))] KeyValuePair<int, string> kv)
        {
            var map = new CsvValueMap<int>(ValueToString);
            Assert.That(map.ToCsvString(kv.Key), Is.EqualTo(kv.Value));
        }

        [Test]
        public void InvalidValue_ConvertToCsvTest()
        {
            var map = new CsvValueMap<int>(ValueToString);
            Assert.Throws<KeyNotFoundException>(() => map.ToCsvString(0));
        }

        [Test]
        public void ConvertToValueTest([ValueSource(nameof(ValueToString))] KeyValuePair<int, string> kv)
        {
            var map = new CsvValueMap<int>(ValueToString);
            Assert.That(map.ToValue(kv.Value), Is.EqualTo(kv.Key));
        }

        [Test]
        public void InvalidValue_ConvertToValueTest()
        {
            var map = new CsvValueMap<int>(ValueToString);
            Assert.Throws<KeyNotFoundException>(() => map.ToValue("0"));
        }
    }
}
