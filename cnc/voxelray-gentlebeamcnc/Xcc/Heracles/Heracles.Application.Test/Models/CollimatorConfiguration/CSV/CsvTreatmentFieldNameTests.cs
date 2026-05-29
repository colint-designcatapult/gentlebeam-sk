using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvTreatmentFieldNameTests
    {
        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new CsvTreatmentFieldName());
        }

        [Test]
        public void ConversionTest([Values] TreatmentFieldName fieldName)
        {
            var csvFieldName = new CsvTreatmentFieldName(fieldName);

            Assert.DoesNotThrow(() => csvFieldName.TryParse(csvFieldName.ToString()));
            Assert.That(csvFieldName.Value, Is.EqualTo(fieldName));
        }

        [TestCase(TreatmentFieldName.Minus1L1, ExpectedResult="FIELD_NAME_MINUS_1L1")]
        [TestCase(TreatmentFieldName.Plus0R4, ExpectedResult ="FIELD_NAME_PLUS_0R4")]
        public string ToStringFormatTest(TreatmentFieldName fieldName)
        {
            var csvFieldName = new CsvTreatmentFieldName(fieldName);
            return csvFieldName.ToString();
        }
    }
}
