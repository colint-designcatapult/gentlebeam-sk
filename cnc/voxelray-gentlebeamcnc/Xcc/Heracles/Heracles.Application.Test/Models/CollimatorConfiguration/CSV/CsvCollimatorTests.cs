using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Application.Models.RDBMS;
using Heracles.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvEnergyTypeTests
    {
        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new CsvEnergyType());
        }

        [Test]
        public void ConversionTest([Values] Energy energyValue)
        {
            var csvEnergy = new CsvEnergyType(energyValue);

            Assert.DoesNotThrow(() => csvEnergy.TryParse(csvEnergy.ToString()));
            Assert.That(csvEnergy.Value, Is.EqualTo(energyValue));
        }

        [TestCase(Energy.Energy_50, ExpectedResult = "50")]
        [TestCase(Energy.Energy_70, ExpectedResult = "70")]
        [TestCase(Energy.Energy_100, ExpectedResult = "100")]
        public string ToStringFormatTest(Energy value)
        {
            var csvEnergy = new CsvEnergyType(value);
            return csvEnergy.ToString();
        }
    }


    internal class CsvCollimatorTypeTests
    {
        [Test]
        public void ConstructorTest()
        {
            Assert.DoesNotThrow(() => new CsvCollimatorType());
        }

        [TestCase(TargetType.TargetType_61_Fields)]
        [TestCase(TargetType.TargetType_50mm_SSD_13_Fields)]
        [TestCase(TargetType.TargetType_30mm_SSD_7_Fields)]
        public void ConversionTest(TargetType typeValue)
        {
            var csvCollimatorType = new CsvCollimatorType(typeValue);

            Assert.DoesNotThrow(() => csvCollimatorType.TryParse(csvCollimatorType.ToString()));
            Assert.That(csvCollimatorType.Value, Is.EqualTo(typeValue));
        }

        [TestCase(TargetType.TargetType_61_Fields, ExpectedResult = "61_cell")]
        [TestCase(TargetType.TargetType_50mm_SSD_13_Fields, ExpectedResult = "13_cell_IMVB")]
        [TestCase(TargetType.TargetType_30mm_SSD_7_Fields, ExpectedResult = "7_cell_IMVB")]
        public string ToStringFormatTest(TargetType value)
        {
            var csvCollimatorType = new CsvCollimatorType(value);
            return csvCollimatorType.ToString();
        }
    }

    internal class CsvCollimatorTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var collimator = new CsvCollimator();
            Assert.Multiple(() =>
            {
                Assert.That(collimator.Serial, Is.EqualTo(""));
                Assert.That(collimator.Type.Value, Is.EqualTo(TargetType.TargetType_None));
                Assert.That(collimator.DoseRate, Is.EqualTo(0));
                Assert.That(collimator.Energy.Value, Is.EqualTo((Energy)0));
                Assert.That(collimator.IsActive.Value, Is.EqualTo(false));
            });
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new Collimator()
            {
                Serial = "1",
                Configuration = new Domain.DataManagement.System.Collimators.CollimatorConfiguration
                {
                    Type = TargetType.TargetType_61_Fields,
                    ReferencedDoseRate = 2,
                    Energy = Energy.Energy_50,
                },
                IsActive = true,
            };
            var collimator = new CsvCollimator(config);
            Assert.Multiple(() =>
            {
                Assert.That(collimator.Serial, Is.EqualTo(config.Serial));
                Assert.That(collimator.Type.Value, Is.EqualTo(config.Configuration.Type));
                Assert.That(collimator.DoseRate, Is.EqualTo(config.Configuration.ReferencedDoseRate));
                Assert.That(collimator.Energy.Value, Is.EqualTo(config.Configuration.Energy));
                Assert.That(collimator.IsActive.Value, Is.EqualTo(config.IsActive));
            });
        }

        [Test]
        public void NullReference_CustomConstructorTest()
        {
            Assert.Throws<NullReferenceException>(() => new CsvCollimator(null));
        }
    }
}
