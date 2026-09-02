using Xcc.Infra.Networking.gRPC.EventStreams;

namespace Xcc.Test.Xcc.Infra.Networking.gRPC.EventStreams;

internal class BroadcastEventHubTests
{
    [Test]
    public async Task Publish_DeliversEventToEverySubscriber()
    {
        var hub = new BroadcastEventHub<int>();
        using var cancellation = new CancellationTokenSource();
        var firstReceived = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReceived = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task firstStream = hub.StreamAsync(
            (value, _) =>
            {
                firstReceived.TrySetResult(value);
                return Task.CompletedTask;
            },
            cancellation.Token);
        Task secondStream = hub.StreamAsync(
            (value, _) =>
            {
                secondReceived.TrySetResult(value);
                return Task.CompletedTask;
            },
            cancellation.Token);

        hub.Publish(42);

        await Task.WhenAll(firstReceived.Task, secondReceived.Task).WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Multiple(() =>
        {
            Assert.That(firstReceived.Task.Result, Is.EqualTo(42));
            Assert.That(secondReceived.Task.Result, Is.EqualTo(42));
        });

        await cancellation.CancelAsync();
        await IgnoreCancellation(firstStream);
        await IgnoreCancellation(secondStream);
    }

    private static async Task IgnoreCancellation(Task stream)
    {
        try
        {
            await stream;
        }
        catch (OperationCanceledException)
        {
        }
    }
}
