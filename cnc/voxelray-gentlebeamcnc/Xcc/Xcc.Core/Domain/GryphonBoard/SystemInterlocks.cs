using System;
using System.ComponentModel.DataAnnotations;

namespace Xcc.Core.Domain.GryphonBoard;

public enum SystemInterlock
{
    [Display(Name = "Door closed")]
    DoorClosed = 0,
    [Display(Name = "Spare interlock 2")]
    SpareInterlock2 = 1,
    [Display(Name = "Base e-stop released")]
    BaseEStopReleased = 2,
    [Display(Name = "Remote e-stop released")]
    RemoteEStopReleased = 3,
    [Display(Name = "KUKA 1 ready")]
    Kuka1Ready = 4,
    [Display(Name = "KUKA 2 ready")]
    Kuka2Ready = 5,
    [Display(Name = "Water level OK")]
    WaterLevelOk = 6,
    [Display(Name = "Ion pump OK")]
    IonPumpOk = 7,
    [Display(Name = "Timer 1 ready")]
    Timer1Ready = 8,
    [Display(Name = "Timer 2 ready")]
    Timer2Ready = 9,
    [Display(Name = "HVPS ready")]
    HvpsReady = 10,
    [Display(Name = "Cooler ready")]
    CoolerReady = 11,
    [Display(Name = "Water temperature OK")]
    WaterTemperatureOk = 12,
    [Display(Name = "Watchdog ready")]
    WatchdogReady = 13,
    [Display(Name = "MCU fault clear")]
    McuFaultClear = 14,
    [Display(Name = "Spare interlock 1")]
    SpareInterlock1 = 15,
    [Display(Name = "Buffered master fault clear")]
    MasterFaultClear = 16,
    [Display(Name = "Remote key on")]
    RemoteKeyOn = 18,
    [Display(Name = "Base key on")]
    BaseKeyOn = 19,
}

public static class SystemInterlockExtensions
{
    public static bool FeedsMasterFault(this SystemInterlock interlock) =>
        interlock is SystemInterlock.DoorClosed
            or SystemInterlock.SpareInterlock2
            or SystemInterlock.BaseEStopReleased
            or SystemInterlock.RemoteEStopReleased
            or SystemInterlock.Kuka1Ready
            or SystemInterlock.Kuka2Ready
            or SystemInterlock.WaterLevelOk
            or SystemInterlock.IonPumpOk
            or SystemInterlock.Timer1Ready
            or SystemInterlock.Timer2Ready
            or SystemInterlock.HvpsReady
            or SystemInterlock.CoolerReady
            or SystemInterlock.WaterTemperatureOk
            or SystemInterlock.WatchdogReady
            or SystemInterlock.SpareInterlock1;
}

public readonly struct SystemInterlocks : IEquatable<SystemInterlocks>
{
    private readonly ulong _activeMask;
    private readonly ulong _availableMask;
    private readonly ulong _requiredMask;

    public SystemInterlocks(
        uint rawFlags,
        uint rawRequiredFlags,
        ulong activeMask,
        ulong availableMask,
        ulong requiredMask)
    {
        RawFlags = rawFlags;
        RawRequiredFlags = rawRequiredFlags;
        _activeMask = activeMask;
        _availableMask = availableMask;
        _requiredMask = requiredMask;
    }

    public uint RawFlags { get; }
    public uint RawRequiredFlags { get; }
    public bool RequiredInterlocksReady =>
        (_availableMask & _requiredMask) == _requiredMask
        && (_activeMask & _requiredMask) == _requiredMask;

    public bool? DoorClosed => GetState(SystemInterlock.DoorClosed);
    public bool? SpareInterlock2 => GetState(SystemInterlock.SpareInterlock2);
    public bool? BaseEStopReleased => GetState(SystemInterlock.BaseEStopReleased);
    public bool? RemoteEStopReleased => GetState(SystemInterlock.RemoteEStopReleased);
    public bool? Kuka1Ready => GetState(SystemInterlock.Kuka1Ready);
    public bool? Kuka2Ready => GetState(SystemInterlock.Kuka2Ready);
    public bool? WaterLevelOk => GetState(SystemInterlock.WaterLevelOk);
    public bool? IonPumpOk => GetState(SystemInterlock.IonPumpOk);
    public bool? Timer1Ready => GetState(SystemInterlock.Timer1Ready);
    public bool? Timer2Ready => GetState(SystemInterlock.Timer2Ready);
    public bool? HvpsReady => GetState(SystemInterlock.HvpsReady);
    public bool? CoolerReady => GetState(SystemInterlock.CoolerReady);
    public bool? WaterTemperatureOk => GetState(SystemInterlock.WaterTemperatureOk);
    public bool? WatchdogReady => GetState(SystemInterlock.WatchdogReady);
    public bool? McuFaultClear => GetState(SystemInterlock.McuFaultClear);
    public bool? SpareInterlock1 => GetState(SystemInterlock.SpareInterlock1);
    public bool? MasterFaultClear => GetState(SystemInterlock.MasterFaultClear);
    public bool? RemoteKeyOn => GetState(SystemInterlock.RemoteKeyOn);
    public bool? BaseKeyOn => GetState(SystemInterlock.BaseKeyOn);

    public bool? GetState(SystemInterlock interlock)
    {
        var bit = (int)interlock;
        if ((uint)bit >= 64u)
            return null;

        var mask = 1UL << bit;
        return (_availableMask & mask) == 0 ? null : (_activeMask & mask) != 0;
    }

    public bool IsRequired(SystemInterlock interlock)
    {
        var bit = (int)interlock;
        return (uint)bit < 64u && (_requiredMask & (1UL << bit)) != 0;
    }

    public bool Equals(SystemInterlocks other) =>
        RawFlags == other.RawFlags
        && RawRequiredFlags == other.RawRequiredFlags
        && _activeMask == other._activeMask
        && _availableMask == other._availableMask
        && _requiredMask == other._requiredMask;

    public override bool Equals(object? obj) => obj is SystemInterlocks other && Equals(other);
    public override int GetHashCode() =>
        HashCode.Combine(RawFlags, RawRequiredFlags, _activeMask, _availableMask, _requiredMask);
    public static bool operator ==(SystemInterlocks left, SystemInterlocks right) => left.Equals(right);
    public static bool operator !=(SystemInterlocks left, SystemInterlocks right) => !left.Equals(right);
}
