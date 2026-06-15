namespace Empyrean.Common.Infra.Networking.Udp
{

    /// <summary>
    /// Helper class to just extract data from the packet payload sequentially, without field indexes
    /// </summary>
    public class UdpPacketIterator
    {
        private int currentIndex = 0;

        public UdpPacketIterator(byte[]? data)
        {
            UdpPacket = new UdpPacket(data);
        }
        public UdpPacketIterator(UdpPacket? packet)
        {
            if (packet is null)
            {
                throw new ArgumentNullException("UdpPacketIterator error: no data");
            }
            UdpPacket = packet;
        }
 
        public UdpPacket UdpPacket { get; }

        public UdpPacket.Field First()
        {
            currentIndex = 0;
            return UdpPacket[currentIndex];
        }

        public UdpPacket.Field Next()
        {
            return UdpPacket[++currentIndex];
        }
    }
}