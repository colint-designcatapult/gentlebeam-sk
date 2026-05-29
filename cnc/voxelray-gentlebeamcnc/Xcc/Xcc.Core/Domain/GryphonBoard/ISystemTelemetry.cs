using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard;

public interface ISystemTelemetry
{
    GcbStateNew ControlBoardState { get; }
    int SystemRuntime { get; }
    int FaultFlags { get; }
    uint InterlockFlags { get; }
    RingLedState RingLedState { get; }
    BaseLedState BaseLedState { get; }
    uint CollimatorId1 { get; }
    uint CollimatorId2 { get; }
    ulong CollimatorSerial { get; set; }
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
    uint HvpsIOStatus { get; }
    uint HvpsFlagStatus { get; }
    float KvSetpoint { get; }
    float KvFeedback { get; }
    float EmissionCurrent { get; }
    float HeaterCurrentSetpoint { get; }
    float HeaterCurrentFeedback { get; }
    float EmissionCurrentLimit { get; }
    float HvpsPowerSetpoint { get; }
    float GridSetpoint { get; }
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
    float[] Mag1 { get; }
    float[] Mag2 { get; }
    uint Applicator { get; }
    float CabinetTemperature { get; }

    string ToString();
    bool IsFaultState();
    bool IsEmissionState();
    string GetVerticallyFormattedString();
}