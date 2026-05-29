using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Xcc.Application.Domain.System;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvHeaterCurrentTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var heaterCurrent = new CsvHeaterCurrent();
            Assert.That(heaterCurrent.HeaterCurrent, Is.EqualTo(0));
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new HeaterCurrentConfig()
            {
                HeaterCurrent = 1.0,
            };
            var csvHeaterCurrent = new CsvHeaterCurrent(config.HeaterCurrent.Value);
            Assert.That(csvHeaterCurrent.HeaterCurrent, Is.EqualTo(config.HeaterCurrent.Value));
        }
    }
}
