using Heracles.Application.Domain.DataManagement.System.Collimators;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvPresetTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var csvFactor = new CsvPreset();
            Assert.Multiple(() =>
            {
                Assert.That(csvFactor.PresetName, Is.EqualTo(string.Empty));
                Assert.That(csvFactor.IsDefault.Value, Is.EqualTo(false));
                Assert.That(csvFactor.IsActive.Value, Is.EqualTo(false));
            });
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new PresetConfiguration()
            {
                PresetName = "Default",
                IsActive = true,
                IsDefault = true,
            };
            var csvFactor = new CsvPreset(config);
            Assert.Multiple(() =>
            {
                Assert.That(csvFactor.PresetName, Is.EqualTo(config.PresetName));
                Assert.That(csvFactor.IsDefault.Value, Is.EqualTo(config.IsDefault));
                Assert.That(csvFactor.IsActive.Value, Is.EqualTo(config.IsActive));
            });
        }

        [Test]
        public void NullReference_CustomConstructorTest()
        {
            Assert.Throws<NullReferenceException>(() => new CsvPreset(null));
        }
    }
}
