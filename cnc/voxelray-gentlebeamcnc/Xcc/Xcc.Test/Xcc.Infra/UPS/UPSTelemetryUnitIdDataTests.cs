using Xcc.Infra.UPS;

namespace Xcc.Test.Xcc.Infra.UPS
{
    internal class UPSTelemetryUnitIdDataTests
    {
        [Test]
        public void Defaults()
        {
            var sut = new UpsTelemetry.UnitIdData();
            
            Assert.That(sut.Model, Is.Null);
            Assert.That(sut.Serial, Is.Null);
        }
        
        [Test]
        public void GettersSetters(
            [Values("model 1", "testModel")] string model,
            [Values("345", "testSerial")] string serial)
        {
            var sut = new UpsTelemetry.UnitIdData
            {
                Model = model,
                Serial = serial
            };
            
            Assert.That(sut.Model, Is.EqualTo(model));
            Assert.That(sut.Serial, Is.EqualTo(serial));
        }
    }
}
