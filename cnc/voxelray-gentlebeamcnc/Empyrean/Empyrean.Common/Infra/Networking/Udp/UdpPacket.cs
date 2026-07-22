using System;
using System.Buffers.Binary;

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
        public readonly struct Field
        {
            private readonly uint _value;

            public Field(byte[] value)
            {
                if (value.Length != sizeof(uint))
                    throw new ArgumentException("UdpPacket error: invalid input field data size");

                _value = BinaryPrimitives.ReadUInt32LittleEndian(value);
            }

            public Field(int value) => _value = unchecked((uint)value);
            public Field(uint value) => _value = value;
            public Field(float value) => _value = BitConverter.SingleToUInt32Bits(value);

            public byte[] Data
            {
                get
                {
                    var data = new byte[sizeof(uint)];
                    BinaryPrimitives.WriteUInt32LittleEndian(data, _value);
                    return data;
                }
            }

            public static implicit operator float(Field value) => BitConverter.UInt32BitsToSingle(value._value);
            public static implicit operator int(Field value) => unchecked((int)value._value);
            public static implicit operator uint(Field value) => value._value;
            public static implicit operator byte[](Field value) => value.Data;
            public static implicit operator Field(float value) => new(value);
            public static implicit operator Field(uint value) => new(value);
            public static implicit operator Field(int value) => new(value);
            public static implicit operator Field(byte[] value) => new(value);
        }

        private const uint HeaderFields = (uint)UdpPacketHeaderField.HeaderFields;
        private const int MinimumPacketSize = ((int)UdpPacketHeaderField.HeaderFields + 1) * sizeof(uint);
        private byte[] _buffer;
        private int _payloadLength;

        public UdpPacket()
        {
            _buffer = Array.Empty<byte>();
        }

        public UdpPacket(uint packetType, uint packetCounter, uint payloadLength = 0)
        {
            var totalPacketSize = checked((int)CalculatePacketSize(payloadLength));
            _payloadLength = checked((int)payloadLength);
            _buffer = new byte[totalPacketSize];

            SetHeaderField(UdpPacketHeaderField.Sync1, uint.MaxValue);
            SetHeaderField(UdpPacketHeaderField.Sync2, uint.MaxValue);
            SetHeaderField(UdpPacketHeaderField.PacketType, packetType);
            SetHeaderField(UdpPacketHeaderField.PacketCounter, packetCounter);
            SetHeaderField(UdpPacketHeaderField.PayloadLength, payloadLength);
            if (payloadLength == 0)
                UpdateCRC();
        }

        public UdpPacket(byte[]? buffer)
        {
            _buffer = Array.Empty<byte>();
            var reason = Validate(buffer, out var payloadLength);
            if (reason == ValidationFailure.None)
            {
                _buffer = buffer!;
                _payloadLength = payloadLength;
                return;
            }

            throw reason switch
            {
                ValidationFailure.Null => new ArgumentNullException("UdpPacket parse error: no data"),
                ValidationFailure.TooSmall => new ArgumentException("UdpPacket parse error: packet size is too small"),
                ValidationFailure.LengthMismatch => new ArgumentException("UdpPacket parse error: invalid packet payload size"),
                ValidationFailure.ChecksumMismatch => new ArgumentException("UdpPacket parse error: invalid checksum value"),
                _ => new InvalidOperationException("Unexpected UDP packet validation result"),
            };
        }

        public uint PacketType => GetHeaderField(UdpPacketHeaderField.PacketType);
        public uint PacketCounter => GetHeaderField(UdpPacketHeaderField.PacketCounter);
        public uint PayloadLength => unchecked((uint)_payloadLength);
        public uint CRC => BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(_buffer.Length - sizeof(uint)));
        public byte[] Buffer => _buffer;
        public ReadOnlySpan<byte> Payload =>
            _payloadLength == 0
                ? ReadOnlySpan<byte>.Empty
                : _buffer.AsSpan(GetPayloadFieldOffsetUnsafe(0), checked(_payloadLength * sizeof(uint)));


        public Field this[int index]
        {
            get => new(BinaryPrimitives.ReadUInt32LittleEndian(_buffer.AsSpan(GetPayloadFieldOffset(index), sizeof(uint))));
            set => BinaryPrimitives.WriteUInt32LittleEndian(
                _buffer.AsSpan(GetPayloadFieldOffset(index), sizeof(uint)),
                value);
        }

        public bool TryReset(byte[]? data)
        {
            var reason = Validate(data, out var payloadLength);
            if (reason != ValidationFailure.None)
            {
                _buffer = Array.Empty<byte>();
                _payloadLength = 0;
                return false;
            }

            _buffer = data!;
            _payloadLength = payloadLength;
            return true;
        }

        public UdpPacket Set(int payloadField, Field value)
        {
            this[payloadField] = value;
            return this;
        }

        public UdpPacket UpdateCRC()
        {
            var checksumOffset = GetPayloadFieldOffsetUnsafe(_payloadLength);
            var checksum = CrcUtils.ComputeChecksum(_buffer.AsSpan(0, checksumOffset));
            BinaryPrimitives.WriteUInt32LittleEndian(
                _buffer.AsSpan(checksumOffset, sizeof(uint)),
                checksum);
            return this;
        }

        private ValidationFailure Validate(byte[]? data, out int payloadLength)
        {
            payloadLength = 0;
            if (data is null)
                return ValidationFailure.Null;
            if (data.Length < MinimumPacketSize)
                return ValidationFailure.TooSmall;

            var specifiedPayloadFields = BinaryPrimitives.ReadUInt32LittleEndian(
                data.AsSpan(GetHeaderFieldOffset(UdpPacketHeaderField.PayloadLength), sizeof(uint)));
            var expectedPacketSize = (ulong)(HeaderFields + 1u + specifiedPayloadFields) * sizeof(uint);
            if (expectedPacketSize != (ulong)data.Length || specifiedPayloadFields > int.MaxValue)
                return ValidationFailure.LengthMismatch;

            var checksumOffset = data.Length - sizeof(uint);
            var packetChecksum = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(checksumOffset));
            var calculatedChecksum = CrcUtils.ComputeChecksum(data.AsSpan(0, checksumOffset));
            if (packetChecksum != calculatedChecksum)
                return ValidationFailure.ChecksumMismatch;

            payloadLength = (int)specifiedPayloadFields;
            return ValidationFailure.None;
        }

        private uint GetHeaderField(UdpPacketHeaderField field) =>
            BinaryPrimitives.ReadUInt32LittleEndian(
                _buffer.AsSpan(GetHeaderFieldOffset(field), sizeof(uint)));

        private void SetHeaderField(UdpPacketHeaderField field, uint value) =>
            BinaryPrimitives.WriteUInt32LittleEndian(
                _buffer.AsSpan(GetHeaderFieldOffset(field), sizeof(uint)),
                value);

        private static int GetFieldOffset(int field) => field * sizeof(uint);
        private static int GetHeaderFieldOffset(UdpPacketHeaderField field) => GetFieldOffset((int)field);
        private static int GetPayloadFieldOffsetUnsafe(int field) => GetFieldOffset(field + (int)HeaderFields);
        private static uint CalculatePacketSize(uint payloadFields = 0) => (HeaderFields + payloadFields + 1u) * sizeof(uint);

        private int GetPayloadFieldOffset(int field)
        {
            if (field < 0 || field >= _payloadLength)
                throw new ArgumentOutOfRangeException("UdpPacket error: payload field index is out of range");
            return GetPayloadFieldOffsetUnsafe(field);
        }

        private enum ValidationFailure
        {
            None,
            Null,
            TooSmall,
            LengthMismatch,
            ChecksumMismatch,
        }
    }
}
