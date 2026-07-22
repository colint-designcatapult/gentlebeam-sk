using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public class SystemTelemetryChangedEvent : PubSubEvent<ISystemTelemetry?> { }
    public class FaultsChangedEvent : PubSubEvent<IReadOnlyList<FaultEntry>> { }

    public class GCBDataStore(IEventAggregator eventAggregator) : BindableBase, IGCBDataStore
    {
        private const uint MaximumFaults = 4;

        private readonly object _faultLock = new();
        private readonly FaultEntry?[] _faultSlots = new FaultEntry?[MaximumFaults];
        private ISystemTelemetry? _systemTelemetry;
        private uint? _faultEpoch;
        private uint _activeFaultCount;
        private IReadOnlyList<FaultEntry> _activeFaults = Array.Empty<FaultEntry>();

        public IEventAggregator EventAggregator { get; } = eventAggregator;

        public ISystemTelemetry? SystemTelemetry
        {
            get => _systemTelemetry;
            set
            {
                if (SetProperty(ref _systemTelemetry, value))
                {
                    EventAggregator.GetEvent<SystemTelemetryChangedEvent>().Publish(_systemTelemetry);
                }
            }
        }

        public IReadOnlyList<FaultEntry> ActiveFaults
        {
            get
            {
                lock (_faultLock)
                {
                    return _activeFaults;
                }
            }
        }

        public void ApplyFaultUpdate(FaultUpdate update)
        {
            ArgumentNullException.ThrowIfNull(update);
            ValidateUpdate(update);

            IReadOnlyList<FaultEntry>? publishedSnapshot = null;
            lock (_faultLock)
            {
                int epochOrder = _faultEpoch is uint currentEpoch
                    ? CompareEpoch(update.ClearEpoch, currentEpoch)
                    : 1;
                if (epochOrder < 0)
                {
                    return;
                }

                if (epochOrder == 0 && update.ActiveCount < _activeFaultCount)
                {
                    throw new ArgumentException("Active fault count cannot decrease within an epoch.", nameof(update));
                }

                if (epochOrder == 0 && update.Entry is not null)
                {
                    RejectDuplicateFormatAtAnotherIndex(update.EntryIndex, update.Entry, nameof(update));
                }

                bool changed = false;
                if (epochOrder > 0)
                {
                    Array.Clear(_faultSlots);
                    _activeFaultCount = 0;
                    _faultEpoch = update.ClearEpoch;
                    changed = true;
                }

                if (update.ActiveCount == 0)
                {
                    if (_activeFaultCount != 0)
                    {
                        Array.Clear(_faultSlots);
                        _activeFaultCount = 0;
                        changed = true;
                    }
                }
                else
                {
                    if (_activeFaultCount != update.ActiveCount)
                    {
                        _activeFaultCount = update.ActiveCount;
                        changed = true;
                    }

                    int index = checked((int)update.EntryIndex);
                    if (!Equals(_faultSlots[index], update.Entry))
                    {
                        _faultSlots[index] = update.Entry;
                        changed = true;
                    }
                }

                if (changed)
                {
                    publishedSnapshot = RefreshSnapshot();
                }
            }

            if (publishedSnapshot is not null)
            {
                EventAggregator.GetEvent<FaultsChangedEvent>().Publish(publishedSnapshot);
            }
        }

        public void ReplaceFaults(FaultSnapshot snapshot)
        {
            ArgumentNullException.ThrowIfNull(snapshot);
            ValidateSnapshot(snapshot);

            IReadOnlyList<FaultEntry>? publishedSnapshot = null;
            lock (_faultLock)
            {
                int epochOrder = _faultEpoch is uint currentEpoch
                    ? CompareEpoch(snapshot.ClearEpoch, currentEpoch)
                    : 1;
                if (epochOrder < 0)
                {
                    return;
                }
                if (epochOrder == 0 && snapshot.Entries.Count < _activeFaultCount)
                {
                    throw new ArgumentException("Active fault count cannot decrease within an epoch.", nameof(snapshot));
                }

                bool changed = epochOrder > 0 || _activeFaultCount != (uint)snapshot.Entries.Count;
                for (int index = 0; index < _faultSlots.Length; index++)
                {
                    FaultEntry? replacement = index < snapshot.Entries.Count
                        ? snapshot.Entries[index]
                        : null;
                    if (!Equals(_faultSlots[index], replacement))
                    {
                        changed = true;
                    }
                }

                if (changed)
                {
                    _faultEpoch = snapshot.ClearEpoch;
                    _activeFaultCount = (uint)snapshot.Entries.Count;
                    Array.Clear(_faultSlots);
                    for (int index = 0; index < snapshot.Entries.Count; index++)
                    {
                        _faultSlots[index] = snapshot.Entries[index];
                    }
                    publishedSnapshot = RefreshSnapshot();
                }
            }

            if (publishedSnapshot is not null)
            {
                EventAggregator.GetEvent<FaultsChangedEvent>().Publish(publishedSnapshot);
            }
        }

        private static int CompareEpoch(uint candidate, uint current) =>
            unchecked((int)(candidate - current));

        private static void ValidateUpdate(FaultUpdate update)
        {
            if (update.ActiveCount > MaximumFaults)
            {
                throw new ArgumentOutOfRangeException(nameof(update), "Active fault count cannot exceed four.");
            }
            if (update.ActiveCount == 0)
            {
                if (update.Entry is not null)
                {
                    throw new ArgumentException("A clear update cannot contain a fault entry.", nameof(update));
                }
                return;
            }
            if (update.Entry is null || update.EntryIndex >= update.ActiveCount)
            {
                throw new ArgumentException("A fault update must contain an entry at an index below the active count.", nameof(update));
            }
        }

        private static void ValidateSnapshot(FaultSnapshot snapshot)
        {
            if (snapshot.Entries.Count > MaximumFaults)
            {
                throw new ArgumentOutOfRangeException(nameof(snapshot), "A fault snapshot cannot contain more than four entries.");
            }

            for (int index = 0; index < snapshot.Entries.Count; index++)
            {
                FaultEntry entry = snapshot.Entries[index]
                    ?? throw new ArgumentException("A fault snapshot cannot contain null entries.", nameof(snapshot));
                for (int otherIndex = 0; otherIndex < index; otherIndex++)
                {
                    FaultEntry other = snapshot.Entries[otherIndex];
                    if (other.FormatHash == entry.FormatHash &&
                        string.Equals(other.Format, entry.Format, StringComparison.Ordinal))
                    {
                        throw new ArgumentException("A fault snapshot cannot repeat the same format at another index.", nameof(snapshot));
                    }
                }
            }
        }

        private void RejectDuplicateFormatAtAnotherIndex(uint entryIndex, FaultEntry entry, string parameterName)
        {
            for (int index = 0; index < _faultSlots.Length; index++)
            {
                FaultEntry? other = _faultSlots[index];
                if ((uint)index != entryIndex &&
                    other is not null &&
                    other.FormatHash == entry.FormatHash &&
                    string.Equals(other.Format, entry.Format, StringComparison.Ordinal))
                {
                    throw new ArgumentException("The same fault format cannot appear at another index.", parameterName);
                }
            }
        }

        private IReadOnlyList<FaultEntry> RefreshSnapshot()
        {
            var entries = new List<FaultEntry>(_faultSlots.Length);
            foreach (FaultEntry? entry in _faultSlots)
            {
                if (entry is not null)
                {
                    entries.Add(entry);
                }
            }

            _activeFaults = entries.AsReadOnly();
            return _activeFaults;
        }
    }
}
