using System.Buffers.Binary;
using System.Text;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Test.Xcc.Infra.GryphonBoard.CommandAPI;

public class FaultEntryParserTests
{
    private const int ResponseWords = 45;
    private const int FormatBytes = 128;

    [Test]
    public void Parse_DecodesCompleteEntry()
    {
        var packet = BuildEntryPacket(
            format: "ADC %u failed with code 0x%X at %f V.",
            arguments:
            [
                32,
                0xdeadbeef,
                BitConverter.SingleToUInt32Bits(3.25f),
            ],
            faultType: (uint)SystemFault.AdcBusCommFault,
            clearEpoch: 17,
            entryIndex: 2,
            activeCount: 3,
            state: GcbStateNew.Emission,
            runtime: 654321);

        var update = FaultEntryParser.Parse(packet);
        int payloadByteLength = packet.Payload.Length;

        Assert.Multiple(() =>
        {
            Assert.That(update.ClearEpoch, Is.EqualTo(17));
            Assert.That(update.EntryIndex, Is.EqualTo(2));
            Assert.That(update.ActiveCount, Is.EqualTo(3));
            Assert.That(update.Entry, Is.Not.Null);
            Assert.That(update.Entry!.FaultType, Is.EqualTo(SystemFault.AdcBusCommFault));
            Assert.That(update.Entry.CapturedState, Is.EqualTo(GcbStateNew.Emission));
            Assert.That(update.Entry.CapturedRuntime, Is.EqualTo(654321));
            Assert.That(update.Entry.Format, Is.EqualTo("ADC %u failed with code 0x%X at %f V."));
            Assert.That(update.Entry.Message, Is.EqualTo("ADC 32 failed with code 0xDEADBEEF at 3.25 V."));
            Assert.That(update.Entry.FormatHash, Is.EqualTo(CrcUtils.ComputeChecksum(Encoding.ASCII.GetBytes(update.Entry.Format))));
            Assert.That(payloadByteLength, Is.EqualTo(ResponseWords * sizeof(uint)));
        });
    }

    [Test]
    public void Parse_Accepts127VisibleFormatBytes()
    {
        string format = new('A', FormatBytes - 1);

        var update = FaultEntryParser.Parse(BuildEntryPacket(format));

        Assert.That(update.Entry!.Format, Has.Length.EqualTo(127));
        Assert.That(update.Entry.Message, Is.EqualTo(format));
    }

    [TestCase("plumless")]
    [TestCase("buckeroo")]
    public void Parse_AcceptsDistinctFormatsWithSameCrc32(string format)
    {
        const uint collisionHash = 0x4ddb0c25;
        var packet = BuildEntryPacket(format, transmittedHash: collisionHash);

        var update = FaultEntryParser.Parse(packet);

        Assert.That(CrcUtils.ComputeChecksum(Encoding.ASCII.GetBytes(format)), Is.EqualTo(collisionHash));
        Assert.That(update.Entry!.Format, Is.EqualTo(format));
    }

    [Test]
    public void Parse_DecodesOtherFaultCategory()
    {
        var packet = BuildEntryPacket("Uncategorized failure.", faultType: (uint)SystemFault.OtherFault);

        var update = FaultEntryParser.Parse(packet);

        Assert.That(update.Entry!.FaultType, Is.EqualTo(SystemFault.OtherFault));
    }

    [Test]
    public void Parse_DecodesClearResponse()
    {
        var packet = BuildEmptyPacket(clearEpoch: 42, entryIndex: 0, activeCount: 0);

        var update = FaultEntryParser.Parse(packet);

        Assert.Multiple(() =>
        {
            Assert.That(update.ClearEpoch, Is.EqualTo(42));
            Assert.That(update.EntryIndex, Is.Zero);
            Assert.That(update.ActiveCount, Is.Zero);
            Assert.That(update.Entry, Is.Null);
        });
    }

    [Test]
    public void Parse_DecodesOutOfRangeIndexedResponse()
    {
        var packet = BuildEmptyPacket(clearEpoch: 7, entryIndex: 9, activeCount: 2);

        var update = FaultEntryParser.Parse(packet);

        Assert.Multiple(() =>
        {
            Assert.That(update.ClearEpoch, Is.EqualTo(7));
            Assert.That(update.EntryIndex, Is.EqualTo(9));
            Assert.That(update.ActiveCount, Is.EqualTo(2));
            Assert.That(update.Entry, Is.Null);
        });
    }

    [TestCase(44)]
    [TestCase(46)]
    public void Parse_RejectsPayloadsThatAreNotExactly45Words(int payloadWords)
    {
        var packet = new UdpPacket((uint)GCBPacketType.FaultInfoResponse, 1, (uint)payloadWords).UpdateCRC();

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<ArgumentException>());
    }

    [Test]
    public void Parse_RejectsWrongPacketType()
    {
        var packet = BuildEntryPacket("Failure.", packetType: GCBPacketType.TelemetryResponse);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<ArgumentException>());
    }

    [TestCase(25u, 1u, 0u, 0u)]
    [TestCase(1u, 5u, 0u, 0u)]
    [TestCase(1u, 1u, 0u, 6u)]
    [TestCase(1u, 1u, 1u, 0u)]
    public void Parse_RejectsInvalidTypeCountIndexOrArgumentCount(
        uint faultType,
        uint activeCount,
        uint entryIndex,
        uint argumentCount)
    {
        var packet = BuildEntryPacket(
            "Failure.",
            faultType: faultType,
            activeCount: activeCount,
            entryIndex: entryIndex,
            argumentCount: argumentCount);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_RejectsMissingFormatTerminator()
    {
        byte[] formatField = Enumerable.Repeat((byte)'A', FormatBytes).ToArray();
        var packet = BuildPacket(
            faultType: 1,
            formatHash: CrcUtils.ComputeChecksum(formatField),
            clearEpoch: 1,
            entryIndex: 0,
            activeCount: 1,
            state: GcbStateNew.Fault,
            runtime: 1,
            argumentCount: 0,
            formatField: formatField,
            arguments: []);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_RejectsNonZeroFormatPadding()
    {
        byte[] formatField = EncodeFormat(Encoding.ASCII.GetBytes("Failure."));
        formatField["Failure.".Length + 1] = (byte)'X';
        var packet = BuildPacket(
            faultType: 1,
            formatHash: CrcUtils.ComputeChecksum(Encoding.ASCII.GetBytes("Failure.")),
            clearEpoch: 1,
            entryIndex: 0,
            activeCount: 1,
            state: GcbStateNew.Fault,
            runtime: 1,
            argumentCount: 0,
            formatField: formatField,
            arguments: []);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [TestCase(0x1f)]
    [TestCase(0x7f)]
    [TestCase(0x80)]
    public void Parse_RejectsNonPrintableOrNonAsciiFormatBytes(byte invalidByte)
    {
        byte[] formatField = new byte[FormatBytes];
        formatField[0] = invalidByte;
        var packet = BuildPacket(
            faultType: 1,
            formatHash: CrcUtils.ComputeChecksum(formatField.AsSpan(0, 1)),
            clearEpoch: 1,
            entryIndex: 0,
            activeCount: 1,
            state: GcbStateNew.Fault,
            runtime: 1,
            argumentCount: 0,
            formatField: formatField,
            arguments: []);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_RejectsFormatHashMismatch()
    {
        var packet = BuildEntryPacket("Failure.", transmittedHash: 0x12345678);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_RejectsMalformedFormatGrammar()
    {
        var packet = BuildEntryPacket("Unsupported %s.", arguments: [1]);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [TestCase("Missing %u.", 0u)]
    [TestCase("Unexpected argument.", 1u)]
    public void Parse_RejectsConsumingSpecifierAndArgumentCountMismatch(string format, uint argumentCount)
    {
        uint[] arguments = argumentCount == 0 ? [] : [1];
        var packet = BuildEntryPacket(format, arguments: arguments, argumentCount: argumentCount);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_RejectsEmptyEntryDataForAnInRangeIndex()
    {
        var packet = BuildEmptyPacket(clearEpoch: 1, entryIndex: 1, activeCount: 2);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    [Test]
    public void Parse_RejectsEntryFieldsInAnEmptyResponse()
    {
        var packet = BuildPacket(
            faultType: 0,
            formatHash: 0,
            clearEpoch: 1,
            entryIndex: 8,
            activeCount: 2,
            state: GcbStateNew.Startup,
            runtime: 1,
            argumentCount: 0,
            formatField: new byte[FormatBytes],
            arguments: []);

        Assert.That(() => FaultEntryParser.Parse(packet), Throws.TypeOf<FormatException>());
    }

    private static UdpPacket BuildEntryPacket(
        string format,
        uint[]? arguments = null,
        uint faultType = (uint)SystemFault.InterlockFault,
        uint clearEpoch = 1,
        uint entryIndex = 0,
        uint activeCount = 1,
        GcbStateNew state = GcbStateNew.Fault,
        uint runtime = 123,
        uint? argumentCount = null,
        uint? transmittedHash = null,
        GCBPacketType packetType = GCBPacketType.FaultInfoResponse)
    {
        arguments ??= [];
        byte[] visibleFormat = Encoding.ASCII.GetBytes(format);
        return BuildPacket(
            faultType,
            transmittedHash ?? CrcUtils.ComputeChecksum(visibleFormat),
            clearEpoch,
            entryIndex,
            activeCount,
            state,
            runtime,
            argumentCount ?? (uint)arguments.Length,
            EncodeFormat(visibleFormat),
            arguments,
            packetType);
    }

    private static UdpPacket BuildEmptyPacket(uint clearEpoch, uint entryIndex, uint activeCount) =>
        BuildPacket(
            faultType: 0,
            formatHash: 0,
            clearEpoch: clearEpoch,
            entryIndex: entryIndex,
            activeCount: activeCount,
            state: GcbStateNew.Startup,
            runtime: 0,
            argumentCount: 0,
            formatField: new byte[FormatBytes],
            arguments: []);

    private static UdpPacket BuildPacket(
        uint faultType,
        uint formatHash,
        uint clearEpoch,
        uint entryIndex,
        uint activeCount,
        GcbStateNew state,
        uint runtime,
        uint argumentCount,
        byte[] formatField,
        uint[] arguments,
        GCBPacketType packetType = GCBPacketType.FaultInfoResponse)
    {
        if (formatField.Length != FormatBytes)
            throw new ArgumentException($"Format field must contain exactly {FormatBytes} bytes.", nameof(formatField));
        if (arguments.Length > 5)
            throw new ArgumentException("At most five arguments can be encoded.", nameof(arguments));

        var packet = new UdpPacket((uint)packetType, 99, ResponseWords);
        packet[0] = faultType;
        packet[1] = formatHash;
        packet[2] = clearEpoch;
        packet[3] = entryIndex;
        packet[4] = activeCount;
        packet[5] = unchecked((uint)(int)state);
        packet[6] = runtime;
        packet[7] = argumentCount;

        for (int index = 0; index < FormatBytes / sizeof(uint); ++index)
            packet[8 + index] = BinaryPrimitives.ReadUInt32LittleEndian(formatField.AsSpan(index * sizeof(uint), sizeof(uint)));
        for (int index = 0; index < arguments.Length; ++index)
            packet[40 + index] = arguments[index];

        return packet.UpdateCRC();
    }


    private static byte[] EncodeFormat(byte[] visibleFormat)
    {
        if (visibleFormat.Length >= FormatBytes)
            throw new ArgumentException("Visible format must leave room for a NUL terminator.", nameof(visibleFormat));

        var field = new byte[FormatBytes];
        visibleFormat.CopyTo(field, 0);
        return field;
    }
}
