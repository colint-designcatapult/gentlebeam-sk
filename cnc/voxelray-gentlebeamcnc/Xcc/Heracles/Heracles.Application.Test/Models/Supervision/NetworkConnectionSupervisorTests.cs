using System.Net;
using System.Net.Sockets;
using Heracles.Application.Models.Settings;
using Heracles.Application.Models.Supervision;
using Heracles.Core.Models;
using Moq;
using Xcc.Application.Models;
using Xcc.Core.Logging;
using Xcc.Core.Models;

namespace Heracles.Application.Test.Models.Supervision;

internal class NetworkConnectionSupervisorTests
{
    [Test]
    public void GetGcbTelemetryConnection_UsesLocalDevelopmentListenerPort()
    {
        int endpointPort = GetAvailableUdpPort();
        int listenerPort = GetAvailableUdpPort();
        while (listenerPort == endpointPort)
            listenerPort = GetAvailableUdpPort();

        var settings = new SystemSettings
        {
            EndPointsConfiguration = new EndPointsConfiguration
            {
                GCBTelemetryEndPoint = new SystemEndPoint($"127.0.0.1:{endpointPort}")
            }
        };
        var settingsStore = new Mock<ISystemSettingsStore>();
        settingsStore.SetupGet(store => store.Settings).Returns(settings);
        var debugSettings = new Mock<IDebugSettings>();
        debugSettings.SetupGet(debug => debug.GcbTelemetryListenerPort).Returns(listenerPort);
        var supervisor = new NetworkConnectionSupervisor(
            settingsStore.Object,
            Mock.Of<ILogWriter>(),
            debugSettings.Object);

        using var connection = supervisor.GetGcbTelemetryConnection();

        Assert.Throws<SocketException>(() =>
        {
            using var duplicateListener = new UdpClient(new IPEndPoint(IPAddress.Any, listenerPort));
        });
        Assert.DoesNotThrow(() =>
        {
            using var endpointListener = new UdpClient(new IPEndPoint(IPAddress.Any, endpointPort));
        });
    }

    private static int GetAvailableUdpPort()
    {
        using var socket = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)socket.Client.LocalEndPoint!).Port;
    }
}
