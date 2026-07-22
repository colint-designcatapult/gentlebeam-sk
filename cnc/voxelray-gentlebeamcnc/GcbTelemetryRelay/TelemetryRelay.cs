using System.Net;
using System.Net.Sockets;

namespace GcbTelemetryRelay;

public sealed class TelemetryRelay
{
    private readonly IPEndPoint _listenEndpoint;
    private readonly IPEndPoint[] _targetEndpoints;

    public TelemetryRelay(IPEndPoint listenEndpoint, IEnumerable<IPEndPoint> targetEndpoints)
    {
        ArgumentNullException.ThrowIfNull(listenEndpoint);
        ArgumentNullException.ThrowIfNull(targetEndpoints);

        _listenEndpoint = listenEndpoint;
        _targetEndpoints = targetEndpoints.ToArray();

        if (_listenEndpoint.AddressFamily != AddressFamily.InterNetwork)
            throw new ArgumentException("The telemetry relay supports IPv4 endpoints only.", nameof(listenEndpoint));
        if (_targetEndpoints.Length == 0)
            throw new ArgumentException("At least one target endpoint is required.", nameof(targetEndpoints));
        if (_targetEndpoints.Any(endpoint => endpoint.AddressFamily != AddressFamily.InterNetwork))
            throw new ArgumentException("The telemetry relay supports IPv4 endpoints only.", nameof(targetEndpoints));
    }

    public IPEndPoint? BoundEndpoint { get; private set; }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using var client = new UdpClient(AddressFamily.InterNetwork);
        client.Client.ExclusiveAddressUse = true;
        client.Client.Bind(_listenEndpoint);
        DisableUdpConnectionReset(client);

        BoundEndpoint = (IPEndPoint)client.Client.LocalEndPoint!;

        try
        {
            while (true)
            {
                var datagram = await client.ReceiveAsync(cancellationToken);
                foreach (var targetEndpoint in _targetEndpoints)
                    await client.SendAsync(datagram.Buffer, targetEndpoint, cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static void DisableUdpConnectionReset(UdpClient client)
    {
        if (!OperatingSystem.IsWindows())
            return;

        const int SioUdpConnectionReset = -1744830452;
        client.Client.IOControl(SioUdpConnectionReset, [0], null);
    }
}
