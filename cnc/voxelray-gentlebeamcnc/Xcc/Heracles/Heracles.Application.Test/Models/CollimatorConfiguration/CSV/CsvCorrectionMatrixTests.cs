using Heracles.Application.Models.CollimatorConfiguration.CSV.Types;
using Xcc.Application.Domain.System;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Models.CollimatorConfiguration.CSV
{
    internal class CsvCorrectionMatrixTests
    {
        [Test]
        public void DefaultConstructorTest()
        {
            var matrix = new CsvCorrectionMatrix();
            Assert.Multiple(() =>
            {
                Assert.That(matrix.MagnetometerType.Value, Is.EqualTo(MagnetometerType.Front));
                Assert.That(matrix.CM11, Is.EqualTo(0));
                Assert.That(matrix.CM12, Is.EqualTo(0));
                Assert.That(matrix.CM13, Is.EqualTo(0));
                Assert.That(matrix.CM21, Is.EqualTo(0));
                Assert.That(matrix.CM22, Is.EqualTo(0));
                Assert.That(matrix.CM23, Is.EqualTo(0));
            });
        }

        [Test]
        public void CustomConstructorTest()
        {
            var config = new CorrectionMatrix()
            {
                MagnetometerType = MagnetometerType.Back,
                Cm11 = 11, Cm12 = 12, Cm13 = 13, Cm21 = 21, Cm22 = 22, Cm23 = 23,
            };
            var matrix = new CsvCorrectionMatrix(config);
            Assert.Multiple(() =>
            {
                Assert.That(matrix.MagnetometerType.Value, Is.EqualTo(config.MagnetometerType));
                Assert.That(matrix.CM11, Is.EqualTo(config.Cm11));
                Assert.That(matrix.CM12, Is.EqualTo(config.Cm12));
                Assert.That(matrix.CM13, Is.EqualTo(config.Cm13));
                Assert.That(matrix.CM21, Is.EqualTo(config.Cm21));
                Assert.That(matrix.CM22, Is.EqualTo(config.Cm22));
                Assert.That(matrix.CM23, Is.EqualTo(config.Cm23));
            });
        }

        [Test]
        public void NullReference_CustomConstructorTest()
        {
            Assert.Throws<NullReferenceException>(() => new CsvCorrectionMatrix(null));
        }
    }
}
