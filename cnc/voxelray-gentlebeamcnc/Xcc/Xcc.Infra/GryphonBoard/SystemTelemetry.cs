using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard;

public sealed class SystemNormalTelemetry : ISystemTelemetry
{
    public FirmwareMode FirmwareMode => FirmwareMode.Normal;
    public GcbStateNew ControlBoardState { get; init; }
    public int SystemRuntime { get; init; }
    public SystemFaults Faults { get; init; }
    public SystemInterlocks Interlocks { get; init; }
    public RingLedState? RingLedState { get; init; }
    public BaseLedState? BaseLedState { get; init; }
    public uint? CollimatorId1 { get; init; }
    public uint? CollimatorId2 { get; init; }
    public ulong? CollimatorSerial { get; init; }
    public int ButtonsState { get; init; }
    public int CurrentOperationalPoint { get; init; }
    public int TotalOperationalPoints { get; init; }
    public int InternalTimerState { get; init; }
    public float PrimaryTimerValue { get; init; }
    public int Timer1State { get; init; }
    public float SecondaryTimer1Value { get; init; }
    public int Timer2State { get; init; }
    public float SecondaryTimer2Value { get; init; }
    public int RuntimeCounterHVPS { get; init; }
    public HvpsTelemetryStatus Hvps { get; init; }
    public float? KvSetpoint { get; init; }
    public float KvFeedback { get; init; }
    public float EmissionCurrent { get; init; }
    public float HeaterCurrentSetpoint { get; init; }
    public float HeaterCurrentFeedback { get; init; }
    public float? EmissionCurrentLimit { get; init; }
    public float? HvpsPowerSetpoint { get; init; }
    public float? GridSetpoint { get; init; }
    public float GridVoltage { get; init; }
    public float XCoilCurrent { get; init; }
    public float YCoilCurrent { get; init; }
    public float FocusCurrent { get; init; }
    public float IonPumpFeedback { get; init; }
    public float WaterPressure { get; init; }
    public float WaterFlowRate { get; init; }
    public float WaterTemperature { get; init; }
    public float HeatSinkTemperature { get; init; }
    public float PeltierTemperature { get; init; }
    public float CabinetTemperature { get; init; }
    public TelemetryVector3? Mag1 { get; init; }
    public TelemetryVector3? Mag2 { get; init; }

    public static SystemNormalTelemetry Parse(byte[] data)
    {
        var state = new NormalTelemetryState();
        state.Update(new UdpPacket(data));
        return state.Snapshot();
    }

    public bool IsFaultState() => IsFaultState(ControlBoardState);
    public bool IsEmissionState() => IsEmissionState(ControlBoardState);
    public override string ToString() => TelemetryFormatter.Format(this, verticallyAligned: false);
    public string GetVerticallyFormattedString() => TelemetryFormatter.Format(this, verticallyAligned: true);

    internal static bool IsFaultState(GcbStateNew state) => state is
        GcbStateNew.Fault or GcbStateNew.ColdFault or GcbStateNew.WarmupFault;

    internal static bool IsEmissionState(GcbStateNew state) => state is
        GcbStateNew.Emission or GcbStateNew.Imaging;
}

public sealed class SystemCalibrationTelemetry : ISystemTelemetry
{
    public FirmwareMode FirmwareMode => FirmwareMode.Calibration;
    public GcbStateNew ControlBoardState { get; init; }
    public int SystemRuntime { get; init; }
    public SystemFaults Faults { get; init; }
    public SystemInterlocks Interlocks { get; init; }
    public RingLedState? RingLedState { get; init; }
    public BaseLedState? BaseLedState { get; init; }
    public uint? CollimatorId1 { get; init; }
    public uint? CollimatorId2 { get; init; }
    public ulong? CollimatorSerial { get; init; }
    public int ButtonsState { get; init; }
    public int CurrentOperationalPoint { get; init; }
    public int TotalOperationalPoints { get; init; }
    public int InternalTimerState { get; init; }
    public float PrimaryTimerValue { get; init; }
    public int Timer1State { get; init; }
    public float SecondaryTimer1Value { get; init; }
    public int Timer2State { get; init; }
    public float SecondaryTimer2Value { get; init; }
    public int RuntimeCounterHVPS { get; init; }
    public HvpsTelemetryStatus Hvps { get; init; }
    public float? KvSetpoint { get; init; }
    public float KvFeedback { get; init; }
    public float EmissionCurrent { get; init; }
    public float HeaterCurrentSetpoint { get; init; }
    public float HeaterCurrentFeedback { get; init; }
    public float? EmissionCurrentLimit { get; init; }
    public float? HvpsPowerSetpoint { get; init; }
    public float? GridSetpoint { get; init; }
    public float GridVoltage { get; init; }
    public float XCoilCurrent { get; init; }
    public float YCoilCurrent { get; init; }
    public float FocusCurrent { get; init; }
    public float IonPumpFeedback { get; init; }
    public float WaterPressure { get; init; }
    public float WaterFlowRate { get; init; }
    public float WaterTemperature { get; init; }
    public float HeatSinkTemperature { get; init; }
    public float PeltierTemperature { get; init; }
    public float CabinetTemperature { get; init; }
    public TelemetryVector3? Mag1 { get; init; }
    public TelemetryVector3? Mag2 { get; init; }

    public static SystemCalibrationTelemetry Parse(byte[] data)
    {
        var state = new CalibrationTelemetryState();
        state.Update(new UdpPacket(data));
        return state.Snapshot();
    }

    public bool IsFaultState() => IsFaultState(ControlBoardState);
    public bool IsEmissionState() => IsEmissionState(ControlBoardState);
    public override string ToString() => TelemetryFormatter.Format(this, verticallyAligned: false);
    public string GetVerticallyFormattedString() => TelemetryFormatter.Format(this, verticallyAligned: true);

    internal static bool IsFaultState(GcbStateNew state) => state is
        GcbStateNew.Fault or GcbStateNew.ColdFault or GcbStateNew.WarmupFault;

    internal static bool IsEmissionState(GcbStateNew state) => state is
        GcbStateNew.Emission or GcbStateNew.Imaging;
}

internal static class SystemInterlockTranslator
{
    private const uint PhysicalInputMask = (uint)GcbInterlockFlags.All;

    internal const ulong AvailablePhysicalInterlocks = PhysicalInputMask;

    internal static ulong Translate(uint rawFlags) => rawFlags & PhysicalInputMask;
}

internal sealed class NormalTelemetryState
{
    private const ulong AvailableFaults =
        (1UL << (int)SystemFault.InterlockFault)
        | (1UL << (int)SystemFault.HvpsReportedFault)
        | (1UL << (int)SystemFault.VoltageFault)
        | (1UL << (int)SystemFault.CurrentFault)
        | (1UL << (int)SystemFault.FilamentFault)
        | (1UL << (int)SystemFault.GridFault)
        | (1UL << (int)SystemFault.CoilFault)
        | (1UL << (int)SystemFault.IonPumpFault)
        | (1UL << (int)SystemFault.IonRepellerFault)
        | (1UL << (int)SystemFault.PeltierFault)
        | (1UL << (int)SystemFault.HeatsinkFault)
        | (1UL << (int)SystemFault.CoolantFault)
        | (1UL << (int)SystemFault.InternalSupplyVoltageFault)
        | (1UL << (int)SystemFault.PcCommFault)
        | (1UL << (int)SystemFault.HvpsCommFault)
        | (1UL << (int)SystemFault.TimerCommFault)
        | (1UL << (int)SystemFault.HeadBoardCommFault)
        | (1UL << (int)SystemFault.LedBoardCommFault)
        | (1UL << (int)SystemFault.PeltierControllerCommFault)
        | (1UL << (int)SystemFault.QcWellCommFault)
        | (1UL << (int)SystemFault.AdcBusCommFault)
        | (1UL << (int)SystemFault.MemoryFault)
        | (1UL << (int)SystemFault.InvalidConfigFault);


    private GcbStateNew _controlBoardState;
    private int _systemRuntime;
    private SystemFaults _faults;
    private SystemInterlocks _interlocks;
    private RingLedState _ringLedState;
    private BaseLedState _baseLedState;
    private uint _collimatorId1;
    private uint _collimatorId2;
    private ulong _collimatorSerial;
    private int _buttonsState;
    private int _currentOperationalPoint;
    private int _totalOperationalPoints;
    private int _internalTimerState;
    private float _primaryTimerValue;
    private int _timer1State;
    private float _secondaryTimer1Value;
    private int _timer2State;
    private float _secondaryTimer2Value;
    private int _runtimeCounterHvps;
    private HvpsTelemetryStatus _hvps;
    private float? _kvSetpoint;
    private float _kvFeedback;
    private float _emissionCurrent;
    private float _heaterCurrentSetpoint;
    private float _heaterCurrentFeedback;
    private float? _emissionCurrentLimit;
    private float? _hvpsPowerSetpoint;
    private float _gridSetpoint;
    private float _gridVoltage;
    private float _xCoilCurrent;
    private float _yCoilCurrent;
    private float _focusCurrent;
    private float _ionPumpFeedback;
    private float _waterPressure;
    private float _waterFlowRate;
    private float _waterTemperature;
    private float _heatSinkTemperature;
    private float _peltierTemperature;
    private float _cabinetTemperature;
    private TelemetryVector3 _mag1;
    private TelemetryVector3 _mag2;

    internal uint Runtime => unchecked((uint)_systemRuntime);

    internal void Update(UdpPacket packet)
    {
        if (packet.PacketType != (uint)GCBPacketType.TelemetryResponse
            || packet.PayloadLength != (uint)NormalTelemetryField.PayloadFields)
        {
            throw new ArgumentException("Invalid normal telemetry packet");
        }

        _controlBoardState = (GcbStateNew)(int)packet[(int)NormalTelemetryField.SystemState];
        _systemRuntime = packet[(int)NormalTelemetryField.SystemRuntime];
        var rawFaults = (uint)packet[(int)NormalTelemetryField.SystemFaultFlags];
        var rawInterlocks = (uint)packet[(int)NormalTelemetryField.InterlockFlags];
        var rawRequiredInterlocks = (uint)packet[(int)NormalTelemetryField.RequiredInterlockFlags];
        _faults = new SystemFaults(rawFaults, null, TranslateFaults(rawFaults), AvailableFaults);
        _interlocks = new SystemInterlocks(
            rawInterlocks,
            rawRequiredInterlocks,
            SystemInterlockTranslator.Translate(rawInterlocks),
            SystemInterlockTranslator.AvailablePhysicalInterlocks,
            SystemInterlockTranslator.Translate(rawRequiredInterlocks));
        _ringLedState = (RingLedState)(int)packet[(int)NormalTelemetryField.RingLedState];
        _baseLedState = (BaseLedState)(int)packet[(int)NormalTelemetryField.BaseLedState];
        _collimatorId1 = packet[(int)NormalTelemetryField.Collimator1];
        _collimatorId2 = packet[(int)NormalTelemetryField.Collimator2];
        _collimatorSerial = ((ulong)_collimatorId2 << 32) | _collimatorId1;
        _buttonsState = packet[(int)NormalTelemetryField.Buttons];
        _currentOperationalPoint = packet[(int)NormalTelemetryField.CurrentPoint];
        _totalOperationalPoints = packet[(int)NormalTelemetryField.TotalPoints];
        _internalTimerState = packet[(int)NormalTelemetryField.InternalTimerState];
        _primaryTimerValue = packet[(int)NormalTelemetryField.InternalTimerValue];
        _timer1State = packet[(int)NormalTelemetryField.Timer1State];
        _secondaryTimer1Value = packet[(int)NormalTelemetryField.Timer1Value];
        _timer2State = packet[(int)NormalTelemetryField.Timer2State];
        _secondaryTimer2Value = packet[(int)NormalTelemetryField.Timer2Value];
        _runtimeCounterHvps = packet[(int)NormalTelemetryField.HvpsRuntime];
        _hvps = new HvpsTelemetryStatus(
            RawStatusFlags: packet[(int)NormalTelemetryField.HvpsStatusFlags],
            RawIoFlags: packet[(int)NormalTelemetryField.HvpsIO],
            RawErrorFlags: null);
        _kvFeedback = packet[(int)NormalTelemetryField.KvFeedback];
        _emissionCurrent = packet[(int)NormalTelemetryField.MaFeedback];
        _heaterCurrentSetpoint = packet[(int)NormalTelemetryField.FilamentSetpoint];
        _heaterCurrentFeedback = packet[(int)NormalTelemetryField.FilamentFeedback];
        _gridSetpoint = packet[(int)NormalTelemetryField.GridSetpoint];
        _gridVoltage = packet[(int)NormalTelemetryField.GridFeedback];
        _xCoilCurrent = packet[(int)NormalTelemetryField.XCoilCurrent];
        _yCoilCurrent = packet[(int)NormalTelemetryField.YCoilCurrent];
        _focusCurrent = packet[(int)NormalTelemetryField.FocusCoilCurrent];
        _ionPumpFeedback = packet[(int)NormalTelemetryField.IonPumpFeedback];
        _waterPressure = packet[(int)NormalTelemetryField.WaterPressure];
        _waterFlowRate = packet[(int)NormalTelemetryField.WaterFlow];
        _waterTemperature = packet[(int)NormalTelemetryField.WaterTemp];
        _heatSinkTemperature = packet[(int)NormalTelemetryField.HeatsinkTemp];
        _peltierTemperature = packet[(int)NormalTelemetryField.PeltierTemp];
        _cabinetTemperature = packet[(int)NormalTelemetryField.CabinetTemp];
        _mag1 = new TelemetryVector3(
            packet[(int)NormalTelemetryField.Mag1X],
            packet[(int)NormalTelemetryField.Mag1Y],
            packet[(int)NormalTelemetryField.Mag1Z]);
        _mag2 = new TelemetryVector3(
            packet[(int)NormalTelemetryField.Mag2X],
            packet[(int)NormalTelemetryField.Mag2Y],
            packet[(int)NormalTelemetryField.Mag2Z]);
        _kvSetpoint = packet[(int)NormalTelemetryField.KvSetpoint];
        _emissionCurrentLimit = packet[(int)NormalTelemetryField.EmissionCurrentLimit];
        _hvpsPowerSetpoint = packet[(int)NormalTelemetryField.HvpsPowerSetpoint];
    }

    internal SystemNormalTelemetry Snapshot() => new()
    {
        ControlBoardState = _controlBoardState,
        SystemRuntime = _systemRuntime,
        Faults = _faults,
        Interlocks = _interlocks,
        RingLedState = _ringLedState,
        BaseLedState = _baseLedState,
        CollimatorId1 = _collimatorId1,
        CollimatorId2 = _collimatorId2,
        CollimatorSerial = _collimatorSerial,
        ButtonsState = _buttonsState,
        CurrentOperationalPoint = _currentOperationalPoint,
        TotalOperationalPoints = _totalOperationalPoints,
        InternalTimerState = _internalTimerState,
        PrimaryTimerValue = _primaryTimerValue,
        Timer1State = _timer1State,
        SecondaryTimer1Value = _secondaryTimer1Value,
        Timer2State = _timer2State,
        SecondaryTimer2Value = _secondaryTimer2Value,
        RuntimeCounterHVPS = _runtimeCounterHvps,
        Hvps = _hvps,
        KvSetpoint = _kvSetpoint,
        KvFeedback = _kvFeedback,
        EmissionCurrent = _emissionCurrent,
        HeaterCurrentSetpoint = _heaterCurrentSetpoint,
        HeaterCurrentFeedback = _heaterCurrentFeedback,
        EmissionCurrentLimit = _emissionCurrentLimit,
        HvpsPowerSetpoint = _hvpsPowerSetpoint,
        GridSetpoint = _gridSetpoint,
        GridVoltage = _gridVoltage,
        XCoilCurrent = _xCoilCurrent,
        YCoilCurrent = _yCoilCurrent,
        FocusCurrent = _focusCurrent,
        IonPumpFeedback = _ionPumpFeedback,
        WaterPressure = _waterPressure,
        WaterFlowRate = _waterFlowRate,
        WaterTemperature = _waterTemperature,
        HeatSinkTemperature = _heatSinkTemperature,
        PeltierTemperature = _peltierTemperature,
        CabinetTemperature = _cabinetTemperature,
        Mag1 = _mag1,
        Mag2 = _mag2,
    };

    private static ulong TranslateFaults(uint rawFlags)
    {
        ulong active = 0;
        MapFault(rawFlags, 1, SystemFault.InterlockFault, ref active);
        MapFault(rawFlags, 2, SystemFault.HvpsReportedFault, ref active);
        MapFault(rawFlags, 3, SystemFault.VoltageFault, ref active);
        MapFault(rawFlags, 4, SystemFault.CurrentFault, ref active);
        MapFault(rawFlags, 5, SystemFault.FilamentFault, ref active);
        MapFault(rawFlags, 6, SystemFault.GridFault, ref active);
        MapFault(rawFlags, 7, SystemFault.CoilFault, ref active);
        MapFault(rawFlags, 8, SystemFault.IonPumpFault, ref active);
        MapFault(rawFlags, 9, SystemFault.IonRepellerFault, ref active);
        MapFault(rawFlags, 10, SystemFault.PeltierFault, ref active);
        MapFault(rawFlags, 11, SystemFault.HeatsinkFault, ref active);
        MapFault(rawFlags, 12, SystemFault.CoolantFault, ref active);
        MapFault(rawFlags, 13, SystemFault.InternalSupplyVoltageFault, ref active);
        MapFault(rawFlags, 14, SystemFault.PcCommFault, ref active);
        MapFault(rawFlags, 15, SystemFault.HvpsCommFault, ref active);
        MapFault(rawFlags, 16, SystemFault.TimerCommFault, ref active);
        MapFault(rawFlags, 17, SystemFault.HeadBoardCommFault, ref active);
        MapFault(rawFlags, 18, SystemFault.LedBoardCommFault, ref active);
        MapFault(rawFlags, 19, SystemFault.PeltierControllerCommFault, ref active);
        MapFault(rawFlags, 20, SystemFault.QcWellCommFault, ref active);
        MapFault(rawFlags, 21, SystemFault.AdcBusCommFault, ref active);
        MapFault(rawFlags, 22, SystemFault.MemoryFault, ref active);
        MapFault(rawFlags, 23, SystemFault.InvalidConfigFault, ref active);
        return active;
    }


    private static void MapFault(uint raw, int rawBit, SystemFault fault, ref ulong active)
    {
        if ((raw & (1u << rawBit)) != 0)
            active |= 1UL << (int)fault;
    }

}

internal sealed class CalibrationTelemetryState
{
    private const ulong AvailableFaults =
        (1UL << (int)SystemFault.InterlockFault)
        | (1UL << (int)SystemFault.HvpsReportedFault)
        | (1UL << (int)SystemFault.VoltageFault)
        | (1UL << (int)SystemFault.CurrentFault)
        | (1UL << (int)SystemFault.FilamentFault)
        | (1UL << (int)SystemFault.GridFault)
        | (1UL << (int)SystemFault.CoilFault)
        | (1UL << (int)SystemFault.IonPumpFault)
        | (1UL << (int)SystemFault.IonRepellerFault)
        | (1UL << (int)SystemFault.PeltierFault)
        | (1UL << (int)SystemFault.HeatsinkFault)
        | (1UL << (int)SystemFault.CoolantFault)
        | (1UL << (int)SystemFault.InternalSupplyVoltageFault)
        | (1UL << (int)SystemFault.PcCommFault)
        | (1UL << (int)SystemFault.HvpsCommFault)
        | (1UL << (int)SystemFault.TimerCommFault)
        | (1UL << (int)SystemFault.HeadBoardCommFault)
        | (1UL << (int)SystemFault.LedBoardCommFault)
        | (1UL << (int)SystemFault.PeltierControllerCommFault)
        | (1UL << (int)SystemFault.QcWellCommFault)
        | (1UL << (int)SystemFault.AdcBusCommFault)
        | (1UL << (int)SystemFault.MemoryFault)
        | (1UL << (int)SystemFault.InvalidConfigFault);


    private GcbStateNew _controlBoardState;
    private int _systemRuntime;
    private SystemFaults _faults;
    private SystemInterlocks _interlocks;
    private int _buttonsState;
    private int _currentOperationalPoint;
    private int _totalOperationalPoints;
    private int _internalTimerState;
    private float _primaryTimerValue;
    private int _timer1State;
    private float _secondaryTimer1Value;
    private int _timer2State;
    private float _secondaryTimer2Value;
    private int _runtimeCounterHvps;
    private HvpsTelemetryStatus _hvps;
    private float _kvFeedback;
    private float _emissionCurrent;
    private float _heaterCurrentSetpoint;
    private float _heaterCurrentFeedback;
    private float _gridVoltage;
    private float _xCoilCurrent;
    private float _yCoilCurrent;
    private float _focusCurrent;
    private float _ionPumpFeedback;
    private float _waterPressure;
    private float _waterFlowRate;
    private float _waterTemperature;
    private float _heatSinkTemperature;
    private float _peltierTemperature;
    private float _cabinetTemperature;

    internal uint Runtime => unchecked((uint)_systemRuntime);

    internal void Update(UdpPacket packet)
    {
        if (packet.PacketType != (uint)GCBPacketType.TelemetryResponse || packet.PayloadLength != 47u)
            throw new ArgumentException("Invalid calibration telemetry packet");

        _controlBoardState = (GcbStateNew)(int)packet[1];
        _currentOperationalPoint = packet[2];
        _totalOperationalPoints = packet[3];
        _internalTimerState = packet[4];
        _timer1State = packet[5];
        _timer2State = packet[6];
        _systemRuntime = packet[7];
        _runtimeCounterHvps = packet[8];
        _buttonsState = packet[11];
        var rawFaults = (uint)packet[12];
        var rawCommunicationFaults = (uint)packet[13];
        var rawInterlocks = (uint)packet[14];
        var rawRequiredInterlocks = (uint)packet[10];
        _faults = new SystemFaults(
            rawFaults,
            rawCommunicationFaults,
            TranslateFaults(rawFaults),
            AvailableFaults);
        _interlocks = new SystemInterlocks(
            rawInterlocks,
            rawRequiredInterlocks,
            SystemInterlockTranslator.Translate(rawInterlocks),
            SystemInterlockTranslator.AvailablePhysicalInterlocks,
            SystemInterlockTranslator.Translate(rawRequiredInterlocks));
        _hvps = new HvpsTelemetryStatus(
            RawStatusFlags: packet[16],
            RawIoFlags: packet[15],
            RawErrorFlags: packet[17]);
        _primaryTimerValue = packet[18];
        _secondaryTimer1Value = packet[19];
        _secondaryTimer2Value = packet[20];
        _kvFeedback = packet[21];
        _emissionCurrent = packet[22];
        _gridVoltage = packet[23];
        _heaterCurrentFeedback = packet[24];
        _heaterCurrentSetpoint = packet[25];
        _xCoilCurrent = packet[29];
        _yCoilCurrent = packet[30];
        _focusCurrent = packet[33];
        _ionPumpFeedback = packet[35];
        _waterPressure = packet[38];
        _waterFlowRate = packet[39];
        _waterTemperature = packet[40];
        _heatSinkTemperature = packet[41];
        _peltierTemperature = packet[42];
        _cabinetTemperature = packet[43];
    }

    internal SystemCalibrationTelemetry Snapshot() => new()
    {
        ControlBoardState = _controlBoardState,
        SystemRuntime = _systemRuntime,
        Faults = _faults,
        Interlocks = _interlocks,
        ButtonsState = _buttonsState,
        CurrentOperationalPoint = _currentOperationalPoint,
        TotalOperationalPoints = _totalOperationalPoints,
        InternalTimerState = _internalTimerState,
        PrimaryTimerValue = _primaryTimerValue,
        Timer1State = _timer1State,
        SecondaryTimer1Value = _secondaryTimer1Value,
        Timer2State = _timer2State,
        SecondaryTimer2Value = _secondaryTimer2Value,
        RuntimeCounterHVPS = _runtimeCounterHvps,
        Hvps = _hvps,
        KvFeedback = _kvFeedback,
        EmissionCurrent = _emissionCurrent,
        HeaterCurrentSetpoint = _heaterCurrentSetpoint,
        HeaterCurrentFeedback = _heaterCurrentFeedback,
        GridVoltage = _gridVoltage,
        XCoilCurrent = _xCoilCurrent,
        YCoilCurrent = _yCoilCurrent,
        FocusCurrent = _focusCurrent,
        IonPumpFeedback = _ionPumpFeedback,
        WaterPressure = _waterPressure,
        WaterFlowRate = _waterFlowRate,
        WaterTemperature = _waterTemperature,
        HeatSinkTemperature = _heatSinkTemperature,
        PeltierTemperature = _peltierTemperature,
        CabinetTemperature = _cabinetTemperature,
    };

    private static ulong TranslateFaults(uint rawFlags)
    {
        ulong active = 0;
        MapFault(rawFlags, 1, SystemFault.InterlockFault, ref active);
        MapFault(rawFlags, 2, SystemFault.HvpsReportedFault, ref active);
        MapFault(rawFlags, 3, SystemFault.VoltageFault, ref active);
        MapFault(rawFlags, 4, SystemFault.CurrentFault, ref active);
        MapFault(rawFlags, 5, SystemFault.FilamentFault, ref active);
        MapFault(rawFlags, 6, SystemFault.GridFault, ref active);
        MapFault(rawFlags, 7, SystemFault.CoilFault, ref active);
        MapFault(rawFlags, 8, SystemFault.IonPumpFault, ref active);
        MapFault(rawFlags, 9, SystemFault.IonRepellerFault, ref active);
        MapFault(rawFlags, 10, SystemFault.PeltierFault, ref active);
        MapFault(rawFlags, 11, SystemFault.HeatsinkFault, ref active);
        MapFault(rawFlags, 12, SystemFault.CoolantFault, ref active);
        MapFault(rawFlags, 13, SystemFault.InternalSupplyVoltageFault, ref active);
        MapFault(rawFlags, 14, SystemFault.PcCommFault, ref active);
        MapFault(rawFlags, 15, SystemFault.HvpsCommFault, ref active);
        MapFault(rawFlags, 16, SystemFault.TimerCommFault, ref active);
        MapFault(rawFlags, 17, SystemFault.HeadBoardCommFault, ref active);
        MapFault(rawFlags, 18, SystemFault.LedBoardCommFault, ref active);
        MapFault(rawFlags, 19, SystemFault.PeltierControllerCommFault, ref active);
        MapFault(rawFlags, 20, SystemFault.QcWellCommFault, ref active);
        MapFault(rawFlags, 21, SystemFault.AdcBusCommFault, ref active);
        MapFault(rawFlags, 22, SystemFault.MemoryFault, ref active);
        MapFault(rawFlags, 23, SystemFault.InvalidConfigFault, ref active);
        return active;
    }


    private static void MapFault(uint raw, int rawBit, SystemFault fault, ref ulong active)
    {
        if ((raw & (1u << rawBit)) != 0)
            active |= 1UL << (int)fault;
    }

}

internal static class TelemetryFormatter
{
    internal static string Format(ISystemTelemetry telemetry, bool verticallyAligned)
    {
        var properties = telemetry.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var nameWidth = 0;
        if (verticallyAligned)
        {
            foreach (var property in properties)
                nameWidth = Math.Max(nameWidth, property.Name.Length);
        }

        var builder = new StringBuilder();
        foreach (var property in properties)
        {
            var name = verticallyAligned ? property.Name.PadRight(nameWidth) : property.Name;
            builder.Append(name).Append(": ");
            AppendValue(builder, property.GetValue(telemetry));
            builder.AppendLine();
        }

        return builder.ToString();
    }

    private static void AppendValue(StringBuilder builder, object? value)
    {
        switch (value)
        {
            case null:
                builder.Append("N/A");
                break;
            case float number:
                builder.Append(number.ToString("F3", CultureInfo.InvariantCulture));
                break;
            case double number:
                builder.Append(number.ToString("F3", CultureInfo.InvariantCulture));
                break;
            case int number:
                builder.Append("0x").Append(number.ToString("X", CultureInfo.InvariantCulture));
                break;
            case uint number:
                builder.Append("0x").Append(number.ToString("X", CultureInfo.InvariantCulture));
                break;
            case ulong number:
                builder.Append("0x").Append(number.ToString("X", CultureInfo.InvariantCulture));
                break;
            default:
                builder.Append(value);
                break;
        }
    }
}
