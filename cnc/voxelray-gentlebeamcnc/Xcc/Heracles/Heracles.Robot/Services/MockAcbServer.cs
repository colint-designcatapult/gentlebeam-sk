using Empyrean.Common.Infra.Networking.Udp;

using Heracles.Core.Enums;
using Heracles.Core.Models;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Heracles.Robot.Models;
using Heracles.Robot.Models.Enums;
using Xcc.Core.Models;

namespace Heracles.Robot.Services
{
    public class MockAcbServer
    {
        Task serverTask = null;
        public MockAcbServer(
            IAppGlobals appGlobals)
        {
            AppGlobals = appGlobals;
        }

        public IAppGlobals AppGlobals { get; }

        public void Start(int serverPort = 50008, double intensity = 200)
        {
            var cancellationToken = AppGlobals.AppCancellationTokenSource.Token;
            serverTask = Task.Run(async () => 
            {
                var server = new System.Net.Sockets.UdpClient(serverPort);
                AcbActuatorState state = AcbActuatorState.Unlock;
                AcbActuatorState nonWorkingstate = AcbActuatorState.Unknown;
                AcbActuatorId workingActuator = AcbActuatorId.Image;

                while (!cancellationToken.IsCancellationRequested)
                {
                    //await Task.Delay(300);
                    var recv = await server.ReceiveAsync(cancellationToken);

                    UdpPacket request = new(recv.Buffer);
                    if (request.PacketType == (uint)AcbPacketType.StatusPoll)
                    {
                        UdpPacket response = new(request.PacketType, request.PacketCounter, 3);
                        int pedal = state switch
                        {
                            AcbActuatorState.Unlock => 0x02000000, // up
                            AcbActuatorState.Lock => 0x01000000,   // down
                            AcbActuatorState.Unknown => 0,         // unknown
                            _ => throw new Exception("Oh ooh")
                        };
                        response[0] = (int)state | pedal;
                        response[1] = (int)nonWorkingstate | pedal;
                        response[2] = (int)nonWorkingstate | pedal;
                        //Debug.WriteLine($"{DateTime.Now} Process polling command: actuatorId={actuatorId}, state={(int)response[1]}");
                        response.UpdateCRC();
                        await server.SendAsync(response.Buffer, response.Buffer.Length, recv.RemoteEndPoint);
                    }
                    else if (request.PacketType == (uint)AcbPacketType.Actuators)
                    {
                        _ = Task.Run(async () =>
                        {
                            // Send another poll response just to test if we're able to handle this:
                            UdpPacket response = new(request.PacketType, request.PacketCounter, 3);
                            response[0] = request[0];
                            response[1] = request[1];
                            response[2] = 0;
                            response.UpdateCRC();

                            await server.SendAsync(response.Buffer, response.Buffer.Length, recv.RemoteEndPoint);
                            Debug.WriteLine($"{DateTime.Now} Process actuator command: set state = {state}");
                            if ((int)request[0] == (int)workingActuator)
                            {
                                state = AcbActuatorState.Unknown;
                                await Task.Delay(2000);
                                state = (AcbActuatorState)(int)request[1];
                            }
                        });
                    }
                }
            },  cancellationToken);
        }
    }
}
