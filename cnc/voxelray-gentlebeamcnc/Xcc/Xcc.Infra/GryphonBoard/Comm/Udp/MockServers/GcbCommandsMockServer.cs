using Empyrean.Common.Infra.Networking.Udp;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xcc.Core.Enums;
using Xcc.Core.Models;
using Xcc.Infra.GryphonBoard.CommandAPI;

namespace Xcc.Infra.GryphonBoard.Comm.Udp.MockServers
{
    public class GcbCommandsMockServerTask
    {
        Task serverTask = null!;
        public GcbCommandsMockServerTask(
            IAppGlobals appGlobals)
        {
            AppGlobals = appGlobals;
        }

        public IAppGlobals AppGlobals { get; }

        public void Start(int serverPort = 50007)
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
                    if (request.PacketType != (uint)GCBPacketType.TelemetryRequest)
                    {
                        Debug.WriteLine($"{DateTime.Now} GcbCommandsMockServerTask: reply with error 100 type packet by now");
                        byte[] response = GcbXRayCmdResponseGenerator.GenerateInvalidPacketResponse(request.PacketType);
                        await server.SendAsync(response, response.Length, recv.RemoteEndPoint);
                    }
                    else
                    {
                        Debug.WriteLine($"{DateTime.Now} GcbCommandsMockServerTask: invalid request packet type");
                    }
                }
            }, cancellationToken);
        }
    }
}
