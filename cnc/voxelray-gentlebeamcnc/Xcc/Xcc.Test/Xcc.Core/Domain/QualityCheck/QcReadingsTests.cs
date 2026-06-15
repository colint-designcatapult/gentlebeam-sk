using Xcc.Core.Domain.QualityCheck;

namespace Xcc.Test.Xcc.Core.Domain.QualityCheck
{
    public class QcReadingsTests
    {
        [Test]
        public void QcReadings_Ctor_Throws()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new QcReadings(null));
            Assert.That(ex!.Message, Does.Contain("no data"));
        }
        
        [Test]
        public void QcReadings_Ctor()
        {
            var input = new float[] { 1.1f, 2.2f, 3.3f };

            var sut = new QcReadings(input);

            Assert.That(sut.Data, Is.EqualTo(input));
        }
    }
}