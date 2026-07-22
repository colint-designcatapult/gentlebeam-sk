using System.Collections.Generic;
using Xcc.Core.Enums;

namespace Xcc.Core.Domain.GryphonBoard
{
    public sealed record FaultEntry(
        SystemFault FaultType,
        uint FormatHash,
        GcbStateNew CapturedState,
        uint CapturedRuntime,
        string Format,
        string Message)
    {
        public override string ToString() =>
            $"{FaultType}: {Message} (state {CapturedState}, runtime {CapturedRuntime})";
    }

    public sealed record FaultUpdate(
        uint ClearEpoch,
        uint EntryIndex,
        uint ActiveCount,
        FaultEntry? Entry);

    public sealed record FaultSnapshot(
        uint ClearEpoch,
        IReadOnlyList<FaultEntry> Entries);
}
