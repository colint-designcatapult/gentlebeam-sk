using System;
using Xcc.Core.Domain.UPS;
using Xcc.Core.Services;
using Xcc.Infra.Networking.Udp;

namespace Xcc.Infra.UPS
{
    public class UpsTelemetryBroadcaster(System.Net.IPAddress broadcastAddress, int broadcastPort) : IDisposable
    {
        public void Send(UpsType upsType, IUpsTelemetry? telemetry)
        {
            var packet = UpsTelemetryPacket.FromTelemetry(upsType, _packetCounter++, telemetry);
            _udpBroadcaster.Send(packet.Buffer);
        }

        public void Dispose()
        {
            _udpBroadcaster.Dispose();
        }

        private UdpBroadcaster _udpBroadcaster = new(broadcastAddress, broadcastPort);
        uint _packetCounter = 0;
    }
}
