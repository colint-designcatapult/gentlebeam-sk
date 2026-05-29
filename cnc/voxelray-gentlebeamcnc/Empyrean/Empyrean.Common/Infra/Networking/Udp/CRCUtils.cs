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

        public static uint ComputeChecksum(byte[] bytes)
        {
            return ComputeChecksum(bytes, Table);
        }

        public static byte[] GetCrc(byte[] bytes, in uint[] crcTable)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes, crcTable));
        }

        public static byte[] GetCrc(byte[] bytes)
        {
            return GetCrc(bytes, Table);
        }

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
                    {
                        temp = temp >> 1 ^ poly;
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                crcTable[i] = temp;
            }

            return crcTable;
        }

        //public static uint CRC32(byte[] bytes)
        //{
        //    uint b, crc, mask;
        //    crc = 0xFFFFFFFF;

        //    for (var i = 0; i < bytes.Length; i++)
        //    {
        //        b = bytes[i]; 

        //        crc = crc ^ b;

        //        for (var j = 7; j >= 0; j--)
        //        { 
        //            mask = ~(crc & 1U) + 1U;

        //            crc = (crc >> 1) ^ (0xEDB88320 & mask);
        //        }
        //    }

        //    return ~crc;
        //}
    }
}
