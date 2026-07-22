using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Test.Xcc.Infra.GryphonBoard;

internal class SystemTelemetryTests
{
    [Test]
    public void NormalParser_MapsAllPublishedFieldsAndSemanticValues()
    {
        var packet = NewTelemetryPacket(46);
        packet[(int)NormalTelemetryField.SystemState] = (int)GcbStateNew.Emission;
        packet[(int)NormalTelemetryField.SystemRuntime] = 101;
        packet[(int)NormalTelemetryField.SystemFaultFlags] = (1u << 3) | (1u << 23);
        packet[(int)NormalTelemetryField.InterlockFlags] =
            (1u << 0) | (1u << 2) | (1u << 3) | (1u << 6) | (1u << 7)
            | (1u << 8) | (1u << 9) | (1u << 10) | (1u << 11) | (1u << 12)
            | (1u << 13) | (1u << 18) | (1u << 19);
        packet[(int)NormalTelemetryField.RingLedState] = (int)RingLedState.TBD2;
        packet[(int)NormalTelemetryField.BaseLedState] = (int)BaseLedState.TBD2;
        packet[(int)NormalTelemetryField.Collimator1] = 0x89ABCDEFu;
        packet[(int)NormalTelemetryField.Collimator2] = 0x01234567u;
        packet[(int)NormalTelemetryField.Buttons] = 108;
        packet[(int)NormalTelemetryField.CurrentPoint] = 109;
        packet[(int)NormalTelemetryField.TotalPoints] = 110;
        packet[(int)NormalTelemetryField.InternalTimerState] = 111;
        packet[(int)NormalTelemetryField.InternalTimerValue] = 112.5f;
        packet[(int)NormalTelemetryField.Timer1State] = 113;
        packet[(int)NormalTelemetryField.Timer1Value] = 114.5f;
        packet[(int)NormalTelemetryField.Timer2State] = 115;
        packet[(int)NormalTelemetryField.Timer2Value] = 116.5f;
        packet[(int)NormalTelemetryField.HvpsRuntime] = 117;
        packet[(int)NormalTelemetryField.HvpsIO] = 0x80010182u;
        packet[(int)NormalTelemetryField.HvpsStatusFlags] = 0x80000209u;
        packet[(int)NormalTelemetryField.KvFeedback] = 120.5f;
        packet[(int)NormalTelemetryField.MaFeedback] = 121.5f;
        packet[(int)NormalTelemetryField.FilamentSetpoint] = 122.5f;
        packet[(int)NormalTelemetryField.FilamentFeedback] = 123.5f;
        packet[(int)NormalTelemetryField.GridSetpoint] = 124.5f;
        packet[(int)NormalTelemetryField.GridFeedback] = 125.5f;
        packet[(int)NormalTelemetryField.XCoilCurrent] = 126.5f;
        packet[(int)NormalTelemetryField.YCoilCurrent] = 127.5f;
        packet[(int)NormalTelemetryField.FocusCoilCurrent] = 128.5f;
        packet[(int)NormalTelemetryField.IonPumpFeedback] = 129.5f;
        packet[(int)NormalTelemetryField.WaterPressure] = 130.5f;
        packet[(int)NormalTelemetryField.WaterFlow] = 131.5f;
        packet[(int)NormalTelemetryField.WaterTemp] = 132.5f;
        packet[(int)NormalTelemetryField.HeatsinkTemp] = 133.5f;
        packet[(int)NormalTelemetryField.PeltierTemp] = 134.5f;
        packet[(int)NormalTelemetryField.CabinetTemp] = 135.5f;
        packet[(int)NormalTelemetryField.Mag1X] = 136.5f;
        packet[(int)NormalTelemetryField.Mag1Y] = 137.5f;
        packet[(int)NormalTelemetryField.Mag1Z] = 138.5f;
        packet[(int)NormalTelemetryField.Mag2X] = 139.5f;
        packet[(int)NormalTelemetryField.Mag2Y] = 140.5f;
        packet[(int)NormalTelemetryField.Mag2Z] = 141.5f;
        packet[(int)NormalTelemetryField.TvmInterlock] = 5u;
        packet[(int)NormalTelemetryField.KvSetpoint] = 143.5f;
        packet[(int)NormalTelemetryField.EmissionCurrentLimit] = 144.5f;
        packet[(int)NormalTelemetryField.HvpsPowerSetpoint] = 145.5f;

        var telemetry = SystemNormalTelemetry.Parse(packet.UpdateCRC().Buffer);

        Assert.Multiple(() =>
        {
            Assert.That(telemetry.FirmwareMode, Is.EqualTo(FirmwareMode.Normal));
            Assert.That(telemetry.ControlBoardState, Is.EqualTo(GcbStateNew.Emission));
            Assert.That(telemetry.SystemRuntime, Is.EqualTo(101));
            Assert.That(telemetry.Faults.RawFlags, Is.EqualTo((1u << 3) | (1u << 23)));
            Assert.That(telemetry.Faults.RawCommunicationFlags, Is.Null);
            Assert.That(telemetry.Faults.GetState(SystemFault.VoltageFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.InvalidConfigFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.CurrentFault), Is.False);
            Assert.That(telemetry.Interlocks.RawFlags, Is.EqualTo(0xC3FCDu));
            Assert.That(telemetry.Interlocks.RawTvmFlags, Is.EqualTo(5u));
            Assert.That(telemetry.Interlocks.DoorClosed, Is.True);
            Assert.That(telemetry.Interlocks.CollimatorOn, Is.True);
            Assert.That(telemetry.Interlocks.TvmNotchEngaged, Is.True);
            Assert.That(telemetry.Interlocks.DriveSystemReady, Is.Null);
            Assert.That(telemetry.Interlocks.Kuka1Ready, Is.Null);
            Assert.That(telemetry.RingLedState, Is.EqualTo(RingLedState.TBD2));
            Assert.That(telemetry.BaseLedState, Is.EqualTo(BaseLedState.TBD2));
            Assert.That(telemetry.CollimatorId1, Is.EqualTo(0x89ABCDEFu));
            Assert.That(telemetry.CollimatorId2, Is.EqualTo(0x01234567u));
            Assert.That(telemetry.CollimatorSerial, Is.EqualTo(0x0123456789ABCDEFul));
            Assert.That(telemetry.ButtonsState, Is.EqualTo(108));
            Assert.That(telemetry.CurrentOperationalPoint, Is.EqualTo(109));
            Assert.That(telemetry.TotalOperationalPoints, Is.EqualTo(110));
            Assert.That(telemetry.InternalTimerState, Is.EqualTo(111));
            Assert.That(telemetry.PrimaryTimerValue, Is.EqualTo(112.5f));
            Assert.That(telemetry.Timer1State, Is.EqualTo(113));
            Assert.That(telemetry.SecondaryTimer1Value, Is.EqualTo(114.5f));
            Assert.That(telemetry.Timer2State, Is.EqualTo(115));
            Assert.That(telemetry.SecondaryTimer2Value, Is.EqualTo(116.5f));
            Assert.That(telemetry.RuntimeCounterHVPS, Is.EqualTo(117));
            Assert.That(telemetry.Hvps.RawIoFlags, Is.EqualTo(0x80010182u));
            Assert.That(telemetry.Hvps.RawStatusFlags, Is.EqualTo(0x80000209u));
            Assert.That(telemetry.Hvps.RawErrorFlags, Is.Null);
            Assert.That(telemetry.KvFeedback, Is.EqualTo(120.5f));
            Assert.That(telemetry.EmissionCurrent, Is.EqualTo(121.5f));
            Assert.That(telemetry.HeaterCurrentSetpoint, Is.EqualTo(122.5f));
            Assert.That(telemetry.HeaterCurrentFeedback, Is.EqualTo(123.5f));
            Assert.That(telemetry.GridSetpoint, Is.EqualTo(124.5f));
            Assert.That(telemetry.GridVoltage, Is.EqualTo(125.5f));
            Assert.That(telemetry.XCoilCurrent, Is.EqualTo(126.5f));
            Assert.That(telemetry.YCoilCurrent, Is.EqualTo(127.5f));
            Assert.That(telemetry.FocusCurrent, Is.EqualTo(128.5f));
            Assert.That(telemetry.IonPumpFeedback, Is.EqualTo(129.5f));
            Assert.That(telemetry.WaterPressure, Is.EqualTo(130.5f));
            Assert.That(telemetry.WaterFlowRate, Is.EqualTo(131.5f));
            Assert.That(telemetry.WaterTemperature, Is.EqualTo(132.5f));
            Assert.That(telemetry.HeatSinkTemperature, Is.EqualTo(133.5f));
            Assert.That(telemetry.PeltierTemperature, Is.EqualTo(134.5f));
            Assert.That(telemetry.CabinetTemperature, Is.EqualTo(135.5f));
            Assert.That(telemetry.Mag1, Is.EqualTo(new TelemetryVector3(136.5f, 137.5f, 138.5f)));
            Assert.That(telemetry.Mag2, Is.EqualTo(new TelemetryVector3(139.5f, 140.5f, 141.5f)));
            Assert.That(telemetry.KvSetpoint, Is.EqualTo(143.5f));
            Assert.That(telemetry.EmissionCurrentLimit, Is.EqualTo(144.5f));
            Assert.That(telemetry.HvpsPowerSetpoint, Is.EqualTo(145.5f));
            Assert.That(telemetry.IsEmissionState(), Is.True);
        });
    }

    [TestCase(43, null, null, null)]
    [TestCase(44, 43f, null, null)]
    [TestCase(45, 43f, 44f, null)]
    [TestCase(46, 43f, 44f, 45f)]
    public void NormalParser_PreservesOptionalTailCompatibility(
        int fieldCount,
        float? expectedKv,
        float? expectedCurrent,
        float? expectedPower)
    {
        var packet = NewTelemetryPacket((uint)fieldCount);
        if (fieldCount > 43) packet[43] = 43f;
        if (fieldCount > 44) packet[44] = 44f;
        if (fieldCount > 45) packet[45] = 45f;

        var telemetry = SystemNormalTelemetry.Parse(packet.UpdateCRC().Buffer);

        Assert.Multiple(() =>
        {
            Assert.That(telemetry.KvSetpoint, Is.EqualTo(expectedKv));
            Assert.That(telemetry.EmissionCurrentLimit, Is.EqualTo(expectedCurrent));
            Assert.That(telemetry.HvpsPowerSetpoint, Is.EqualTo(expectedPower));
        });
    }

    [TestCase(42)]
    [TestCase(47)]
    public void NormalParser_RejectsInvalidFieldCounts(int fieldCount)
    {
        var packet = NewTelemetryPacket((uint)fieldCount).UpdateCRC();
        Assert.That(() => SystemNormalTelemetry.Parse(packet.Buffer), Throws.ArgumentException);
    }

    [Test]
    public void CalibrationParser_MapsAuthoritativeLayoutAndUnavailableValues()
    {
        var packet = NewTelemetryPacket(47);
        packet[1] = (int)GcbStateNew.WarmupFault;
        packet[2] = 102;
        packet[3] = 103;
        packet[4] = 104;
        packet[5] = 105;
        packet[6] = 106;
        packet[7] = 107;
        packet[8] = 108;
        packet[11] = 111;
        packet[12] = (1u << 5) | (1u << 22);
        packet[13] = 0xA5A5A5A5u;
        packet[14] = 0xC3FFFu;
        packet[15] = 0x80010182u;
        packet[16] = 0x80000209u;
        packet[17] = 0xDEADBEEFu;
        packet[18] = 118.5f;
        packet[19] = 119.5f;
        packet[20] = 120.5f;
        packet[21] = 121.5f;
        packet[22] = 122.5f;
        packet[23] = 123.5f;
        packet[24] = 124.5f;
        packet[25] = 125.5f;
        packet[29] = 129.5f;
        packet[30] = 130.5f;
        packet[33] = 133.5f;
        packet[35] = 135.5f;
        packet[38] = 138.5f;
        packet[39] = 139.5f;
        packet[40] = 140.5f;
        packet[41] = 141.5f;
        packet[42] = 142.5f;
        packet[43] = 143.5f;

        var telemetry = SystemCalibrationTelemetry.Parse(packet.UpdateCRC().Buffer);

        Assert.Multiple(() =>
        {
            Assert.That(telemetry.FirmwareMode, Is.EqualTo(FirmwareMode.Calibration));
            Assert.That(telemetry.ControlBoardState, Is.EqualTo(GcbStateNew.WarmupFault));
            Assert.That(telemetry.CurrentOperationalPoint, Is.EqualTo(102));
            Assert.That(telemetry.TotalOperationalPoints, Is.EqualTo(103));
            Assert.That(telemetry.InternalTimerState, Is.EqualTo(104));
            Assert.That(telemetry.Timer1State, Is.EqualTo(105));
            Assert.That(telemetry.Timer2State, Is.EqualTo(106));
            Assert.That(telemetry.SystemRuntime, Is.EqualTo(107));
            Assert.That(telemetry.RuntimeCounterHVPS, Is.EqualTo(108));
            Assert.That(telemetry.ButtonsState, Is.EqualTo(111));
            Assert.That(telemetry.Faults.RawFlags, Is.EqualTo((1u << 5) | (1u << 22)));
            Assert.That(telemetry.Faults.RawCommunicationFlags, Is.EqualTo(0xA5A5A5A5u));
            Assert.That(telemetry.Faults.GetState(SystemFault.FilamentFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.MemoryFault), Is.True);
            Assert.That(telemetry.Interlocks.RawFlags, Is.EqualTo(0xC3FFFu));
            Assert.That(telemetry.Interlocks.RawTvmFlags, Is.Null);
            Assert.That(telemetry.Interlocks.DriveSystemReady, Is.True);
            Assert.That(telemetry.Interlocks.Kuka1Ready, Is.True);
            Assert.That(telemetry.Interlocks.Kuka2Ready, Is.True);
            Assert.That(telemetry.Interlocks.TvmNotchEngaged, Is.Null);
            Assert.That(telemetry.Hvps.RawIoFlags, Is.EqualTo(0x80010182u));
            Assert.That(telemetry.Hvps.RawStatusFlags, Is.EqualTo(0x80000209u));
            Assert.That(telemetry.Hvps.RawErrorFlags, Is.EqualTo(0xDEADBEEFu));
            Assert.That(telemetry.PrimaryTimerValue, Is.EqualTo(118.5f));
            Assert.That(telemetry.SecondaryTimer1Value, Is.EqualTo(119.5f));
            Assert.That(telemetry.SecondaryTimer2Value, Is.EqualTo(120.5f));
            Assert.That(telemetry.KvFeedback, Is.EqualTo(121.5f));
            Assert.That(telemetry.EmissionCurrent, Is.EqualTo(122.5f));
            Assert.That(telemetry.GridVoltage, Is.EqualTo(123.5f));
            Assert.That(telemetry.HeaterCurrentFeedback, Is.EqualTo(124.5f));
            Assert.That(telemetry.HeaterCurrentSetpoint, Is.EqualTo(125.5f));
            Assert.That(telemetry.XCoilCurrent, Is.EqualTo(129.5f));
            Assert.That(telemetry.YCoilCurrent, Is.EqualTo(130.5f));
            Assert.That(telemetry.FocusCurrent, Is.EqualTo(133.5f));
            Assert.That(telemetry.IonPumpFeedback, Is.EqualTo(135.5f));
            Assert.That(telemetry.WaterPressure, Is.EqualTo(138.5f));
            Assert.That(telemetry.WaterFlowRate, Is.EqualTo(139.5f));
            Assert.That(telemetry.WaterTemperature, Is.EqualTo(140.5f));
            Assert.That(telemetry.HeatSinkTemperature, Is.EqualTo(141.5f));
            Assert.That(telemetry.PeltierTemperature, Is.EqualTo(142.5f));
            Assert.That(telemetry.CabinetTemperature, Is.EqualTo(143.5f));
            Assert.That(telemetry.RingLedState, Is.Null);
            Assert.That(telemetry.BaseLedState, Is.Null);
            Assert.That(telemetry.CollimatorId1, Is.Null);
            Assert.That(telemetry.CollimatorId2, Is.Null);
            Assert.That(telemetry.CollimatorSerial, Is.Null);
            Assert.That(telemetry.KvSetpoint, Is.Null);
            Assert.That(telemetry.EmissionCurrentLimit, Is.Null);
            Assert.That(telemetry.HvpsPowerSetpoint, Is.Null);
            Assert.That(telemetry.GridSetpoint, Is.Null);
            Assert.That(telemetry.Mag1, Is.Null);
            Assert.That(telemetry.Mag2, Is.Null);
            Assert.That(telemetry.IsFaultState(), Is.True);
        });
    }

    [TestCase(46)]
    [TestCase(48)]
    public void CalibrationParser_RequiresExactlyFortySevenFields(int fieldCount)
    {
        var packet = NewTelemetryPacket((uint)fieldCount).UpdateCRC();
        Assert.That(() => SystemCalibrationTelemetry.Parse(packet.Buffer), Throws.ArgumentException);
    }

    private static UdpPacket NewTelemetryPacket(uint payloadLength) =>
        new((uint)GCBPacketType.TelemetryResponse, 0, payloadLength);
}
