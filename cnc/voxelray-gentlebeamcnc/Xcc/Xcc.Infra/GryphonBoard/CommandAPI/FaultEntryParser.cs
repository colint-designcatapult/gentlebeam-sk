using System;
using System.Text;
using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Infra.GryphonBoard.CommandAPI
{
    public static class FaultEntryParser
    {
        public const int ResponseWords = 45;
        private const int MetadataWords = 8;
        private const int FormatBytes = 128;
        private const int ArgumentWordOffset = 40;
        private const int MaxActiveFaults = 4;
        private const int MaxArguments = 5;

        public static FaultUpdate Parse(UdpPacket packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            if (packet.PacketType != (uint)GCBPacketType.FaultInfoResponse)
                throw new ArgumentException("Packet is not a fault information response.", nameof(packet));
            if (packet.PayloadLength != ResponseWords)
                throw new ArgumentException($"Fault information response must contain exactly {ResponseWords} payload words.", nameof(packet));

            uint faultTypeValue = packet[0];
            uint formatHash = packet[1];
            uint clearEpoch = packet[2];
            uint entryIndex = packet[3];
            uint activeCount = packet[4];
            uint capturedState = packet[5];
            uint capturedRuntime = packet[6];
            uint argumentCount = packet[7];

            if (faultTypeValue > (uint)SystemFault.OtherFault)
                throw new FormatException("Fault type is outside the supported range.");
            if (activeCount > MaxActiveFaults)
                throw new FormatException($"Active fault count cannot exceed {MaxActiveFaults}.");
            if (argumentCount > MaxArguments)
                throw new FormatException($"Fault argument count cannot exceed {MaxArguments}.");

            ReadOnlySpan<byte> formatBytes = packet.Payload.Slice(MetadataWords * sizeof(uint), FormatBytes);
            int terminatorIndex = formatBytes.IndexOf((byte)0);
            if (terminatorIndex < 0)
                throw new FormatException("Fault format is missing its NUL terminator.");

            ReadOnlySpan<byte> visibleFormatBytes = formatBytes[..terminatorIndex];
            foreach (byte value in visibleFormatBytes)
            {
                if (value < 0x20 || value > 0x7e)
                    throw new FormatException("Fault format must contain only printable ASCII characters.");
            }

            foreach (byte value in formatBytes[(terminatorIndex + 1)..])
            {
                if (value != 0)
                    throw new FormatException("Fault format contains non-zero bytes after its NUL terminator.");
            }

            Span<uint> arguments = stackalloc uint[MaxArguments];
            for (int index = 0; index < MaxArguments; ++index)
                arguments[index] = packet[ArgumentWordOffset + index];

            if (faultTypeValue == (uint)SystemFault.Reserved)
            {
                if (activeCount != 0 && entryIndex < activeCount)
                    throw new FormatException("An in-range fault response must contain an entry.");
                if (formatHash != 0 ||
                    capturedState != 0 ||
                    capturedRuntime != 0 ||
                    argumentCount != 0 ||
                    terminatorIndex != 0 ||
                    ContainsNonZero(arguments))
                {
                    throw new FormatException("An empty fault response contains entry data.");
                }

                return new FaultUpdate(clearEpoch, entryIndex, activeCount, null);
            }

            if (entryIndex >= activeCount)
                throw new FormatException("A fault entry index must be less than the active fault count.");

            uint calculatedHash = CrcUtils.ComputeChecksum(visibleFormatBytes);
            if (formatHash != calculatedHash)
                throw new FormatException("Fault format checksum does not match the transmitted hash.");

            string format = Encoding.ASCII.GetString(visibleFormatBytes);
            string message = FaultMessageFormatter.Format(format, arguments[..(int)argumentCount]);
            var entry = new FaultEntry(
                (SystemFault)faultTypeValue,
                formatHash,
                unchecked((GcbStateNew)(int)capturedState),
                capturedRuntime,
                format,
                message);

            return new FaultUpdate(clearEpoch, entryIndex, activeCount, entry);
        }

        private static bool ContainsNonZero(ReadOnlySpan<uint> values)
        {
            foreach (uint value in values)
            {
                if (value != 0)
                    return true;
            }

            return false;
        }
    }
}
