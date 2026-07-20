using System.Globalization;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard;

public class FaultEntryTests
{
    [Test]
    public void FaultEntry_ExposesImmutableProtocolValues()
    {
        var entry = new FaultEntry(
            SystemFault.VoltageFault,
            0x12345678,
            GcbStateNew.Emission,
            9876,
            "kV feedback %f is outside target %f.",
            "kV feedback 42.5 is outside target 45.");

        Assert.Multiple(() =>
        {
            Assert.That(entry.FaultType, Is.EqualTo(SystemFault.VoltageFault));
            Assert.That(entry.FormatHash, Is.EqualTo(0x12345678));
            Assert.That(entry.CapturedState, Is.EqualTo(GcbStateNew.Emission));
            Assert.That(entry.CapturedRuntime, Is.EqualTo(9876));
            Assert.That(entry.Format, Is.EqualTo("kV feedback %f is outside target %f."));
            Assert.That(entry.Message, Is.EqualTo("kV feedback 42.5 is outside target 45."));
        });
    }

    [Test]
    public void FaultEntry_ToString_UsesDisplayContractExactly()
    {
        var entry = new FaultEntry(
            SystemFault.OtherFault,
            7,
            GcbStateNew.WarmupFault,
            123,
            "Failure.",
            "Failure.");

        Assert.That(
            entry.ToString(),
            Is.EqualTo("OtherFault: Failure. (state WarmupFault, runtime 123)"));
    }

    [Test]
    public void Format_SupportsEverySpecifierAndEscapedPercent()
    {
        uint[] arguments =
        [
            unchecked((uint)-42),
            uint.MaxValue,
            0xdeadbeef,
            0xabcdef12,
            BitConverter.SingleToUInt32Bits(1.25f),
        ];

        string message = FaultMessageFormatter.Format(
            "signed %d; unsigned %u; lower %x; upper %X; float %f; percent %%.",
            arguments);

        Assert.That(
            message,
            Is.EqualTo("signed -42; unsigned 4294967295; lower deadbeef; upper ABCDEF12; float 1.25; percent %."));
    }

    [Test]
    [SetCulture("de-DE")]
    public void Format_UsesInvariantG9FloatFormatting()
    {
        uint[] arguments = [BitConverter.SingleToUInt32Bits(1f / 3f)];

        string message = FaultMessageFormatter.Format("%f", arguments);

        Assert.That(message, Is.EqualTo("0.333333343"));
        Assert.That(CultureInfo.CurrentCulture.Name, Is.EqualTo("de-DE"));
    }

    [Test]
    public void Format_RequiresExactArgumentConsumption()
    {
        Assert.Multiple(() =>
        {
            Assert.That(
                () => FaultMessageFormatter.Format("%d %u", new uint[] { 1 }),
                Throws.TypeOf<FormatException>());
            Assert.That(
                () => FaultMessageFormatter.Format("%d", new uint[] { 1, 2 }),
                Throws.TypeOf<FormatException>());
            Assert.That(
                () => FaultMessageFormatter.Format("%%", new uint[] { 1 }),
                Throws.TypeOf<FormatException>());
        });
    }

    [TestCase("%")]
    [TestCase("%s")]
    [TestCase("%02d")]
    [TestCase("%.2f")]
    [TestCase("%ld")]
    [TestCase("%-d")]
    public void Format_RejectsMalformedOrUnsupportedGrammar(string format)
    {
        Assert.That(
            () => FaultMessageFormatter.Format(format, new uint[] { 1 }),
            Throws.TypeOf<FormatException>());
    }
}