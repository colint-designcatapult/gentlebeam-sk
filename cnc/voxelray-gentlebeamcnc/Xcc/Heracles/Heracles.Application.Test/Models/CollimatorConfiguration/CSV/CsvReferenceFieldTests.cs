using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Xcc.Application.Domain.System;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvReferenceFieldTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var csvReferenceField = new CsvReferenceField();
            Assert.Multiple(() =>
            {
                Assert.That(csvReferenceField.MagnetometerType.Value, Is.EqualTo(MagnetometerType.Front));
                Assert.That(csvReferenceField.RF11, Is.EqualTo(0));
                Assert.That(csvReferenceField.RF21, Is.EqualTo(0));
                Assert.That(csvReferenceField.RF31, Is.EqualTo(0));
            });
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new ReferenceField()
            {
                MagnetometerType = MagnetometerType.Back,
                Rf11 = 11, Rf21 = 21, Rf31 = 31,
            };
            var csvReferenceField = new CsvReferenceField(config);
            Assert.Multiple(() =>
            {
                Assert.That(csvReferenceField.MagnetometerType.Value, Is.EqualTo(config.MagnetometerType));
                Assert.That(csvReferenceField.RF11, Is.EqualTo(config.Rf11));
                Assert.That(csvReferenceField.RF21, Is.EqualTo(config.Rf21));
                Assert.That(csvReferenceField.RF31, Is.EqualTo(config.Rf31));
            });
        }

        [Test]
        public void NullReference_CustomConstructorTest()
        {
            Assert.Throws<NullReferenceException>(() => new CsvReferenceField(null));
        }
    }
}
