using System;

namespace Xcc.Core.Domain.GryphonBoard;

public enum SystemInterlock
{
    DoorClosed,
    DriveSystemReady,
    BaseEStopReleased,
    RemoteEStopReleased,
    Kuka1Ready,
    Kuka2Ready,
    WaterLevelOk,
    IonPumpOk,
    Timer1Ready,
    Timer2Ready,
    HvpsReady,
    CoolerReady,
    HeadInterfaceBoardReady,
    WatchdogReady,
    RemoteKeyOn,
    CollimatorOn,
    TvmNotchEngaged,
}

public readonly struct SystemInterlocks : IEquatable<SystemInterlocks>
{
    private readonly ulong _activeMask;
    private readonly ulong _availableMask;

    public SystemInterlocks(uint rawFlags, uint? rawTvmFlags, ulong activeMask, ulong availableMask)
    {
        RawFlags = rawFlags;
        RawTvmFlags = rawTvmFlags;
        _activeMask = activeMask;
        _availableMask = availableMask;
    }

    public uint RawFlags { get; }
    public uint? RawTvmFlags { get; }

    public bool? DoorClosed => GetState(SystemInterlock.DoorClosed);
    public bool? DriveSystemReady => GetState(SystemInterlock.DriveSystemReady);
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
    public bool? HeadInterfaceBoardReady => GetState(SystemInterlock.HeadInterfaceBoardReady);
    public bool? WatchdogReady => GetState(SystemInterlock.WatchdogReady);
    public bool? RemoteKeyOn => GetState(SystemInterlock.RemoteKeyOn);
    public bool? CollimatorOn => GetState(SystemInterlock.CollimatorOn);
    public bool? TvmNotchEngaged => GetState(SystemInterlock.TvmNotchEngaged);

    public bool? GetState(SystemInterlock interlock)
    {
        var bit = (int)interlock;
        if ((uint)bit >= 64u)
            return null;

        var mask = 1UL << bit;
        return (_availableMask & mask) == 0 ? null : (_activeMask & mask) != 0;
    }

    public bool Equals(SystemInterlocks other) =>
        RawFlags == other.RawFlags
        && RawTvmFlags == other.RawTvmFlags
        && _activeMask == other._activeMask
        && _availableMask == other._availableMask;

    public override bool Equals(object? obj) => obj is SystemInterlocks other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(RawFlags, RawTvmFlags, _activeMask, _availableMask);
    public static bool operator ==(SystemInterlocks left, SystemInterlocks right) => left.Equals(right);
    public static bool operator !=(SystemInterlocks left, SystemInterlocks right) => !left.Equals(right);
}
