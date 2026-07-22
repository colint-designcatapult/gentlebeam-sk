using System.Net;
using System.Net.Sockets;
using GcbTelemetryRelay;
using NUnit.Framework;

namespace Empyrean.Common.Test.Infra.Networking.Udp;

internal class TelemetryRelayTests
{
    [Test]
    public async Task RunAsync_ForwardsEveryDatagramToBothApplicationPorts()
    {
        using var externalListener = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        using var indoorListener = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        var externalEndpoint = (IPEndPoint)externalListener.Client.LocalEndPoint!;
        var indoorEndpoint = (IPEndPoint)indoorListener.Client.LocalEndPoint!;
        var relay = new TelemetryRelay(
            new IPEndPoint(IPAddress.Loopback, 0),
            [externalEndpoint, indoorEndpoint]);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(3));

        var relayTask = relay.RunAsync(cancellation.Token);
        using var firmwareSender = new UdpClient(AddressFamily.InterNetwork);
        byte[] telemetryDatagram = [1, 2, 3, 4, 5];
        await firmwareSender.SendAsync(telemetryDatagram, relay.BoundEndpoint!, cancellation.Token);

        var received = await Task.WhenAll(
            externalListener.ReceiveAsync(cancellation.Token).AsTask(),
            indoorListener.ReceiveAsync(cancellation.Token).AsTask());

        Assert.Multiple(() =>
        {
            Assert.That(received[0].Buffer, Is.EqualTo(telemetryDatagram));
            Assert.That(received[1].Buffer, Is.EqualTo(telemetryDatagram));
        });

        await cancellation.CancelAsync();
        await relayTask;
    }
}
