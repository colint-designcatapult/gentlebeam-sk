using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard;

public interface ISystemTelemetry
{
    FirmwareMode FirmwareMode { get; }
    GcbStateNew ControlBoardState { get; }
    int SystemRuntime { get; }
    SystemFaults Faults { get; }
    SystemInterlocks Interlocks { get; }
    RingLedState? RingLedState { get; }
    BaseLedState? BaseLedState { get; }
    uint? CollimatorId1 { get; }
    uint? CollimatorId2 { get; }
    ulong? CollimatorSerial { get; }
    int ButtonsState { get; }
    int CurrentOperationalPoint { get; }
    int TotalOperationalPoints { get; }
    int InternalTimerState { get; }
    float PrimaryTimerValue { get; }
    int Timer1State { get; }
    float SecondaryTimer1Value { get; }
    int Timer2State { get; }
    float SecondaryTimer2Value { get; }
    int RuntimeCounterHVPS { get; }
    HvpsTelemetryStatus Hvps { get; }
    float? KvSetpoint { get; }
    float KvFeedback { get; }
    float EmissionCurrent { get; }
    float HeaterCurrentSetpoint { get; }
    float HeaterCurrentFeedback { get; }
    float? EmissionCurrentLimit { get; }
    float? HvpsPowerSetpoint { get; }
    float? GridSetpoint { get; }
    float GridVoltage { get; }
    float XCoilCurrent { get; }
    float YCoilCurrent { get; }
    float FocusCurrent { get; }
    float IonPumpFeedback { get; }
    float WaterPressure { get; }
    float WaterFlowRate { get; }
    float WaterTemperature { get; }
    float HeatSinkTemperature { get; }
    float PeltierTemperature { get; }
    float CabinetTemperature { get; }
    TelemetryVector3? Mag1 { get; }
    TelemetryVector3? Mag2 { get; }

    bool IsFaultState();
    bool IsEmissionState();
    string GetVerticallyFormattedString();
}

public static class SystemTelemetryReadinessExtensions
{
    public static bool IsSystemReady(this ISystemTelemetry telemetry, bool applicatorIsReady) =>
        telemetry.Interlocks.MasterFaultClear == true
        && !telemetry.Faults.AnyActive
        && telemetry.Interlocks.RequiredInterlocksReady
        && applicatorIsReady;
}