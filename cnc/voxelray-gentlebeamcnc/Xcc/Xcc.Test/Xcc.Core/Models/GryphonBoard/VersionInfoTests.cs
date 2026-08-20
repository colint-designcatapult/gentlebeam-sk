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

            Assert.That(sut.FirmwareVersion, Is.Null);
            Assert.That(sut.FirmwareChecksum, Is.EqualTo(0));
            Assert.That(sut.Mode, Is.EqualTo(FirmwareMode.Normal));
            Assert.That(sut.HvpsFirmwareVersion, Is.Null);
            Assert.That(sut.HvpsMode, Is.EqualTo(FirmwareMode.Normal));
        }

        [Test]
        public void VersionInfo_GettersSetters()
        {
            var sut = new VersionInfo
            {
                FirmwareVersion = "3.0.0-deadbeef.42",
                FirmwareChecksum = 42,
                Mode = FirmwareMode.Demo,
                HvpsFirmwareVersion = "3.0.0-deadbeef.42",
                HvpsMode = FirmwareMode.Calibration,
            };

            Assert.That(sut.FirmwareVersion, Is.EqualTo("3.0.0-deadbeef.42"));
            Assert.That(sut.FirmwareChecksum, Is.EqualTo(42));
            Assert.That(sut.Mode, Is.EqualTo(FirmwareMode.Demo));
            Assert.That(sut.HvpsFirmwareVersion, Is.EqualTo("3.0.0-deadbeef.42"));
            Assert.That(sut.HvpsMode, Is.EqualTo(FirmwareMode.Calibration));
        }

        [Test]
        public void VersionInfo_ToString()
        {
            var sut = new VersionInfo
            {
                FirmwareVersion = "3.0.0-deadbeef.42",
                FirmwareChecksum = 42,
                Mode = FirmwareMode.Demo,
                HvpsFirmwareVersion = "3.0.0-deadbeef.42",
                HvpsMode = FirmwareMode.Calibration,
            };

            var res = sut.ToString();

            Assert.That(res, Does.Contain("FirmwareVersion: 3.0.0-deadbeef.42"));
            Assert.That(res, Does.Contain("FirmwareChecksum: 42"));
            Assert.That(res, Does.Contain("Mode: Demo"));
            Assert.That(res, Does.Contain("HvpsMode: Calibration"));
            Assert.That(res, Does.Contain(Environment.NewLine));
        }
    }
}