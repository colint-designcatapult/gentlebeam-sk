using System.Globalization;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Ucsi.Storage;

internal sealed class RecordedSystemTelemetry : ISystemTelemetry
{
    private readonly IReadOnlyDictionary<string, object?> _values;

    public RecordedSystemTelemetry(IReadOnlyDictionary<string, object?> values)
    {
        _values = values;
        Faults = BuildFaults(values);
        Interlocks = BuildInterlocks(values);
        Hvps = new HvpsTelemetryStatus(
            Get<uint>("system.Hvps.RawStatusFlags"),
            Get<uint>("system.Hvps.RawIoFlags"),
            Get<uint?>("system.Hvps.RawErrorFlags"));
        Mag1 = BuildVector("system.Mag1");
        Mag2 = BuildVector("system.Mag2");
    }

    public FirmwareMode FirmwareMode => Get<FirmwareMode>("system.FirmwareMode");
    public GcbStateNew ControlBoardState => Get<GcbStateNew>("system.ControlBoardState");
    public int SystemRuntime => Get<int>("system.SystemRuntime");
    public SystemFaults Faults { get; }
    public SystemInterlocks Interlocks { get; }
    public RingLedState? RingLedState => Get<RingLedState?>("system.RingLedState");
    public BaseLedState? BaseLedState => Get<BaseLedState?>("system.BaseLedState");
    public uint? CollimatorId1 => Get<uint?>("system.CollimatorId1");
    public uint? CollimatorId2 => Get<uint?>("system.CollimatorId2");
    public ulong? CollimatorSerial => Get<ulong?>("system.CollimatorSerial");
    public int ButtonsState => Get<int>("system.ButtonsState");
    public int CurrentOperationalPoint => Get<int>("system.CurrentOperationalPoint");
    public int TotalOperationalPoints => Get<int>("system.TotalOperationalPoints");
    public int InternalTimerState => Get<int>("system.InternalTimerState");
    public float PrimaryTimerValue => Get<float>("system.PrimaryTimerValue");
    public int Timer1State => Get<int>("system.Timer1State");
    public float SecondaryTimer1Value => Get<float>("system.SecondaryTimer1Value");
    public int Timer2State => Get<int>("system.Timer2State");
    public float SecondaryTimer2Value => Get<float>("system.SecondaryTimer2Value");
    public int RuntimeCounterHVPS => Get<int>("system.RuntimeCounterHVPS");
    public HvpsTelemetryStatus Hvps { get; }
    public float? KvSetpoint => Get<float?>("system.KvSetpoint");
    public float KvFeedback => Get<float>("system.KvFeedback");
    public float EmissionCurrent => Get<float>("system.EmissionCurrent");
    public float HeaterCurrentSetpoint => Get<float>("system.HeaterCurrentSetpoint");
    public float HeaterCurrentFeedback => Get<float>("system.HeaterCurrentFeedback");
    public float? EmissionCurrentLimit => Get<float?>("system.EmissionCurrentLimit");
    public float? HvpsPowerSetpoint => Get<float?>("system.HvpsPowerSetpoint");
    public float? GridSetpoint => Get<float?>("system.GridSetpoint");
    public float GridVoltage => Get<float>("system.GridVoltage");
    public float XCoilCurrent => Get<float>("system.XCoilCurrent");
    public float YCoilCurrent => Get<float>("system.YCoilCurrent");
    public float FocusCurrent => Get<float>("system.FocusCurrent");
    public float IonPumpFeedback => Get<float>("system.IonPumpFeedback");
    public float WaterPressure => Get<float>("system.WaterPressure");
    public float WaterFlowRate => Get<float>("system.WaterFlowRate");
    public float WaterTemperature => Get<float>("system.WaterTemperature");
    public float HeatSinkTemperature => Get<float>("system.HeatSinkTemperature");
    public float PeltierTemperature => Get<float>("system.PeltierTemperature");
    public float CabinetTemperature => Get<float>("system.CabinetTemperature");
    public TelemetryVector3? Mag1 { get; }
    public TelemetryVector3? Mag2 { get; }

    public bool IsFaultState() => ControlBoardState is
        GcbStateNew.Fault or GcbStateNew.ColdFault or GcbStateNew.WarmupFault;

    public bool IsEmissionState() => ControlBoardState is
        GcbStateNew.Emission or GcbStateNew.Imaging;

    public string GetVerticallyFormattedString() => string.Join(
        Environment.NewLine,
        _values.Select(pair => $"{pair.Key}: {Convert.ToString(pair.Value, CultureInfo.InvariantCulture)}"));

    private T Get<T>(string id)
    {
        if (!_values.TryGetValue(id, out object? value) || value is null)
            return default!;
        if (value is T typed)
            return typed;

        Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        object converted = targetType.IsEnum
            ? Enum.ToObject(targetType, value)
            : Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        return (T)converted;
    }

    private TelemetryVector3? BuildVector(string prefix)
    {
        float? x = Get<float?>($"{prefix}.X");
        float? y = Get<float?>($"{prefix}.Y");
        float? z = Get<float?>($"{prefix}.Z");
        return x.HasValue && y.HasValue && z.HasValue
            ? new TelemetryVector3(x.Value, y.Value, z.Value)
            : null;
    }

    private static SystemFaults BuildFaults(IReadOnlyDictionary<string, object?> values)
    {
        ulong active = 0;
        ulong available = 0;
        foreach (SystemFault fault in Enum.GetValues<SystemFault>())
        {
            if (!values.TryGetValue($"system.Faults.{fault}", out object? value) || value is not bool state)
                continue;
            ulong mask = 1UL << (int)fault;
            available |= mask;
            if (state)
                active |= mask;
        }

        return new SystemFaults(
            Read<uint>(values, "system.Faults.RawFlags"),
            Read<uint?>(values, "system.Faults.RawCommunicationFlags"),
            active,
            available);
    }

    private static SystemInterlocks BuildInterlocks(IReadOnlyDictionary<string, object?> values)
    {
        ulong active = 0;
        ulong available = 0;
        ulong required = 0;
        foreach (SystemInterlock interlock in Enum.GetValues<SystemInterlock>())
        {
            if (values.TryGetValue($"system.Interlocks.{interlock}", out object? value) && value is bool state)
            {
                ulong mask = 1UL << (int)interlock;
                available |= mask;
                if (state)
                    active |= mask;
            }
            if (Read<bool>(values, $"system.Interlocks.{interlock}.Required"))
                required |= 1UL << (int)interlock;
        }

        return new SystemInterlocks(
            Read<uint>(values, "system.Interlocks.RawFlags"),
            Read<uint>(values, "system.Interlocks.RawRequiredFlags"),
            active,
            available,
            required);
    }

    private static T Read<T>(IReadOnlyDictionary<string, object?> values, string id)
    {
        if (!values.TryGetValue(id, out object? value) || value is null)
            return default!;
        return value is T typed ? typed : (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T), CultureInfo.InvariantCulture);
    }
}
