using System.Net;

namespace GcbTelemetryRelay;

internal static class Program
{
    private const int DefaultFirmwarePort = 40020;
    private static readonly int[] DefaultApplicationPorts = [40021, 40022, 40023];

    private static async Task<int> Main(string[] args)
    {
        if (!TryParsePorts(args, out var firmwarePort, out var applicationPorts))
        {
            Console.Error.WriteLine("Usage: GcbTelemetryRelay [firmware-port application-port [application-port ...]]");
            return 2;
        }

        var targets = applicationPorts.Select(port => new IPEndPoint(IPAddress.Loopback, port));
        var relay = new TelemetryRelay(new IPEndPoint(IPAddress.Any, firmwarePort), targets);
        using var cancellation = new CancellationTokenSource();

        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        try
        {
            Console.WriteLine($"Receiving GCB telemetry on UDP {firmwarePort}.");
            Console.WriteLine($"Forwarding each datagram to {string.Join(", ", applicationPorts.Select(port => $"127.0.0.1:{port}"))}.");
            Console.WriteLine("Press Ctrl+C to stop.");
            await relay.RunAsync(cancellation.Token);
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Telemetry relay stopped: {exception.Message}");
            return 1;
        }
    }

    private static bool TryParsePorts(string[] args, out int firmwarePort, out int[] applicationPorts)
    {
        if (args.Length == 0)
        {
            firmwarePort = DefaultFirmwarePort;
            applicationPorts = DefaultApplicationPorts;
            return true;
        }

        firmwarePort = 0;
        applicationPorts = [];
        if (args.Length < 2)
            return false;

        var ports = new int[args.Length];
        for (var index = 0; index < args.Length; index++)
        {
            if (!int.TryParse(args[index], out ports[index]) || ports[index] is < 1 or > 65535)
                return false;
        }

        firmwarePort = ports[0];
        applicationPorts = ports[1..];
        return applicationPorts.Distinct().Count() == applicationPorts.Length
            && !applicationPorts.Contains(firmwarePort);
    }
}
