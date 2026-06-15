using System.Diagnostics;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Models;
using System.Threading.Tasks;
using System;
using Xcc.Core.Enums;

namespace Xcc.Infra.QualityCheck.Comm.Udp.MockServers
{
    public class MockQcbServer
    {
        Task serverTask = null!;
        public MockQcbServer(
            IAppGlobals appGlobals)
        {
            AppGlobals = appGlobals;
        }

        public IAppGlobals AppGlobals { get; }

        public void Start(int numberOfDiodes, int serverPort, double intensityToShow = 200)
        {
            Debug.WriteLine($"MockQcbServer - start server on local port #{serverPort} with mock intensity = {intensityToShow}");
            var server = new System.Net.Sockets.UdpClient(serverPort);
            var cancellationToken = AppGlobals.AppCancellationTokenSource.Token;

            serverTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var datagram = await server.ReceiveAsync(cancellationToken);
                        UdpPacket requestPacket = new(datagram.Buffer);

                        // Prepare a generic response packet with 5-field payload:
                        UdpPacket response = new(
                                requestPacket.PacketType + 100,
                                requestPacket.PacketCounter,
                                (uint)numberOfDiodes);

                        // Now add custom filling to it
                        if (requestPacket.PacketType == (uint)GCBPacketType.QcbPing)
                        {
                            response[0] = 1; // Ping ack according to the protocol v.3.0
                            Debug.WriteLine($"MockQcbServer: process packet #{requestPacket.PacketCounter} - ping command");
                        }
                        else if (requestPacket.PacketType == (uint)GCBPacketType.QcbReadingsCommand)
                        {
                            QcbReadingsCommandType cmdType = (QcbReadingsCommandType)(uint)requestPacket[0];

                            if (cmdType == QcbReadingsCommandType.Start)
                            {
                                uint rate = requestPacket[1];
                                // Do nothing with the packet, it returns all 0's according to the protocol
                                Debug.WriteLine($"MockQcbServer: process packet #{requestPacket.PacketCounter} - Start command with sampling window of {rate}ms");
                            }
                            else if (cmdType == QcbReadingsCommandType.ReportAndStop)
                            {
                                Debug.WriteLine($"MockQcbServer: process packet #{requestPacket.PacketCounter} - Stop command");
                                // Put here initial intensity values + random deviations in [0, 1) times 1000
                                response[0] = (uint)(intensityToShow * 1000);
                                for (int index = 1; index < numberOfDiodes; ++index)
                                {
                                    int sign = index % 2 == 0 ? 1 : -1;
                                    var intensity = intensityToShow * (1.0 + 0.2 * index);
                                    response[index] = (uint)((intensity + sign * Random.Shared.NextSingle()) * 1000);
                                }
                            }
                        }
                        else // unknown packet type, but we'll response to it with generic packet anyway
                        {
                            Debug.WriteLine($"MockQcbServer: invalid request packet");
                        }

                        response.UpdateCRC();
                        await server.SendAsync(response.Buffer, response.Buffer.Length, datagram.RemoteEndPoint);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"MockQcbServer exception: {ex.Message}");
                    }
                }
            }, AppGlobals.AppCancellationTokenSource.Token);
        }
    }
}
