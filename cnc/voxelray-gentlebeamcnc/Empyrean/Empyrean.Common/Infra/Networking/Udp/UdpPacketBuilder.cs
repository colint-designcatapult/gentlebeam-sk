namespace Empyrean.Common.Infra.Networking.Udp
{

    public class UdpPacketBuilder
    {
        UdpPacket packet;
        int payloadFields = 0;
        int fieldIndex = 0;

        public UdpPacketBuilder(uint packetType, uint packetCounter, uint payloadFields = 0)
        {
            packet = new UdpPacket(packetType, packetCounter, payloadFields);
            this.payloadFields = (int)payloadFields;
        }

        public UdpPacketBuilder Add(float value)
        {
            packet.Set(fieldIndex++, value);
            return this;
        }

        public UdpPacketBuilder Add(int value)
        {
            packet.Set(fieldIndex++, value);
            return this;
        }

        public UdpPacketBuilder Add(byte[] value)
        {
            packet.Set(fieldIndex++, value);
            return this;
        }

        public UdpPacketBuilder Check()
        {
            if (fieldIndex != payloadFields)
            {
                throw new Exception("UdpPacketBuilder error: some packet fields were not set");
            }
            return this;
        }

        public UdpPacketBuilder Sign()
        {
            packet.UpdateCRC();
            return this;
        }

        public byte[] Buffer
        {
            get => packet.Buffer;
        }

        public UdpPacket Packet
        {
            get => packet;
        }

        public static UdpPacket BuildPacket(uint packetType, uint packetCounter, ICollection<UdpPacket.Field> payload)
        {
            uint payloadFields = (payload is null) ? 0 : (uint)payload.Count;
            var builder = new UdpPacketBuilder(packetType, packetCounter, payloadFields);

            if (payload is not null && payloadFields > 0)
            {
                foreach (var field in payload)
                {
                    builder.Add(field.Data);
                }
                builder.Check().Sign();
            }
            return builder.Packet;
        }

        public static byte[] BuildRawPacket(uint packetType, uint packetCounter, ICollection<UdpPacket.Field> payload)
        {
            return BuildPacket(packetType, packetCounter, payload).Buffer;
        }
    }

}