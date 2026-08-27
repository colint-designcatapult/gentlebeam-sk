using System.Net;
using System.Net.Sockets;
using Empyrean.Common.Infra.Networking.Udp;
using Heracles.Ucsi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xcc.Application.Models;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard;
using Xcc.Infra.GryphonBoard.Comm;
using Xcc.Test.Xcc.Infra;

namespace Heracles.Application.Test.Services;

internal sealed class StandaloneUcsiTelemetryTests
{
    [Test]
    public void DefaultConfiguration_UsesRemotePort20AndTelemetryListener40020()
    {
        var options = new UcsiStandaloneTelemetryOptions(new ConfigurationBuilder().Build());

        Assert.Multiple(() =>
        {
            Assert.That(options.RemotePort, Is.EqualTo(20));
            Assert.That(options.ListenerPort, Is.EqualTo(40_020));
        });
    }

    [Test]
    [SkipIfCi("Infrastructure test requiring hardware/UCSI connection")]
    public async Task Listener_CapturesAFullSecondOf100HzTelemetry()
    {
        int listenerPort = ReserveUdpPort();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Ucsi:Telemetry:RemoteAddress"] = "172.31.1.1",
                ["Ucsi:Telemetry:RemotePort"] = "40020",
                ["Ucsi:Telemetry:ListenerPort"] = listenerPort.ToString(),
            })
            .Build();
        var hub = new DecodedTelemetryFrameHub();
        var delivered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        int frameCount = 0;
        DateTimeOffset firstTimestamp = default;
        using IDisposable subscription = hub.Subscribe(frame =>
        {
            if (Interlocked.Increment(ref frameCount) == 1)
                firstTimestamp = frame.ReceivedAtUtc;
            if (Volatile.Read(ref frameCount) == 100)
                delivered.TrySetResult();
        });
        var currentTelemetry = new Mock<ISystemTelemetryChanged>();
        using var service = new GcbTelemetryService(
            new AppGlobals(),
            new StandaloneTelemetryConnectionFactory(new UcsiStandaloneTelemetryOptions(configuration)),
            new SystemTelemetryProcessor(
                currentTelemetry.Object,
                Mock.Of<IGCBDataStore>(),
                hub),
            new UcsiLogBuffer());
        using var sender = new UdpClient();

        service.Start();
        await sender.SendAsync(BuildVersionInfo(), new IPEndPoint(IPAddress.Loopback, listenerPort));
        for (int index = 0; index < 100; index++)
        {
            await sender.SendAsync(BuildNormalTelemetry(index * 10), new IPEndPoint(IPAddress.Loopback, listenerPort));
            await Task.Delay(10);
        }
        await delivered.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Multiple(() =>
        {
            Assert.That(frameCount, Is.EqualTo(100));
            Assert.That(firstTimestamp, Is.Not.EqualTo(default(DateTimeOffset)));
        });
        currentTelemetry.Verify(
            value => value.OnSystemTelemetryChanged(It.IsAny<ISystemTelemetry>()),
            Times.Exactly(4));
    }

    private static int ReserveUdpPort()
    {
        using var reservation = new UdpClient(0);
        return ((IPEndPoint)reservation.Client.LocalEndPoint!).Port;
    }

    private static byte[] BuildVersionInfo()
    {
        var packet = new UdpPacket((uint)GCBPacketType.VersionInfoResponse, 0, 5);
        packet[0] = 2;
        packet[1] = 0;
        packet[2] = 1;
        packet[3] = 0;
        packet[4] = (int)FirmwareMode.Normal;
        return packet.UpdateCRC().Buffer;
    }

    private static byte[] BuildNormalTelemetry(int runtime)
    {
        var packet = new UdpPacket(
            (uint)GCBPacketType.TelemetryResponse,
            0,
            (uint)NormalTelemetryField.PayloadFields);
        packet[(int)NormalTelemetryField.SystemState] = (int)GcbStateNew.StandBy;
        packet[(int)NormalTelemetryField.SystemRuntime] = runtime;
        packet[(int)NormalTelemetryField.Reserved1] = 1u;
        return packet.UpdateCRC().Buffer;
    }
}
