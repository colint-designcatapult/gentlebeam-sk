using System;

namespace Empyrean.Common.Infra.Networking.Udp
{
    /// <summary>
    /// Helper to extract packet payload fields sequentially.
    /// </summary>
    public struct UdpPacketIterator
    {
        private int _currentIndex;

        public UdpPacketIterator(byte[]? data)
        {
            UdpPacket = new UdpPacket(data);
            _currentIndex = -1;
        }

        public UdpPacketIterator(UdpPacket? packet)
        {
            if (packet is null)
                throw new ArgumentNullException("UdpPacketIterator error: no data");

            UdpPacket = packet;
            _currentIndex = -1;
        }

        public UdpPacket UdpPacket { get; }

        public UdpPacket.Field First()
        {
            _currentIndex = 0;
            return UdpPacket[_currentIndex];
        }

        public UdpPacket.Field Next() => UdpPacket[++_currentIndex];
    }
}
