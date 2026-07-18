using System;

namespace Empyrean.Common.Infra.Networking.Udp
{
    public class CrcUtils
    {
        private static readonly uint[] _table = GenerateCrcTable();
        public static uint[] Table => _table;

        public static uint ComputeChecksum(byte[] bytes, in uint[] crcTable)
        {
            uint crc = 0xffffffff;
            foreach (var b in bytes)
            {
                byte index = (byte)(crc & 0xff ^ b);
                crc = crc >> 8 ^ crcTable[index];
            }
            return ~crc;
        }

        public static uint ComputeChecksum(byte[] bytes) => ComputeChecksum(bytes.AsSpan());

        public static uint ComputeChecksum(ReadOnlySpan<byte> bytes)
        {
            uint crc = 0xffffffff;
            foreach (var b in bytes)
            {
                byte index = (byte)(crc & 0xff ^ b);
                crc = crc >> 8 ^ Table[index];
            }
            return ~crc;
        }

        public static byte[] GetCrc(byte[] bytes, in uint[] crcTable) =>
            BitConverter.GetBytes(ComputeChecksum(bytes, crcTable));

        public static byte[] GetCrc(byte[] bytes) => BitConverter.GetBytes(ComputeChecksum(bytes));

        public static uint[] GenerateCrcTable()
        {
            const uint poly = 0xedb88320;
            var crcTable = new uint[256];
            uint temp = 0;

            for (uint i = 0; i < crcTable.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                        temp = temp >> 1 ^ poly;
                    else
                        temp >>= 1;
                }
                crcTable[i] = temp;
            }

            return crcTable;
        }
    }
}
