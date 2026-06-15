using Heracles.Application.Domain.DataManagement.System.Physics;
using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Heracles.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvOutputFactorTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var csvFactor = new CsvOutputFactor();
            Assert.Multiple(() =>
            {
                Assert.That(csvFactor.Field.Value, Is.EqualTo((TreatmentFieldName)0));
                Assert.That(csvFactor.Factor, Is.EqualTo(0));
            });
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new OutputFactor()
            {
                FieldName = TreatmentFieldName.PlusC,
                Factor = 1,
            };
            var csvFactor = new CsvOutputFactor(config);
            Assert.Multiple(() =>
            {
                Assert.That(csvFactor.Field.Value, Is.EqualTo(config.FieldName));
                Assert.That(csvFactor.Factor, Is.EqualTo(config.Factor));
            });
        }

        [Test]
        public void NullReference_CustomConstructorTest()
        {
            Assert.Throws<NullReferenceException>(() => new CsvOutputFactor(null));
        }
    }
}
