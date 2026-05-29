namespace Empyrean.Common.Infra.Networking.Udp
{
    public enum UdpPacketHeaderField : int
    {
        Sync1 = 0, 
        Sync2 = 1, 
        PacketType = 2,
        PacketCounter = 3,
        PayloadLength = 4,
        HeaderFields = 5,
    }

    public class UdpPacket
    {
        public struct Field {
            public byte[] Data { get; private set; }
            public Field(byte[] value)
            {
                if (value.Length == 4)
                {
                    Data = value;
                }
                else
                {
                    throw new ArgumentException("UdpPacket error: invalid input field data size");
                }
            }
            public Field(int value)
            {
                Data = BitConverter.GetBytes(value);
            }
            public Field(uint value)
            {
                Data = BitConverter.GetBytes(value);
            }
            public Field(float value)
            {
                Data = BitConverter.GetBytes(value);
            }

            public static implicit operator float(Field value) => BitConverter.ToSingle(value.Data);
            public static implicit operator int(Field value) => BitConverter.ToInt32(value.Data);
            public static implicit operator uint(Field value) => BitConverter.ToUInt32(value.Data);
            public static implicit operator byte[](Field value) => value.Data;
            public static implicit operator Field(float value) => new Field(value);
            public static implicit operator Field(uint value) => new Field(value);
            public static implicit operator Field(int value) => new Field(value);
            public static implicit operator Field(byte[] value) => new Field(value);
        }

        private readonly byte[] SYNC = BitConverter.GetBytes(0xffffffff);

        private const uint HEADER_FIELDS = (uint)UdpPacketHeaderField.HeaderFields;
        private static readonly uint MIN_PACKET_SIZE = CalculatePacketSize();

        private byte[] buffer;

        private int payloadLength;

        #region Constructors
        public UdpPacket(uint packetType, uint packetCounter, uint payloadLength = 0)
        {
            // header + payload + footer (CRC field):
            uint totalPacketSize = (HEADER_FIELDS + payloadLength + 1) * 4;
            this.payloadLength = (int)payloadLength;
            buffer = new byte[totalPacketSize];

            SetHeaderField(UdpPacketHeaderField.Sync1, SYNC);
            SetHeaderField(UdpPacketHeaderField.Sync2, SYNC);
            SetHeaderField(UdpPacketHeaderField.PacketType, packetType);
            SetHeaderField(UdpPacketHeaderField.PacketCounter, packetCounter);
            SetHeaderField(UdpPacketHeaderField.PayloadLength, payloadLength);
            // We can determine crc already:
            if (payloadLength == 0)
            {
                UpdateCRC();
            }
        }

        public UdpPacket(byte[]? buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("UdpPacket parse error: no data");
            }
            if (buffer.Length < MIN_PACKET_SIZE)
            {
                throw new ArgumentException("UdpPacket parse error: packet size is too small");
            }

            this.buffer = buffer;
            
            // Get and verify specified field number from header:
            uint specifiedPayloadFields = GetHeaderField(UdpPacketHeaderField.PayloadLength);
            this.payloadLength = (int)specifiedPayloadFields;

            uint expectedPacketSize = CalculatePacketSize(specifiedPayloadFields);
            if (expectedPacketSize != buffer.Length)
            {
                throw new ArgumentException("UdpPacket parse error: invalid packet payload size");
            }

            // Check CRC:
            uint packetCRC = CRC;
            uint calculatedCRC = CalculateCRC(
                GetPayloadFieldOffsetUnsafe(this.payloadLength)); // get block size up to the checksum
            if (packetCRC != calculatedCRC)
            {
                throw new ArgumentException("UdpPacket parse error: invalid checksum value");
            }
        }
        #endregion Constructors

        #region Properties
        public uint PacketType
        {
            get => GetHeaderField(UdpPacketHeaderField.PacketType);
        }

        public uint PacketCounter
        {
            get => GetHeaderField(UdpPacketHeaderField.PacketCounter);
        }

        public uint PayloadLength
        {
            get => GetHeaderField(UdpPacketHeaderField.PayloadLength);
        }

        public uint CRC
        {
            get => new Field(buffer.TakeLast(4).ToArray());
        }

        public byte[] Buffer { get => buffer; }
        #endregion Properties

        #region Pulbic methods
        /// <summary>
        /// Payload indexer, starting with 0
        /// </summary>
        /// <param name="index"> defines offset within the payload block in 32bit fields</param>
        /// <returns>Byte representation of a payload field</returns>
        public Field this[int index]
        {
            get => buffer.Skip(GetPayloadFieldOffset(index)).Take(4).ToArray();
            set
            {
                // First we get offset to be sure it's a valid one
                int fieldOffset = GetPayloadFieldOffset(index);
                value.Data.CopyTo(buffer, fieldOffset);
            }
        }

        public UdpPacket Set(int payloadField, Field value)
        {
            this[payloadField] = value;
            return this;
        }

        public UdpPacket UpdateCRC()
        {
            int crcFieldIndex = payloadLength;
            int headerPlusPayloadSize = GetPayloadFieldOffsetUnsafe(crcFieldIndex);

            Field crc = CalculateCRC(headerPlusPayloadSize);

            crc.Data.CopyTo(buffer, headerPlusPayloadSize);

            return this;
        }

        #endregion Pulbic methods

        #region Private methods

        private Field GetHeaderField(UdpPacketHeaderField field)
        {
            return buffer.Skip(GetHeaderFieldOffset(field)).Take(4).ToArray();
        }

        private void SetHeaderField(UdpPacketHeaderField field, Field value)
        {
            value.Data.CopyTo(buffer, GetHeaderFieldOffset(field));
        }

        private static int GetFieldOffset(int field)
        {
            return field * 4;
        }

        private static int GetHeaderFieldOffset(UdpPacketHeaderField field)
        {
            return GetFieldOffset((int)field);
        }

        private static int GetPayloadFieldOffsetUnsafe(int field)
        {
            return GetFieldOffset(field + (int)HEADER_FIELDS);
        }

        private static uint CalculatePacketSize(uint payloadFields = 0)
        {
            return (HEADER_FIELDS + payloadFields + 1) * 4;
        }

        private int GetPayloadFieldOffset(int field)
        {
            if (field < 0 || field >= payloadLength)
            {
                throw new ArgumentOutOfRangeException("UdpPacket error: payload field index is out of range");
            }
            return GetPayloadFieldOffsetUnsafe(field);
        }

        private uint CalculateCRC(int startBlockLength)
        {
            return CrcUtils.ComputeChecksum(buffer.Take(startBlockLength).ToArray());
        }

        #endregion Private methods
    }
}