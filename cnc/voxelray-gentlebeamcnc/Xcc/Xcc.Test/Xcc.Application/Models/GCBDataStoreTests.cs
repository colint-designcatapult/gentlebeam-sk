using Prism.Events;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Application.Models;

internal class GCBDataStoreTests
{
    [Test]
    public void ApplyFaultUpdate_MergesOutOfOrderDeduplicatesAndClears()
    {
        var events = new List<IReadOnlyList<FaultEntry>>();
        var aggregator = new EventAggregator();
        aggregator.GetEvent<FaultsChangedEvent>().Subscribe(events.Add);
        var store = new GCBDataStore(aggregator);
        FaultEntry first = Entry(1, "First fault.");
        FaultEntry second = Entry(2, "Second fault.");

        store.ApplyFaultUpdate(new FaultUpdate(10, 1, 2, second));
        store.ApplyFaultUpdate(new FaultUpdate(10, 1, 2, second));
        store.ApplyFaultUpdate(new FaultUpdate(10, 0, 2, first));

        Assert.That(store.ActiveFaults, Is.EqualTo(new[] { first, second }));
        Assert.That(events, Has.Count.EqualTo(2));

        store.ApplyFaultUpdate(new FaultUpdate(11, 0, 0, null));

        Assert.That(store.ActiveFaults, Is.Empty);
        Assert.That(events, Has.Count.EqualTo(3));
    }

    [Test]
    public void ApplyFaultUpdate_UsesSerialEpochOrderingIncludingWrap()
    {
        var store = new GCBDataStore(new EventAggregator());
        FaultEntry original = Entry(1, "Original fault.");
        FaultEntry stale = Entry(2, "Stale fault.");
        FaultEntry newer = Entry(3, "Newer fault.");
        FaultEntry wrapped = Entry(4, "Wrapped fault.");

        store.ApplyFaultUpdate(new FaultUpdate(10, 0, 1, original));
        store.ApplyFaultUpdate(new FaultUpdate(9, 0, 1, stale));
        Assert.That(store.ActiveFaults, Is.EqualTo(new[] { original }));

        store.ApplyFaultUpdate(new FaultUpdate(11, 0, 1, newer));
        Assert.That(store.ActiveFaults, Is.EqualTo(new[] { newer }));

        var wrappedStore = new GCBDataStore(new EventAggregator());
        wrappedStore.ApplyFaultUpdate(new FaultUpdate(uint.MaxValue, 0, 1, original));
        wrappedStore.ApplyFaultUpdate(new FaultUpdate(0, 0, 1, wrapped));
        Assert.That(wrappedStore.ActiveFaults, Is.EqualTo(new[] { wrapped }));
    }

    [Test]
    public void ApplyFaultUpdate_AllowsHashCollisionButRejectsDuplicateFormatAtAnotherIndex()
    {
        var store = new GCBDataStore(new EventAggregator());
        FaultEntry first = Entry(0x12345678, "Collision text A.");
        FaultEntry collision = Entry(0x12345678, "Collision text B.");
        FaultEntry duplicate = first with { Message = "Different arguments" };

        store.ApplyFaultUpdate(new FaultUpdate(1, 0, 1, first));
        store.ApplyFaultUpdate(new FaultUpdate(1, 1, 2, collision));

        Assert.That(store.ActiveFaults, Is.EqualTo(new[] { first, collision }));
        Assert.That(
            () => store.ApplyFaultUpdate(new FaultUpdate(1, 1, 2, duplicate)),
            Throws.TypeOf<ArgumentException>());
        Assert.That(store.ActiveFaults, Is.EqualTo(new[] { first, collision }));
    }

    [Test]
    public void ApplyFaultUpdate_EnforcesFourEntryAndMonotonicCountLimits()
    {
        var store = new GCBDataStore(new EventAggregator());
        var entries = Enumerable.Range(0, 4)
            .Select(index => Entry((uint)index, $"Fault {index}."))
            .ToArray();

        for (uint index = 0; index < entries.Length; index++)
        {
            store.ApplyFaultUpdate(new FaultUpdate(1, index, index + 1, entries[index]));
        }

        Assert.That(store.ActiveFaults, Has.Count.EqualTo(4));
        Assert.That(
            () => store.ApplyFaultUpdate(new FaultUpdate(1, 0, 5, entries[0])),
            Throws.TypeOf<ArgumentOutOfRangeException>());
        Assert.That(
            () => store.ApplyFaultUpdate(new FaultUpdate(1, 0, 3, entries[0])),
            Throws.TypeOf<ArgumentException>());
        Assert.That(store.ActiveFaults, Has.Count.EqualTo(4));
    }

    [Test]
    public void ReplaceFaults_PublishesOnlyWhenSnapshotMutatesState()
    {
        var events = new List<IReadOnlyList<FaultEntry>>();
        var aggregator = new EventAggregator();
        aggregator.GetEvent<FaultsChangedEvent>().Subscribe(events.Add);
        var store = new GCBDataStore(aggregator);
        FaultEntry first = Entry(1, "First fault.");
        FaultEntry replacement = first with { Message = "Updated message" };
        var initial = new FaultSnapshot(1, Array.AsReadOnly(new[] { first }));

        store.ReplaceFaults(initial);
        store.ReplaceFaults(initial);
        store.ApplyFaultUpdate(new FaultUpdate(1, 0, 1, replacement));

        Assert.That(store.ActiveFaults, Is.EqualTo(new[] { replacement }));
        Assert.That(events, Has.Count.EqualTo(2));
    }

    private static FaultEntry Entry(uint hash, string format) =>
        new(
            SystemFault.OtherFault,
            hash,
            GcbStateNew.Ready,
            123,
            format,
            format);
}
