using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard;

internal class SystemInterlocksTests
{
    [Test]
    public void NamedPropertiesAndGetState_PreserveTriStateSemantics()
    {
        var doorMask = 1UL << (int)SystemInterlock.DoorClosed;
        var active = doorMask;
        var available = active | (1UL << (int)SystemInterlock.BaseKeyOn);
        var interlocks = new SystemInterlocks(0x80000001, 1, active, available, doorMask);

        Assert.Multiple(() =>
        {
            Assert.That(interlocks.RawFlags, Is.EqualTo(0x80000001u));
            Assert.That(interlocks.RawRequiredFlags, Is.EqualTo(1u));
            Assert.That(interlocks.DoorClosed, Is.True);
            Assert.That(interlocks.BaseKeyOn, Is.False);
            Assert.That(interlocks.SpareInterlock2, Is.Null);
            Assert.That(interlocks.GetState((SystemInterlock)64), Is.Null);
            Assert.That(interlocks.GetState((SystemInterlock)(-1)), Is.Null);
            Assert.That(interlocks.IsRequired(SystemInterlock.DoorClosed), Is.True);
            Assert.That(interlocks.IsRequired(SystemInterlock.BaseKeyOn), Is.False);
            Assert.That(interlocks.IsRequired((SystemInterlock)64), Is.False);
            Assert.That(interlocks.RequiredInterlocksReady, Is.True);
        });
    }

    [Test]
    public void ValueEquality_IncludesRawAndSemanticMasks()
    {
        var left = new SystemInterlocks(3, 1, 1, 3, 1);
        var same = new SystemInterlocks(3, 1, 1, 3, 1);
        var differentRawRequired = new SystemInterlocks(3, 0, 1, 3, 1);

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(same));
            Assert.That(left == same, Is.True);
            Assert.That(left != differentRawRequired, Is.True);
        });
    }

    [Test]
    public void RequiredInterlocksReady_RequiresEveryRequiredSignalToBeAvailableAndActive()
    {
        var doorMask = 1UL << (int)SystemInterlock.DoorClosed;
        var baseEStopMask = 1UL << (int)SystemInterlock.BaseEStopReleased;

        var inactive = new SystemInterlocks(0, 1, 0, doorMask, doorMask);
        var unavailable = new SystemInterlocks(0, 1, doorMask, doorMask, doorMask | baseEStopMask);

        Assert.Multiple(() =>
        {
            Assert.That(inactive.RequiredInterlocksReady, Is.False);
            Assert.That(unavailable.RequiredInterlocksReady, Is.False);
        });
    }

    [Test]
    public void FeedsMasterFault_IdentifiesHardwareAndGateInputs()
    {
        var expected = new[]
        {
            SystemInterlock.DoorClosed,
            SystemInterlock.SpareInterlock2,
            SystemInterlock.BaseEStopReleased,
            SystemInterlock.RemoteEStopReleased,
            SystemInterlock.Kuka1Ready,
            SystemInterlock.Kuka2Ready,
            SystemInterlock.WaterLevelOk,
            SystemInterlock.IonPumpOk,
            SystemInterlock.Timer1Ready,
            SystemInterlock.Timer2Ready,
            SystemInterlock.HvpsReady,
            SystemInterlock.CoolerReady,
            SystemInterlock.WaterTemperatureOk,
            SystemInterlock.WatchdogReady,
            SystemInterlock.SpareInterlock1,
        };

        var actual = Enum.GetValues<SystemInterlock>()
            .Where(interlock => interlock.FeedsMasterFault());

        Assert.That(actual, Is.EqualTo(expected));
    }
}
