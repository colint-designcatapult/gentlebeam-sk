using System;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard.CommandAPI;

public static class VersionInfoParser
{
    public static VersionInfo Parse(byte[] data)
    {
        return Parse(new UdpPacket(data));
    }

    public static VersionInfo Parse(UdpPacket packet)
    {
        if (packet.PacketType != (uint)GCBPacketType.VersionInfoResponse
            || packet.PayloadLength != 5u)
        {
            throw new ArgumentException("Invalid VersionInfo packet");
        }

        return new VersionInfo
        {
            Major = packet[0],
            Minor = packet[1],
            Level = packet[2],
            FirmwareChecksum = packet[3],
            Mode = (FirmwareMode)(int)packet[4],
        };
    }
}