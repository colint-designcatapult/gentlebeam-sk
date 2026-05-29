using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class VersionInfoTests
    {
        [Test]
        public void VersionInfo_Defaults()
        {
            var sut = new VersionInfo();

            Assert.That(sut.Major, Is.EqualTo(0));
            Assert.That(sut.Minor, Is.EqualTo(0));
            Assert.That(sut.Level, Is.EqualTo(0));
            Assert.That(sut.FirmwareChecksum, Is.EqualTo(0));
            Assert.That(sut.Mode, Is.EqualTo(FirmwareMode.Normal));
        }
        
        [Test]
        public void VersionInfo_GettersSetters()
        {
            var major = 10;
            var minor = 13;
            var level = 16;
            var firmwareChecksum = 42;
            var mode = FirmwareMode.Demo;
                
            var sut = new VersionInfo
            {
                Major = major,
                Minor = minor,
                Level = level,
                FirmwareChecksum = firmwareChecksum,
                Mode = mode,
            };

            Assert.That(sut.Major, Is.EqualTo(major));
            Assert.That(sut.Minor, Is.EqualTo(minor));
            Assert.That(sut.Level, Is.EqualTo(level));
            Assert.That(sut.FirmwareChecksum, Is.EqualTo(firmwareChecksum));
            Assert.That(sut.Mode, Is.EqualTo(mode));
        }
        
        [Test]
        public void VersionInfo_ToString()
        {   
            var sut = new VersionInfo
            {
                Major = 10,
                Minor = 13,
                Level = 16,
                FirmwareChecksum = 42,
                Mode = FirmwareMode.Demo,
            };

            var res = sut.ToString();
            
            Assert.That(res, Does.Contain("Version: 10.13"));
            Assert.That(res, Does.Contain("Level: 16"));
            Assert.That(res, Does.Contain("FirmwareChecksum: 42"));
            Assert.That(res, Does.Contain("Mode: Demo"));
            Assert.That(res, Does.Contain(Environment.NewLine));
        }
    }
}