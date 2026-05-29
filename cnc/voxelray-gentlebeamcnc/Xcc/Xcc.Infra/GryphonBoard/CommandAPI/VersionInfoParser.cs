using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard.CommandAPI;

public static class VersionInfoParser
{
    public static VersionInfo Parse(byte[] data)
    {
        UdpPacketIterator iterator = new(data);

        int major = iterator.First();
        int minor = iterator.Next();
        int level = iterator.Next();
        int firmwareChecksum = iterator.Next();
        FirmwareMode mode = (FirmwareMode)(int)iterator.Next();

        return new VersionInfo
        {
            Major = major,
            Minor = minor,
            Level = level,
            FirmwareChecksum = firmwareChecksum,
            Mode = mode
        };
    }
}