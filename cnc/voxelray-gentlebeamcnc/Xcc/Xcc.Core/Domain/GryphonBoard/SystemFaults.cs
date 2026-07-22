using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard;

public readonly struct SystemFaults : IEquatable<SystemFaults>
{
    private readonly ulong _activeMask;
    private readonly ulong _availableMask;

    public SystemFaults(uint rawFlags, uint? rawCommunicationFlags, ulong activeMask, ulong availableMask)
    {
        RawFlags = rawFlags;
        RawCommunicationFlags = rawCommunicationFlags;
        _activeMask = activeMask;
        _availableMask = availableMask;
    }

    public uint RawFlags { get; }
    public uint? RawCommunicationFlags { get; }
    public bool AnyActive => (_activeMask & _availableMask) != 0;

    public bool? GetState(SystemFault fault)
    {
        var bit = (int)fault;
        if ((uint)bit >= 64u)
            return null;

        var mask = 1UL << bit;
        return (_availableMask & mask) == 0 ? null : (_activeMask & mask) != 0;
    }

    public override string ToString()
    {
        if (!AnyActive)
            return "No active faults";

        var builder = new StringBuilder();
        foreach (var fault in Enum.GetValues<SystemFault>())
        {
            if (GetState(fault) != true)
                continue;

            if (builder.Length != 0)
                builder.Append(", ");

            builder.Append(GetDisplayName(fault));
        }

        return builder.ToString();
    }

    public bool Equals(SystemFaults other) =>
        RawFlags == other.RawFlags
        && RawCommunicationFlags == other.RawCommunicationFlags
        && _activeMask == other._activeMask
        && _availableMask == other._availableMask;

    public override bool Equals(object? obj) => obj is SystemFaults other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(RawFlags, RawCommunicationFlags, _activeMask, _availableMask);
    public static bool operator ==(SystemFaults left, SystemFaults right) => left.Equals(right);
    public static bool operator !=(SystemFaults left, SystemFaults right) => !left.Equals(right);

    private static string GetDisplayName(SystemFault fault)
    {
        var member = typeof(SystemFault).GetMember(fault.ToString())[0];
        return member.GetCustomAttribute<DisplayAttribute>()?.Name ?? fault.ToString();
    }
}
