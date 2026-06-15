using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvMagnetometerTypeTests
    {
        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new CsvMagnetometerType(MagnetometerType.Front));
        }

        [Test]
        public void ConversionTest([Values] MagnetometerType typeValue)
        {
            var csvMagnetometerType = new CsvMagnetometerType(typeValue);

            Assert.DoesNotThrow(() => csvMagnetometerType.TryParse(csvMagnetometerType.ToString()));
            Assert.That(csvMagnetometerType.Value, Is.EqualTo(typeValue));
        }

        [TestCase(MagnetometerType.Back, ExpectedResult = "MAGNETOMETERTYPE_BACK")]
        [TestCase(MagnetometerType.Front, ExpectedResult = "MAGNETOMETERTYPE_FRONT")]
        public string ToStringFormatTest(MagnetometerType value)
        {
            var csvMagnetometerType = new CsvMagnetometerType(value);
            return csvMagnetometerType.ToString();
        }
    }
}
