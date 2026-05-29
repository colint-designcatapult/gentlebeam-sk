using System;
using System.Diagnostics;
using System.Threading;
using Xcc.Core.Domain.UPS;
using Xcc.Core.Services;
using Xcc.Infra.Networking.Udp;

namespace Xcc.Infra.UPS
{
    public class UpsTelemetryReceiver(int upsBroadcastPort, CancellationToken token) : IUpsService
    {
        public event EventHandler<UpsTelemetryUpdatedArgs>? UpsTelemetryUpdated;

        public void Start()
        {
            UdpReceiver receiver = new UdpReceiver(clientPort: upsBroadcastPort, reusePort: true);
            receiver.UdpReceiveEvent += (s, data) =>
            {
                try
                {
                    var packet = new UpsTelemetryPacket(data.Buffer);
                    var upsType = (UpsType)packet.PacketType;
                    // Skip all other ups telemetry data:
                    if (upsType != UpsType.Primary)
                        return;

                    IUpsTelemetry? upsTelemetry = packet.ToUpsTelemetry();
                    UpsTelemetryUpdated?.Invoke(this, new UpsTelemetryUpdatedArgs(upsType, upsTelemetry));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"UpsTelemetryReceiver error: {ex.Message}");
                }
            };

            receiver.Start(token);
        }
    }
}
