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
        var packet = NewTelemetryPacket((uint)NormalTelemetryField.PayloadFields);
        packet[(int)NormalTelemetryField.SystemState] = (int)GcbStateNew.Emission;
        packet[(int)NormalTelemetryField.SystemRuntime] = 101;
        packet[(int)NormalTelemetryField.SystemFaultFlags] =
            (1u << 3) | (1u << 23) | (1u << 24);
        packet[(int)NormalTelemetryField.InterlockFlags] = 0xDFFFFu;
        packet[(int)NormalTelemetryField.RingLedState] = (int)RingLedState.TBD2;
        packet[(int)NormalTelemetryField.BaseLedState] = (int)BaseLedState.TBD2;
        packet[(int)NormalTelemetryField.Collimator1] = 0x89ABCDEFu;
        packet[(int)NormalTelemetryField.Collimator2] = 0x01234567u;
        packet[(int)NormalTelemetryField.Buttons] = 108;
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
        packet[(int)NormalTelemetryField.QcChannel0Reading] = 142.5f;
        packet[(int)NormalTelemetryField.KvSetpoint] = 143.5f;
        packet[(int)NormalTelemetryField.EmissionCurrentLimit] = 144.5f;
        packet[(int)NormalTelemetryField.HvpsPowerSetpoint] = 145.5f;
        packet[(int)NormalTelemetryField.RequiredInterlockFlags] = (1u << 0) | (1u << 4);
        packet[(int)NormalTelemetryField.QcChannel1Reading] = 146.5f;
        packet[(int)NormalTelemetryField.QcAdcI2cStatus] = 3u;
        packet[(int)NormalTelemetryField.QcChannel0Accumulation] = 0xF0000001u;
        packet[(int)NormalTelemetryField.QcChannel1Accumulation] = 0xE0000002u;

        var telemetry = SystemNormalTelemetry.Parse(packet.UpdateCRC().Buffer);

        Assert.Multiple(() =>
        {
            Assert.That(telemetry.FirmwareMode, Is.EqualTo(FirmwareMode.Normal));
            Assert.That(telemetry.ControlBoardState, Is.EqualTo(GcbStateNew.Emission));
            Assert.That(telemetry.SystemRuntime, Is.EqualTo(101));
            Assert.That(telemetry.Faults.RawFlags,
                Is.EqualTo((1u << 3) | (1u << 23) | (1u << 24)));
            Assert.That(telemetry.Faults.RawCommunicationFlags, Is.Null);
            Assert.That(telemetry.Faults.GetState(SystemFault.VoltageFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.InvalidConfigFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.MagnetometerFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.CurrentFault), Is.False);
            Assert.That(telemetry.Interlocks.RawFlags, Is.EqualTo(0xDFFFFu));
            Assert.That(telemetry.Interlocks.RawRequiredFlags, Is.EqualTo((1u << 0) | (1u << 4)));
            Assert.That(telemetry.Interlocks.DoorClosed, Is.True);
            Assert.That(telemetry.Interlocks.SpareInterlock2, Is.True);
            Assert.That(telemetry.Interlocks.Kuka1Ready, Is.True);
            Assert.That(telemetry.Interlocks.McuFaultClear, Is.True);
            Assert.That(telemetry.Interlocks.SpareInterlock1, Is.True);
            Assert.That(telemetry.Interlocks.MasterFaultClear, Is.True);
            Assert.That(telemetry.Interlocks.BaseKeyOn, Is.True);
            Assert.That(telemetry.Interlocks.IsRequired(SystemInterlock.DoorClosed), Is.True);
            Assert.That(telemetry.Interlocks.IsRequired(SystemInterlock.Kuka1Ready), Is.True);
            Assert.That(telemetry.Interlocks.IsRequired(SystemInterlock.BaseKeyOn), Is.False);
            Assert.That(telemetry.RingLedState, Is.EqualTo(RingLedState.TBD2));
            Assert.That(telemetry.BaseLedState, Is.EqualTo(BaseLedState.TBD2));
            Assert.That(telemetry.CollimatorId1, Is.EqualTo(0x89ABCDEFu));
            Assert.That(telemetry.CollimatorId2, Is.EqualTo(0x01234567u));
            Assert.That(telemetry.CollimatorSerial, Is.EqualTo(0x0123456789ABCDEFul));
            Assert.That(telemetry.ButtonsState, Is.EqualTo(108));
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
            Assert.That(telemetry.QcChannel0Reading, Is.EqualTo(142.5f));
            Assert.That(telemetry.QcChannel1Reading, Is.EqualTo(146.5f));
            Assert.That(telemetry.QcAdc1Connected, Is.True);
            Assert.That(telemetry.QcAdc2Connected, Is.True);
            Assert.That(telemetry.QcChannel0Accumulation, Is.EqualTo(0xF0000001u));
            Assert.That(telemetry.QcChannel1Accumulation, Is.EqualTo(0xE0000002u));
            Assert.That(telemetry.KvSetpoint, Is.EqualTo(143.5f));
            Assert.That(telemetry.EmissionCurrentLimit, Is.EqualTo(144.5f));
            Assert.That(telemetry.HvpsPowerSetpoint, Is.EqualTo(145.5f));
            Assert.That(telemetry.IsEmissionState(), Is.True);
        });
    }

    [Test]
    public void NormalTelemetryFields_PreserveAppendOnlyQcIndices()
    {
        Assert.Multiple(() =>
        {
            Assert.That((int)NormalTelemetryField.QcChannel0Reading, Is.EqualTo(40));
            Assert.That((int)NormalTelemetryField.QcChannel1Reading, Is.EqualTo(45));
            Assert.That((int)NormalTelemetryField.QcAdcI2cStatus, Is.EqualTo(46));
            Assert.That((int)NormalTelemetryField.QcChannel0Accumulation, Is.EqualTo(47));
            Assert.That((int)NormalTelemetryField.QcChannel1Accumulation, Is.EqualTo(48));
            Assert.That((int)NormalTelemetryField.PayloadFields, Is.EqualTo(49));
        });
    }


    [TestCase(46)]
    [TestCase(48)]
    public void NormalParser_RejectsInvalidFieldCounts(int fieldCount)
    {
        var packet = NewTelemetryPacket((uint)fieldCount).UpdateCRC();
        Assert.That(() => SystemNormalTelemetry.Parse(packet.Buffer), Throws.ArgumentException);
    }

    [Test]
    public void CalibrationParser_MapsAuthoritativeLayoutAndUnavailableValues()
    {
        var packet = NewTelemetryPacket((uint)CalibrationTelemetryField.PayloadFields);
        packet[(int)CalibrationTelemetryField.SystemState] =
            (int)GcbStateNew.WarmupFault;
        packet[(int)CalibrationTelemetryField.InternalTimerState] = 104;
        packet[(int)CalibrationTelemetryField.Timer1State] = 105;
        packet[(int)CalibrationTelemetryField.Timer2State] = 106;
        packet[(int)CalibrationTelemetryField.SystemRuntime] = 107;
        packet[(int)CalibrationTelemetryField.HvpsRuntime] = 108;
        packet[(int)CalibrationTelemetryField.Buttons] = 111;
        packet[(int)CalibrationTelemetryField.SystemFaultFlags] =
            (1u << 5) | (1u << 22) | (1u << 24);
        packet[(int)CalibrationTelemetryField.CommunicationFaultFlags] =
            0xA5A5A5A5u;
        packet[(int)CalibrationTelemetryField.InterlockFlags] = 0xDFFFFu;
        packet[(int)CalibrationTelemetryField.HvpsIO] = 0x80010182u;
        packet[(int)CalibrationTelemetryField.HvpsStatusFlags] = 0x80080609u;
        packet[(int)CalibrationTelemetryField.HvpsErrorFlags] = 0xDEADBEEFu;
        packet[(int)CalibrationTelemetryField.InternalTimerValue] = 118.5f;
        packet[(int)CalibrationTelemetryField.Timer1Value] = 119.5f;
        packet[(int)CalibrationTelemetryField.Timer2Value] = 120.5f;
        packet[(int)CalibrationTelemetryField.KvFeedback] = 121.5f;
        packet[(int)CalibrationTelemetryField.MaFeedback] = 122.5f;
        packet[(int)CalibrationTelemetryField.GridFeedback] = 123.5f;
        packet[(int)CalibrationTelemetryField.FilamentFeedback] = 124.5f;
        packet[(int)CalibrationTelemetryField.FilamentSetpoint] = 125.5f;
        packet[(int)CalibrationTelemetryField.XCoilCurrent] = 129.5f;
        packet[(int)CalibrationTelemetryField.YCoilCurrent] = 130.5f;
        packet[(int)CalibrationTelemetryField.FocusCoilCurrent] = 133.5f;
        packet[(int)CalibrationTelemetryField.IonPumpFeedback] = 135.5f;
        packet[(int)CalibrationTelemetryField.WaterPressure] = 138.5f;
        packet[(int)CalibrationTelemetryField.WaterFlow] = 139.5f;
        packet[(int)CalibrationTelemetryField.WaterTemp] = 140.5f;
        packet[(int)CalibrationTelemetryField.HeatsinkTemp] = 141.5f;
        packet[(int)CalibrationTelemetryField.PeltierTemp] = 142.5f;
        packet[(int)CalibrationTelemetryField.CabinetTemp] = 143.5f;
        packet[(int)CalibrationTelemetryField.RequiredInterlockFlags] =
            0xC3FFFu;
        packet[(int)CalibrationTelemetryField.Collimator1] = 0x89ABCDEFu;
        packet[(int)CalibrationTelemetryField.Collimator2] = 0x01234567u;
        packet[(int)CalibrationTelemetryField.Mag1X] = 149.5f;
        packet[(int)CalibrationTelemetryField.Mag1Y] = 150.5f;
        packet[(int)CalibrationTelemetryField.Mag1Z] = 151.5f;
        packet[(int)CalibrationTelemetryField.Mag2X] = 152.5f;
        packet[(int)CalibrationTelemetryField.Mag2Y] = 153.5f;
        packet[(int)CalibrationTelemetryField.Mag2Z] = 154.5f;

        var telemetry = SystemCalibrationTelemetry.Parse(packet.UpdateCRC().Buffer);

        Assert.Multiple(() =>
        {
            Assert.That(telemetry.FirmwareMode, Is.EqualTo(FirmwareMode.Calibration));
            Assert.That(telemetry.ControlBoardState, Is.EqualTo(GcbStateNew.WarmupFault));
            Assert.That(telemetry.InternalTimerState, Is.EqualTo(104));
            Assert.That(telemetry.Timer1State, Is.EqualTo(105));
            Assert.That(telemetry.Timer2State, Is.EqualTo(106));
            Assert.That(telemetry.SystemRuntime, Is.EqualTo(107));
            Assert.That(telemetry.RuntimeCounterHVPS, Is.EqualTo(108));
            Assert.That(telemetry.ButtonsState, Is.EqualTo(111));
            Assert.That(telemetry.Faults.RawFlags,
                Is.EqualTo((1u << 5) | (1u << 22) | (1u << 24)));
            Assert.That(telemetry.Faults.RawCommunicationFlags, Is.EqualTo(0xA5A5A5A5u));
            Assert.That(telemetry.Faults.GetState(SystemFault.FilamentFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.MemoryFault), Is.True);
            Assert.That(telemetry.Faults.GetState(SystemFault.MagnetometerFault), Is.True);
            Assert.That(telemetry.Interlocks.RawFlags, Is.EqualTo(0xDFFFFu));
            Assert.That(telemetry.Interlocks.RawRequiredFlags, Is.EqualTo(0xC3FFFu));
            Assert.That(telemetry.Interlocks.SpareInterlock2, Is.True);
            Assert.That(telemetry.Interlocks.Kuka1Ready, Is.True);
            Assert.That(telemetry.Interlocks.Kuka2Ready, Is.True);
            Assert.That(telemetry.Interlocks.McuFaultClear, Is.True);
            Assert.That(telemetry.Interlocks.SpareInterlock1, Is.True);
            Assert.That(telemetry.Interlocks.MasterFaultClear, Is.True);
            Assert.That(telemetry.Interlocks.IsRequired(SystemInterlock.SpareInterlock2), Is.True);
            Assert.That(telemetry.Interlocks.IsRequired(SystemInterlock.McuFaultClear), Is.False);
            Assert.That(telemetry.Hvps.RawIoFlags, Is.EqualTo(0x80010182u));
            Assert.That(telemetry.Hvps.RawStatusFlags, Is.EqualTo(0x80080609u));
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
            Assert.That(telemetry.CollimatorId1, Is.EqualTo(0x89ABCDEFu));
            Assert.That(telemetry.CollimatorId2, Is.EqualTo(0x01234567u));
            Assert.That(telemetry.CollimatorSerial, Is.EqualTo(0x0123456789ABCDEFuL));
            Assert.That(telemetry.KvSetpoint, Is.Null);
            Assert.That(telemetry.EmissionCurrentLimit, Is.Null);
            Assert.That(telemetry.HvpsPowerSetpoint, Is.Null);
            Assert.That(telemetry.GridSetpoint, Is.Null);
            Assert.That(telemetry.Mag1, Is.EqualTo(new TelemetryVector3(149.5f, 150.5f, 151.5f)));
            Assert.That(telemetry.Mag2, Is.EqualTo(new TelemetryVector3(152.5f, 153.5f, 154.5f)));
            Assert.That(telemetry.QcChannel0Reading, Is.Null);
            Assert.That(telemetry.QcChannel1Reading, Is.Null);
            Assert.That(telemetry.QcChannel0Accumulation, Is.Null);
            Assert.That(telemetry.QcChannel1Accumulation, Is.Null);
            Assert.That(telemetry.QcAdc1Connected, Is.Null);
            Assert.That(telemetry.QcAdc2Connected, Is.Null);
            Assert.That(telemetry.IsFaultState(), Is.True);
        });
    }

    [TestCase(52)]
    [TestCase(54)]
    public void CalibrationParser_RequiresExactFieldCount(int fieldCount)
    {
        var packet = NewTelemetryPacket((uint)fieldCount).UpdateCRC();
        Assert.That(() => SystemCalibrationTelemetry.Parse(packet.Buffer), Throws.ArgumentException);
    }

    private static UdpPacket NewTelemetryPacket(uint payloadLength) =>
        new((uint)GCBPacketType.TelemetryResponse, 0, payloadLength);
}
