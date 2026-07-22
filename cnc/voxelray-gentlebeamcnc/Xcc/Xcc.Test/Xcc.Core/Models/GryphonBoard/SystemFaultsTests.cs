using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard;

internal class SystemFaultsTests
{
    [Test]
    public void GetState_DistinguishesActiveInactiveUnavailableAndUnknownValues()
    {
        var faults = new SystemFaults(
            rawFlags: 0x80000008,
            rawCommunicationFlags: 0x12345678,
            activeMask: 1UL << (int)SystemFault.VoltageFault,
            availableMask: (1UL << (int)SystemFault.VoltageFault) | (1UL << (int)SystemFault.CurrentFault));

        Assert.Multiple(() =>
        {
            Assert.That(faults.RawFlags, Is.EqualTo(0x80000008u));
            Assert.That(faults.RawCommunicationFlags, Is.EqualTo(0x12345678u));
            Assert.That(faults.GetState(SystemFault.VoltageFault), Is.True);
            Assert.That(faults.GetState(SystemFault.CurrentFault), Is.False);
            Assert.That(faults.GetState(SystemFault.InterlockFault), Is.Null);
            Assert.That(faults.GetState((SystemFault)64), Is.Null);
            Assert.That(faults.GetState((SystemFault)(-1)), Is.Null);
            Assert.That(faults.AnyActive, Is.True);
            Assert.That(faults.ToString(), Is.EqualTo("kV Fault"));
        });
    }

    [Test]
    public void ValueEquality_IncludesRawAndSemanticMasks()
    {
        var left = new SystemFaults(3, null, 1, 3);
        var same = new SystemFaults(3, null, 1, 3);
        var differentAvailability = new SystemFaults(3, null, 1, 1);

        Assert.Multiple(() =>
        {
            Assert.That(left, Is.EqualTo(same));
            Assert.That(left == same, Is.True);
            Assert.That(left != differentAvailability, Is.True);
            Assert.That(new SystemFaults(0, null, 0, ulong.MaxValue).ToString(), Is.EqualTo("No active faults"));
        });
    }
}
