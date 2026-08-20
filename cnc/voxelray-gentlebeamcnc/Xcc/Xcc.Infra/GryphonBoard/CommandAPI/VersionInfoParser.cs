using System;
using System.Buffers.Binary;
using System.Text;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard.CommandAPI;

public static class VersionInfoParser
{
    private const int VersionStringWordCount = 8;
    private const uint PayloadWordCount = VersionStringWordCount * 2 + 3;

    public static VersionInfo Parse(byte[] data)
    {
        return Parse(new UdpPacket(data));
    }

    public static VersionInfo Parse(UdpPacket packet)
    {
        if (packet.PacketType != (uint)GCBPacketType.VersionInfoResponse
            || packet.PayloadLength != PayloadWordCount)
        {
            throw new ArgumentException("Invalid VersionInfo packet");
        }

        return new VersionInfo
        {
            FirmwareVersion = ReadVersionString(packet, 0),
            FirmwareChecksum = packet[VersionStringWordCount],
            Mode = (FirmwareMode)(int)packet[VersionStringWordCount + 1],
            HvpsFirmwareVersion = ReadVersionString(packet, VersionStringWordCount + 2),
            HvpsMode = (FirmwareMode)(int)packet[(int)PayloadWordCount - 1],
        };
    }

    private static string ReadVersionString(UdpPacket packet, int offset)
    {
        Span<byte> bytes = stackalloc byte[VersionStringWordCount * sizeof(uint)];
        for (var index = 0; index < VersionStringWordCount; index++)
            BinaryPrimitives.WriteUInt32LittleEndian(bytes[(index * sizeof(uint))..], packet[offset + index]);
        return Encoding.ASCII.GetString(bytes).TrimEnd('\0');
    }
}