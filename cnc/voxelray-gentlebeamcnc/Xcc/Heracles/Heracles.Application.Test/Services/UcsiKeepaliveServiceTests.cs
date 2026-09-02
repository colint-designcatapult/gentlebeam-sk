using System.Diagnostics;
using Heracles.Ucsi.Services;
using Moq;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;

namespace Heracles.Application.Test.Services;

[TestFixture]
internal sealed class UcsiKeepaliveServiceTests
{
    [Test]
    public async Task Start_SendsVersionRequestOncePerSecond()
    {
        byte[] packet = [1, 2, 3, 4];
        var commandOperator = new Mock<IGcbXRayCommandOperator>();
        commandOperator.Setup(value => value.GenerateVersionInfoRequestCmd()).Returns(packet);
        var communication = new Mock<IGcbCommunicationService>();
        var sendTimes = new List<TimeSpan>();
        var stopwatch = Stopwatch.StartNew();
        var twoSends = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        communication.Setup(value => value.SendRequestAsync(packet, 500))
            .Callback(() =>
            {
                sendTimes.Add(stopwatch.Elapsed);
                if(sendTimes.Count == 2)
                    twoSends.TrySetResult();
            })
            .ReturnsAsync(Array.Empty<byte>());
        using var appCancellation = new CancellationTokenSource();
        var appGlobals = new Mock<IAppGlobals>();
        appGlobals.SetupGet(value => value.AppCancellationTokenSource).Returns(appCancellation);
        await using var sut = new UcsiKeepaliveService(
            commandOperator.Object,
            communication.Object,
            appGlobals.Object,
            new UcsiLogBuffer());

        sut.Start();
        sut.Start();
        await twoSends.Task.WaitAsync(TimeSpan.FromSeconds(2.6));

        Assert.Multiple(() =>
        {
            Assert.That(sendTimes, Has.Count.EqualTo(2));
            Assert.That(sendTimes[0].TotalMilliseconds, Is.InRange(850, 1_300));
            Assert.That((sendTimes[1] - sendTimes[0]).TotalMilliseconds, Is.InRange(850, 1_300));
        });
        commandOperator.Verify(value => value.GenerateVersionInfoRequestCmd(), Times.Exactly(2));
        communication.Verify(value => value.SendRequestAsync(packet, 500), Times.Exactly(2));
    }
}
