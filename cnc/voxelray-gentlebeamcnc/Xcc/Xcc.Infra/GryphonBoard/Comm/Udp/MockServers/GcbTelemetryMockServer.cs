using Empyrean.Common.Infra.Networking.Udp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xcc.Core.Enums;
using Xcc.Core.Models;

namespace Xcc.Infra.GryphonBoard.Comm.Udp.MockServers
{
    public class TelemetryPacket : UdpPacket
    {
        public TelemetryPacket(GcbStateNew state, uint packetId = 0)
            : base(packetType: (uint)GCBPacketType.TelemetryResponse,
                  packetCounter: packetId,
                  payloadLength: (uint)GCBTelemetryResponseField.PayloadFields)
        {
            Set((int)GCBTelemetryResponseField.SystemState, (int)state);
            UpdateCRC();
        }
    }

    public class GcbTelemetryMockServer
    {
        Task serverTask = null!;
        public GcbTelemetryMockServer(
            IAppGlobals appGlobals)
        {
            AppGlobals = appGlobals;
        }

        public IAppGlobals AppGlobals { get; }

        public void Start(int serverPort = 50020)
        {
            var cancellationToken = AppGlobals.AppCancellationTokenSource.Token;
            serverTask = Task.Run(async () =>
            {
                var server = new System.Net.Sockets.UdpClient(serverPort);
                while (!cancellationToken.IsCancellationRequested)
                {
                    //await Task.Delay(300);
                    var recv = await server.ReceiveAsync(cancellationToken);

                    UdpPacket request = new(recv.Buffer);
                    if (request.PacketType == (uint)GCBPacketType.TelemetryRequest)
                    {
                        Debug.WriteLine($"{DateTime.Now} GcbTelemetryMockServer: reply with telemetry");
                        UdpPacket response = new TelemetryPacket(GcbStateNew.Cold, packetId: request.PacketCounter);
                        await server.SendAsync(response.Buffer, response.Buffer.Length, recv.RemoteEndPoint);
                    }
                    else
                    {
                        Debug.WriteLine($"{DateTime.Now} GcbTelemetryMockServer: invalid request packet type");
                    }
                }
            }, cancellationToken);
        }
    }
}
