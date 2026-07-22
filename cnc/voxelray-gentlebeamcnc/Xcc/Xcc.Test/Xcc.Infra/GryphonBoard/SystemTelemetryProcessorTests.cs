using Empyrean.Common.Infra.Networking.Udp;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.CommandAPI;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Test.Xcc.Infra.GryphonBoard;

internal class SystemTelemetryProcessorTests
{
    [Test]
    public void Process_SuppressesTelemetryBeforeVersionInfo()
    {
        var callback = new Mock<ISystemTelemetryChanged>();
        var sut = CreateSut(callback.Object);

        Assert.That(sut.Process(BuildNormalTelemetry(0)), Is.False);
        callback.Verify(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()), Times.Never);
    }

    [TestCase(2, 0, 1, FirmwareMode.Normal, typeof(SystemNormalTelemetry))]
    [TestCase(1, 0, 0, FirmwareMode.Calibration, typeof(SystemCalibrationTelemetry))]
    public void Process_SelectsOnlyExactSupportedSignatures(
        int major,
        int minor,
        int level,
        FirmwareMode mode,
        Type expectedType)
    {
        ISystemTelemetry? published = null;
        var callback = new Mock<ISystemTelemetryChanged>();
        callback.Setup(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()))
            .Callback<ISystemTelemetry?>(value => published = value);
        var sut = CreateSut(callback.Object);

        Assert.That(sut.Process(BuildVersionInfo(major, minor, level, mode)), Is.False);
        var result = mode == FirmwareMode.Normal
            ? sut.Process(BuildNormalTelemetry(0))
            : sut.Process(BuildCalibrationTelemetry(0));

        Assert.That(result, Is.True);
        Assert.That(published, Is.TypeOf(expectedType));
    }

    [Test]
    public void Process_UnsupportedVersionClearsPreviousSelection()
    {
        var callback = new Mock<ISystemTelemetryChanged>();
        var sut = CreateSut(callback.Object);
        sut.Process(BuildVersionInfo(2, 0, 1, FirmwareMode.Normal));
        Assert.That(sut.Process(BuildNormalTelemetry(0)), Is.True);

        Assert.That(sut.Process(BuildVersionInfo(2, 0, 2, FirmwareMode.Normal)), Is.False);
        Assert.That(sut.Process(BuildNormalTelemetry(300)), Is.False);
        callback.Verify(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()), Times.Once);
    }

    [Test]
    public void Process_PublishesAtFirmwareRuntimeIntervals()
    {
        var runtimes = new List<int>();
        var callback = new Mock<ISystemTelemetryChanged>();
        callback.Setup(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()))
            .Callback<ISystemTelemetry?>(value => runtimes.Add(value!.SystemRuntime));
        var sut = CreateSut(callback.Object);
        sut.Process(BuildVersionInfo(2, 0, 1, FirmwareMode.Normal));

        foreach (var runtime in new[] { 0, 100, 200, 300 })
            Assert.That(sut.Process(BuildNormalTelemetry(runtime)), Is.True);

        Assert.That(runtimes, Is.EqualTo(new[] { 0, 300 }));
    }

    [Test]
    public void Process_BackwardsRuntimeNeedsVersionInfoToRecoverPublication()
    {
        var runtimes = new List<int>();
        var callback = new Mock<ISystemTelemetryChanged>();
        callback.Setup(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()))
            .Callback<ISystemTelemetry?>(value => runtimes.Add(value!.SystemRuntime));
        var sut = CreateSut(callback.Object);
        var version = BuildVersionInfo(2, 0, 1, FirmwareMode.Normal);

        sut.Process(version);
        Assert.That(sut.Process(BuildNormalTelemetry(300)), Is.True);
        Assert.That(sut.Process(BuildNormalTelemetry(100)), Is.True);
        sut.Process(version);
        Assert.That(sut.Process(BuildNormalTelemetry(100)), Is.True);

        Assert.That(runtimes, Is.EqualTo(new[] { 300, 100 }));
    }

    [Test]
    public void NotifyTelemetryExpired_PublishesNullAndRetainsSelection()
    {
        var published = new List<ISystemTelemetry?>();
        var callback = new Mock<ISystemTelemetryChanged>();
        callback.Setup(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()))
            .Callback<ISystemTelemetry?>(published.Add);
        var sut = CreateSut(callback.Object);

        sut.Process(BuildVersionInfo(2, 0, 1, FirmwareMode.Normal));
        sut.Process(BuildNormalTelemetry(0));
        sut.NotifyTelemetryExpired();
        Assert.That(sut.Process(BuildNormalTelemetry(300)), Is.True);

        Assert.That(published.Select(value => value?.SystemRuntime), Is.EqualTo(new int?[] { 0, null, 300 }));
    }

    [Test]
    public void Process_PublishedSnapshotsRemainStable()
    {
        var published = new List<ISystemTelemetry>();
        var callback = new Mock<ISystemTelemetryChanged>();
        callback.Setup(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()))
            .Callback<ISystemTelemetry?>(value => published.Add(value!));
        var sut = CreateSut(callback.Object);
        sut.Process(BuildVersionInfo(2, 0, 1, FirmwareMode.Normal));

        sut.Process(BuildNormalTelemetry(0, kvFeedback: 50));
        var retained = published[0];
        sut.Process(BuildNormalTelemetry(100, kvFeedback: 60));
        sut.Process(BuildNormalTelemetry(300, kvFeedback: 70));

        Assert.Multiple(() =>
        {
            Assert.That(retained.SystemRuntime, Is.Zero);
            Assert.That(retained.KvFeedback, Is.EqualTo(50));
            Assert.That(published[1].KvFeedback, Is.EqualTo(70));
        });
    }

    [Test]
    public void Process_InvalidPacketsAndWrongLayoutsReturnFalse()
    {
        var callback = new Mock<ISystemTelemetryChanged>();
        var sut = CreateSut(callback.Object);
        var invalidChecksum = BuildNormalTelemetry(0);
        invalidChecksum[0] ^= 1;

        Assert.Multiple(() =>
        {
            Assert.That(sut.Process(invalidChecksum), Is.False);
            Assert.That(sut.Process(Array.Empty<byte>()), Is.False);
        });

        sut.Process(BuildVersionInfo(1, 0, 0, FirmwareMode.Calibration));
        Assert.That(sut.Process(BuildPacket(GCBPacketType.TelemetryResponse, 47)), Is.False);
    }

    [TestCase(FirmwareMode.Normal)]
    [TestCase(FirmwareMode.Calibration)]
    public void Process_DecodesHvpsBitfieldsForBothLayouts(FirmwareMode mode)
    {
        ISystemTelemetry? published = null;
        var callback = new Mock<ISystemTelemetryChanged>();
        callback.Setup(x => x.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()))
            .Callback<ISystemTelemetry?>(value => published = value);
        var sut = CreateSut(callback.Object);

        if (mode == FirmwareMode.Normal)
        {
            sut.Process(BuildVersionInfo(2, 0, 1, FirmwareMode.Normal));
            sut.Process(BuildNormalTelemetry(
                0,
                state: GcbStateNew.Ready,
                faultFlags: 1u << (int)SystemFault.VoltageFault,
                interlockFlags: 0b11u,
                requiredInterlockFlags: 1u,
                statusFlags: 0x80000209,
                ioFlags: 0x80010182,
                kvFeedback: 51.5f));
        }
        else
        {
            sut.Process(BuildVersionInfo(1, 0, 0, FirmwareMode.Calibration));
            sut.Process(BuildCalibrationTelemetry(
                0,
                state: GcbStateNew.Ready,
                faultFlags: 1u << (int)SystemFault.VoltageFault,
                interlockFlags: 0b11u,
                requiredInterlockFlags: 1u,
                statusFlags: 0x80000209,
                ioFlags: 0x80010182,
                errorFlags: 0xDEADBEEF,
                kvFeedback: 51.5f));
        }

        Assert.That(published, Is.Not.Null);
        var telemetry = published!;
        var hvps = telemetry.Hvps;
        Assert.Multiple(() =>
        {
            Assert.That(telemetry.FirmwareMode, Is.EqualTo(mode));
            Assert.That(telemetry.ControlBoardState, Is.EqualTo(GcbStateNew.Ready));
            Assert.That(telemetry.KvFeedback, Is.EqualTo(51.5f));
            Assert.That(telemetry.Faults.GetState(SystemFault.VoltageFault), Is.True);
            Assert.That(telemetry.Interlocks.DoorClosed, Is.True);
            Assert.That(telemetry.Interlocks.SpareInterlock2, Is.True);
            Assert.That(telemetry.Interlocks.IsRequired(SystemInterlock.DoorClosed), Is.True);
            Assert.That(telemetry.Interlocks.RequiredInterlocksReady, Is.True);
            Assert.That(mode == FirmwareMode.Normal
                ? telemetry.CollimatorSerial is not null
                : telemetry.KvSetpoint is null, Is.True);

            Assert.That(hvps.TestMode, Is.True);
            Assert.That(hvps.Warming, Is.True);
            Assert.That(hvps.FastWarmupEnabled, Is.True);
            Assert.That(hvps.HighVoltageControlEnabled, Is.False);
            Assert.That(hvps.GridControlEnabled, Is.False);
            Assert.That(hvps.KilovoltageRamping, Is.False);
            Assert.That(hvps.EmissionOn, Is.False);
            Assert.That(hvps.ConfigurationUnlocked, Is.False);
            Assert.That(hvps.PidEnabled, Is.False);
            Assert.That(hvps.CalibrationGridInterlockEnabled, Is.False);

            Assert.That(hvps.FilamentClockFault, Is.True);
            Assert.That(hvps.PowerFactorCorrectionOk, Is.True);
            Assert.That(hvps.HighVoltageInterlock, Is.True);
            Assert.That(hvps.Temperature2Fault, Is.True);
            Assert.That(hvps.GridClockStatus, Is.False);
            Assert.That(hvps.GridInterlock, Is.False);
            Assert.That(hvps.BeamControl, Is.False);
            Assert.That(hvps.GridStatus, Is.False);
            Assert.That(hvps.CathodeArc, Is.False);
            Assert.That(hvps.FanFault, Is.False);
            Assert.That(hvps.HighVoltageStatus, Is.False);
            Assert.That(hvps.Overcurrent24VoltFault, Is.False);
            Assert.That(hvps.MasterFault, Is.False);
            Assert.That(hvps.HighVoltageOvercurrentFault, Is.False);
            Assert.That(hvps.Temperature1Fault, Is.False);
            Assert.That(hvps.CathodeOvercurrentFault, Is.False);
            Assert.That(hvps.Temperature3Fault, Is.False);
            Assert.That(hvps.UnknownStatusFlags, Is.EqualTo(0x80000000));
            Assert.That(hvps.UnknownIoFlags, Is.EqualTo(0x80000000));
            Assert.That(hvps.HasActiveFaultInput, Is.True);
            Assert.That(hvps.RawErrorFlags, Is.EqualTo(mode == FirmwareMode.Normal ? null : 0xDEADBEEFu));
        });
    }

    [Test]
    public void Process_SteadyState_DoesNotAllocate()
    {
        var callback = new Mock<ISystemTelemetryChanged>();
        var sut = CreateSut(callback.Object);
        var version = BuildVersionInfo(2, 0, 1, FirmwareMode.Normal);
        var first = BuildNormalTelemetry(0);
        var suppressed = BuildNormalTelemetry(100);

        sut.Process(version);
        sut.Process(first);
        for (var index = 0; index < 10; index++)
            sut.Process(suppressed);

        var allSucceeded = true;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 100; index++)
            allSucceeded &= sut.Process(suppressed);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.That(allSucceeded, Is.True);
        Assert.That(allocated, Is.Zero);
    }

    [Test]
    public void Process_FaultResponseUpdatesStoreWithoutPublishingTelemetry()
    {
        const string format = "Telemetry fault.";
        var entry = new FaultEntry(
            SystemFault.OtherFault,
            CrcUtils.ComputeChecksum(System.Text.Encoding.ASCII.GetBytes(format)),
            GcbStateNew.Ready,
            123,
            format,
            format);
        var update = new FaultUpdate(7, 0, 1, entry);
        var callback = new Mock<ISystemTelemetryChanged>();
        var store = new Mock<IGCBDataStore>();
        var sut = CreateSut(callback.Object, store.Object);

        bool result = sut.Process(GcbXRayCmdResponseGenerator.GenerateFaultInfoResponse(0, update));

        Assert.That(result, Is.False);
        store.Verify(value => value.ApplyFaultUpdate(update), Times.Once);
        callback.Verify(value => value.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry?>()), Times.Never);
    }

    [Test]
    public void Process_MalformedFaultResponseDoesNotMutateStore()
    {
        const string format = "Malformed telemetry fault.";
        var entry = new FaultEntry(
            SystemFault.OtherFault,
            CrcUtils.ComputeChecksum(System.Text.Encoding.ASCII.GetBytes(format)),
            GcbStateNew.Ready,
            123,
            format,
            format);
        var update = new FaultUpdate(7, 0, 1, entry);
        var malformed = new UdpPacket(GcbXRayCmdResponseGenerator.GenerateFaultInfoResponse(0, update));
        malformed[1] = entry.FormatHash + 1u;
        malformed.UpdateCRC();
        var callback = new Mock<ISystemTelemetryChanged>();
        var store = new Mock<IGCBDataStore>();
        var sut = CreateSut(callback.Object, store.Object);

        Assert.That(() => sut.Process(malformed.Buffer), Throws.TypeOf<FormatException>());
        store.Verify(value => value.ApplyFaultUpdate(It.IsAny<FaultUpdate>()), Times.Never);
    }

    private static SystemTelemetryProcessor CreateSut(
        ISystemTelemetryChanged callback,
        IGCBDataStore? store = null) =>
        new(callback, store ?? Mock.Of<IGCBDataStore>());

    private static byte[] BuildVersionInfo(int major, int minor, int level, FirmwareMode mode)
    {
        var packet = new UdpPacket((uint)GCBPacketType.VersionInfoResponse, 0, 5);
        packet[0] = major;
        packet[1] = minor;
        packet[2] = level;
        packet[3] = 0;
        packet[4] = (int)mode;
        return packet.UpdateCRC().Buffer;
    }

    private static byte[] BuildNormalTelemetry(
        int runtime,
        GcbStateNew state = GcbStateNew.Startup,
        uint faultFlags = 0,
        uint interlockFlags = 0,
        uint requiredInterlockFlags = 0,
        uint statusFlags = 0,
        uint ioFlags = 0,
        float kvFeedback = 0)
    {
        var packet = new UdpPacket(
            (uint)GCBPacketType.TelemetryResponse,
            0,
            (uint)NormalTelemetryField.PayloadFields);
        packet[(int)NormalTelemetryField.SystemState] = (int)state;
        packet[(int)NormalTelemetryField.SystemRuntime] = runtime;
        packet[(int)NormalTelemetryField.SystemFaultFlags] = faultFlags;
        packet[(int)NormalTelemetryField.InterlockFlags] = interlockFlags;
        packet[(int)NormalTelemetryField.Reserved1] = 1u;
        packet[(int)NormalTelemetryField.RequiredInterlockFlags] = requiredInterlockFlags;
        packet[(int)NormalTelemetryField.HvpsStatusFlags] = statusFlags;
        packet[(int)NormalTelemetryField.HvpsIO] = ioFlags;
        packet[(int)NormalTelemetryField.KvFeedback] = kvFeedback;
        return packet.UpdateCRC().Buffer;
    }

    private static byte[] BuildCalibrationTelemetry(
        int runtime,
        GcbStateNew state = GcbStateNew.Startup,
        uint faultFlags = 0,
        uint interlockFlags = 0,
        uint requiredInterlockFlags = 0,
        uint statusFlags = 0,
        uint ioFlags = 0,
        uint errorFlags = 0,
        float kvFeedback = 0)
    {
        var packet = new UdpPacket((uint)GCBPacketType.TelemetryResponse, 0, 48);
        packet[1] = (int)state;
        packet[7] = runtime;
        packet[12] = faultFlags;
        packet[14] = interlockFlags;
        packet[47] = requiredInterlockFlags;
        packet[15] = ioFlags;
        packet[16] = statusFlags;
        packet[17] = errorFlags;
        packet[21] = kvFeedback;
        return packet.UpdateCRC().Buffer;
    }

    private static byte[] BuildPacket(GCBPacketType packetType, uint payloadLength) =>
        new UdpPacket((uint)packetType, 0, payloadLength).UpdateCRC().Buffer;
}
