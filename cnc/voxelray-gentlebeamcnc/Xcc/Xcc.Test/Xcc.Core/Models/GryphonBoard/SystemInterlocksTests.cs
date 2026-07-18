using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard;

internal class SystemInterlocksTests
{
    [Test]
    public void NamedPropertiesAndGetState_PreserveTriStateSemantics()
    {
        var active = (1UL << (int)SystemInterlock.DoorClosed)
            | (1UL << (int)SystemInterlock.TvmNotchEngaged);
        var available = active | (1UL << (int)SystemInterlock.CollimatorOn);
        var interlocks = new SystemInterlocks(0x80000001, 5, active, available);

        Assert.Multiple(() =>
        {
            Assert.That(interlocks.RawFlags, Is.EqualTo(0x80000001u));
            Assert.That(interlocks.RawTvmFlags, Is.EqualTo(5u));
            Assert.That(interlocks.DoorClosed, Is.True);
            Assert.That(interlocks.CollimatorOn, Is.False);
            Assert.That(interlocks.TvmNotchEngaged, Is.True);
            Assert.That(interlocks.DriveSystemReady, Is.Null);
            Assert.That(interlocks.GetState((SystemInterlock)64), Is.Null);
            Assert.That(interlocks.GetState((SystemInterlock)(-1)), Is.Null);
        });
    }

    [Test]
    public void ValueEquality_IncludesRawAndSemanticMasks()
    {
        var left = new SystemInterlocks(3, null, 1, 3);
        var same = new SystemInterlocks(3, null, 1, 3);
        var differentRawTvm = new SystemInterlocks(3, 0, 1, 3);

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(same));
            Assert.That(left == same, Is.True);
            Assert.That(left != differentRawTvm, Is.True);
        });
    }
}
