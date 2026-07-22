using Empyrean.Common.Infra.Networking.Udp;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Test.Xcc.Infra.GryphonBoard.CommandAPI;

internal class GcbCommandInterfaceFaultTests
{
    [Test]
    public async Task GetFaults_ReturnsStableFourEntrySnapshot()
    {
        FaultEntry[] entries = Enumerable.Range(0, 4)
            .Select(index => Entry(index))
            .ToArray();
        var responses = new Queue<byte[]>(Enumerable.Range(0, 4).Select(index =>
            Response(new FaultUpdate(5, (uint)index, 4, entries[index]))));
        var requestedIndices = new List<uint>();
        GcbCommandInterface sut = CreateService(responses, requestedIndices);

        FaultSnapshot snapshot = await sut.GetFaults();

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.ClearEpoch, Is.EqualTo(5));
            Assert.That(snapshot.Entries, Is.EqualTo(entries));
            Assert.That(requestedIndices, Is.EqualTo(new uint[] { 0, 1, 2, 3 }));
        });
    }

    [Test]
    public async Task GetFaults_ExpandsWhenListGrowsDuringSynchronization()
    {
        FaultEntry[] entries = Enumerable.Range(0, 4)
            .Select(index => Entry(index))
            .ToArray();
        var responses = new Queue<byte[]>(new[]
        {
            Response(new FaultUpdate(5, 0, 2, entries[0])),
            Response(new FaultUpdate(5, 1, 4, entries[1])),
            Response(new FaultUpdate(5, 2, 4, entries[2])),
            Response(new FaultUpdate(5, 3, 4, entries[3]))
        });
        GcbCommandInterface sut = CreateService(responses);

        FaultSnapshot snapshot = await sut.GetFaults();

        Assert.That(snapshot.Entries, Is.EqualTo(entries));
    }

    [Test]
    public async Task GetFaults_RestartsAfterEpochChange()
    {
        FaultEntry oldEntry = Entry(0);
        FaultEntry currentEntry = Entry(1);
        var responses = new Queue<byte[]>(new[]
        {
            Response(new FaultUpdate(5, 0, 2, oldEntry)),
            Response(new FaultUpdate(6, 1, 0, null)),
            Response(new FaultUpdate(6, 0, 1, currentEntry))
        });
        GcbCommandInterface sut = CreateService(responses);

        FaultSnapshot snapshot = await sut.GetFaults();

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.ClearEpoch, Is.EqualTo(6));
            Assert.That(snapshot.Entries, Is.EqualTo(new[] { currentEntry }));
        });
    }

    [Test]
    public void GetFaults_ThrowsAfterThreeEpochChangeRetries()
    {
        FaultEntry entry = Entry(0);
        var responses = new Queue<byte[]>();
        for (uint epoch = 1; epoch <= 3; epoch++)
        {
            responses.Enqueue(Response(new FaultUpdate(epoch, 0, 2, entry)));
            responses.Enqueue(Response(new FaultUpdate(epoch + 1, 1, 0, null)));
        }
        GcbCommandInterface sut = CreateService(responses);

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetFaults());

        Assert.That(exception!.Message, Is.EqualTo("Fault list changed while synchronizing"));
    }

    private static GcbCommandInterface CreateService(
        Queue<byte[]> responses,
        List<uint>? requestedIndices = null)
    {
        var communication = new Mock<IGcbCommunicationService>();
        communication
            .Setup(value => value.SendRequestAsync(It.IsAny<byte[]>()))
            .Callback<byte[]>(request =>
            {
                if (requestedIndices is not null)
                {
                    requestedIndices.Add(new UdpPacket(request)[0]);
                }
            })
            .Returns(() => Task.FromResult<byte[]?>(responses.Dequeue()));

        return new GcbCommandInterface(
            new GcbXRayCommandOperator(),
            communication.Object,
            Mock.Of<ILogWriter>());
    }

    private static FaultEntry Entry(int index)
    {
        string format = $"Fault {index}.";
        return new FaultEntry(
            SystemFault.OtherFault,
            CrcUtils.ComputeChecksum(System.Text.Encoding.ASCII.GetBytes(format)),
            GcbStateNew.Ready,
            (uint)index,
            format,
            format);
    }

    private static byte[] Response(FaultUpdate update) =>
        GcbXRayCmdResponseGenerator.GenerateFaultInfoResponse(0, update);
}
