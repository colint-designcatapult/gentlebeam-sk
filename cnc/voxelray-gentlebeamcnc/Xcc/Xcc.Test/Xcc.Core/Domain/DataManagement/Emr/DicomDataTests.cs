using Xcc.Core.Domain.DataManagement.Emr;

namespace Xcc.Test.Xcc.Core.Domain.DataManagement.Emr
{
    public class DicomDataTests
    {
        [Test]
        public void DicomData_Defaults()
        {
            var sut = new DicomData { Filename = null };

            Assert.That(sut.Filename, Is.Null);
            Assert.That(sut.FileStream, Is.Null);
        }
        
        [Test]
        public void DicomData_SettersGetters(
            [Values("file", "file with spaces")] string filename)
        {
            var memoryStream = new MemoryStream();
            var sut = new DicomData { Filename = filename, FileStream = memoryStream};

            Assert.That(sut.Filename, Is.EqualTo(filename));
            Assert.That(sut.FileStream, Is.SameAs(memoryStream));
        }
    }
}