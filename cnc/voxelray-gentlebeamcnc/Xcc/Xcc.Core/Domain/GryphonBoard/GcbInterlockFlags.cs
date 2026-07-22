using System;

namespace Xcc.Core.Domain.GryphonBoard;

[Flags]
public enum GcbInterlockFlags : uint
{
    DoorClosed = 1u << 0,
    SpareInterlock2 = 1u << 1,
    BaseEStopReleased = 1u << 2,
    RemoteEStopReleased = 1u << 3,
    Kuka1Ready = 1u << 4,
    Kuka2Ready = 1u << 5,
    WaterLevelOk = 1u << 6,
    IonPumpOk = 1u << 7,
    Timer1Ready = 1u << 8,
    Timer2Ready = 1u << 9,
    HvpsReady = 1u << 10,
    CoolerReady = 1u << 11,
    WaterTemperatureOk = 1u << 12,
    WatchdogReady = 1u << 13,
    McuFaultClear = 1u << 14,
    SpareInterlock1 = 1u << 15,
    MasterFaultClear = 1u << 16,
    RemoteKeyOn = 1u << 18,
    BaseKeyOn = 1u << 19,
    All = DoorClosed
        | SpareInterlock2
        | BaseEStopReleased
        | RemoteEStopReleased
        | Kuka1Ready
        | Kuka2Ready
        | WaterLevelOk
        | IonPumpOk
        | Timer1Ready
        | Timer2Ready
        | HvpsReady
        | CoolerReady
        | WaterTemperatureOk
        | WatchdogReady
        | McuFaultClear
        | SpareInterlock1
        | MasterFaultClear
        | RemoteKeyOn
        | BaseKeyOn,
}
