using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard;

internal class HvpsTelemetryStatusTests
{
    [Test]
    public void NamedProperties_DecodeEveryKnownBit()
    {
        var status = new HvpsTelemetryStatus(0x000003FF, 0x0001FFFF, 0x12345678);

        Assert.Multiple(() =>
        {
            Assert.That(status.TestMode, Is.True);
            Assert.That(status.HighVoltageControlEnabled, Is.True);
            Assert.That(status.GridControlEnabled, Is.True);
            Assert.That(status.Warming, Is.True);
            Assert.That(status.KilovoltageRamping, Is.True);
            Assert.That(status.EmissionOn, Is.True);
            Assert.That(status.ConfigurationUnlocked, Is.True);
            Assert.That(status.PidEnabled, Is.True);
            Assert.That(status.CalibrationGridInterlockEnabled, Is.True);
            Assert.That(status.FastWarmupEnabled, Is.True);
            Assert.That(status.GridClockStatus, Is.True);
            Assert.That(status.FilamentClockFault, Is.True);
            Assert.That(status.GridInterlock, Is.True);
            Assert.That(status.BeamControl, Is.True);
            Assert.That(status.GridStatus, Is.True);
            Assert.That(status.CathodeArc, Is.True);
            Assert.That(status.FanFault, Is.True);
            Assert.That(status.PowerFactorCorrectionOk, Is.True);
            Assert.That(status.HighVoltageInterlock, Is.True);
            Assert.That(status.HighVoltageStatus, Is.True);
            Assert.That(status.Overcurrent24VoltFault, Is.True);
            Assert.That(status.MasterFault, Is.True);
            Assert.That(status.HighVoltageOvercurrentFault, Is.True);
            Assert.That(status.Temperature1Fault, Is.True);
            Assert.That(status.CathodeOvercurrentFault, Is.True);
            Assert.That(status.Temperature3Fault, Is.True);
            Assert.That(status.Temperature2Fault, Is.True);
            Assert.That(status.HasActiveFaultInput, Is.True);
            Assert.That(status.UnknownStatusFlags, Is.Zero);
            Assert.That(status.UnknownIoFlags, Is.Zero);
            Assert.That(status.RawErrorFlags, Is.EqualTo(0x12345678u));
        });
    }

    [Test]
    public void UnknownMasks_PreserveBitsOutsideKnownRanges()
    {
        var status = new HvpsTelemetryStatus(0xFFFFFC00, 0xFFFE0000, null);

        Assert.Multiple(() =>
        {
            Assert.That(status.UnknownStatusFlags, Is.EqualTo(0xFFFFFC00u));
            Assert.That(status.UnknownIoFlags, Is.EqualTo(0xFFFE0000u));
            Assert.That(status.HasActiveFaultInput, Is.False);
            Assert.That(status.RawErrorFlags, Is.Null);
        });
    }
}
