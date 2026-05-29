using Heracles.Application.Models.CollimatorConfiguration;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvCoilConfigurationTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var coil = new CsvCoilConfiguration();
            Assert.Multiple(() =>
            {
                Assert.That(coil.FieldName.Value, Is.EqualTo((TreatmentFieldName)0));
                Assert.That(coil.XDeflectionCurrent, Is.EqualTo(0));
                Assert.That(coil.YDeflectionCurrent, Is.EqualTo(0));
                Assert.That(coil.FocusCurrent, Is.EqualTo(0));
            });
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new CoilConfigurationEntry()
            {
                FieldName = TreatmentFieldName.PlusC,
                FocusCurrent = 1,
                XDeflectionCurrent = 2,
                YDeflectionCurrent = 3
            };
            var coil = new CsvCoilConfiguration(config);
            Assert.Multiple(() =>
            {
                Assert.That(coil.FieldName.Value, Is.EqualTo(config.FieldName));
                Assert.That(coil.XDeflectionCurrent, Is.EqualTo(config.XDeflectionCurrent));
                Assert.That(coil.YDeflectionCurrent, Is.EqualTo(config.YDeflectionCurrent));
                Assert.That(coil.FocusCurrent, Is.EqualTo(config.FocusCurrent));
            });
        }

        [Test]
        public void NullReference_CustomConstructorTest()
        {
            Assert.Throws<NullReferenceException>(() => new CsvCoilConfiguration(null));
        }
    }
}
