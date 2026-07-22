using Empyrean.Common.Infra.Networking.Udp;

namespace Empyrean.Common.Test.Infra.Networking.Udp
{
    internal class UdpPacketIteratorTests
    {
        [Test]
        public void Constructor_With_NullBuffer()
        {
            byte[]? nullBytes = null;
            var exception = Assert.Throws<ArgumentNullException>(() => new UdpPacketIterator(nullBytes));
            Assert.That(exception.Message, Contains.Substring("no data"));
        }
        
        [Test]
        public void Constructor_With_NullPacket()
        {
            UdpPacket? nullPacket = null;
            var exception = Assert.Throws<ArgumentNullException>(() => new UdpPacketIterator(nullPacket));
            Assert.That(exception.Message, Contains.Substring("no data"));
        }
        
        [Test]
        public void Constructor_With_SmallBuffer(
            [Range(0, 23)] int bufferSize)
        {
            var smallBuffer = new byte[bufferSize];
            var exception = Assert.Throws<ArgumentException>(() => new UdpPacketIterator(smallBuffer));
            
            Assert.That(exception.Message, Contains.Substring("packet size is too small"));
        }

        /// <summary>
        /// Generate buffer with incorrect size (Payload Length)
        /// </summary>
        private static IEnumerable<byte[]> RangeBufferWithInvalidPayload(int minPayloadCount, int maxPayloadCount, params int [] sizeDiffs)
        {
            for (int payloadCount = minPayloadCount; payloadCount <= maxPayloadCount; payloadCount++)
            {
                var correctSize = 24 + payloadCount * 4;
                foreach (var sizeDiff in sizeDiffs)
                {
                    var incorrectSize = correctSize + sizeDiff;
                    if (incorrectSize > 24)
                    {
                        var buffer = new byte[incorrectSize];

                        // Set the payload length in the header
                        buffer[16] = (byte)(payloadCount >> (0 * 8));
                        buffer[17] = (byte)(payloadCount >> (1 * 8));
                        buffer[18] = (byte)(payloadCount >> (2 * 8));
                        buffer[19] = (byte)(payloadCount >> (3 * 8));

                        yield return buffer;
                    }
                }
            }
        }

        private static IEnumerable<byte[]> BufferWithInvalidPayload => RangeBufferWithInvalidPayload(0, 5, new[]{-3, -2, -1, 1, 2, 3}); // Exclude 0
        
        [Test]
        public void Constructor_With_InvalidBufferSize(
            [ValueSource(nameof(BufferWithInvalidPayload))] byte[] buffer)
        {
            var exception = Assert.Throws<ArgumentException>(() => new UdpPacketIterator(buffer));
            Assert.That(exception.Message, Contains.Substring("invalid packet payload size"));
        }
        
        [Test]
        public void Constructor_With_InvalidBufferCRC()
        {
            Random random = new Random();

            int minSize = 24;
            int wordCount = random.Next(minSize / 4, 100);
            int bufferSize = wordCount * 4;
            
            // Fill buffer with random data and fix payload length field
            var buffer = new byte[bufferSize];
            random.NextBytes(buffer);
            var payloadCount = wordCount - minSize / 4;
            buffer[16] = (byte)(payloadCount >> (0 * 8));
            buffer[17] = (byte)(payloadCount >> (1 * 8));
            buffer[18] = (byte)(payloadCount >> (2 * 8));
            buffer[19] = (byte)(payloadCount >> (3 * 8));
            
            // Calculate CRC of buffer data
            var dataBytes = buffer.Take(bufferSize - 4).ToArray();
            var crc = CrcUtils.ComputeChecksum(dataBytes);
            
            crc += 1; // break the CRC value
            var crcBytes = BitConverter.GetBytes(crc);
            var crcStart = bufferSize - 4;
            buffer[crcStart + 0] = crcBytes[0];
            buffer[crcStart + 1] = crcBytes[1];
            buffer[crcStart + 2] = crcBytes[2];
            buffer[crcStart + 3] = crcBytes[3];
            
            var exception = Assert.Throws<ArgumentException>(() => new UdpPacketIterator(buffer));
            Assert.That(exception.Message, Contains.Substring("invalid checksum value"));
        }
        
        [Test]
        public void Constructor_With_ValidBuffer()
        {
            Random random = new Random();

            int minSize = 24;
            int wordCount = random.Next(minSize / 4, 100);
            int bufferSize = wordCount * 4;
            
            // Fill buffer with random data and fix payload length field
            var buffer = new byte[bufferSize];
            random.NextBytes(buffer);
            var payloadCount = wordCount - minSize / 4;
            buffer[16] = (byte)(payloadCount >> (0 * 8));
            buffer[17] = (byte)(payloadCount >> (1 * 8));
            buffer[18] = (byte)(payloadCount >> (2 * 8));
            buffer[19] = (byte)(payloadCount >> (3 * 8));
            
            // Calculate CRC of buffer data
            var dataBytes = buffer.Take(bufferSize - 4).ToArray();
            var crc = CrcUtils.ComputeChecksum(dataBytes);
            
            var crcBytes = BitConverter.GetBytes(crc);
            var crcStart = bufferSize - 4;
            buffer[crcStart + 0] = crcBytes[0];
            buffer[crcStart + 1] = crcBytes[1];
            buffer[crcStart + 2] = crcBytes[2];
            buffer[crcStart + 3] = crcBytes[3];
            
            var iterator = new UdpPacketIterator(buffer);
            
            Assert.That(iterator.UdpPacket, Is.Not.Null);
            Assert.That(iterator.UdpPacket.Buffer, Is.SameAs(buffer));
        }
        
        [Test]
        public void Constructor_With_ValidPacket(
            [Random(0, 100, 2)] int packetType, 
            [Random(0, 100, 2)] int packetCounter, 
            [Random(0, 100, 2)] int payloadLength)
        {
            var validPacket = new UdpPacket((uint)packetType, (uint)packetCounter, (uint)payloadLength);
            var iterator = new UdpPacketIterator(validPacket);
            
            Assert.That(iterator.UdpPacket, Is.SameAs(validPacket));
            Assert.That(iterator.UdpPacket.Buffer, Is.Not.Null);
            Assert.That(iterator.UdpPacket.PacketType, Is.EqualTo(packetType));
            Assert.That(iterator.UdpPacket.PacketCounter, Is.EqualTo(packetCounter));
            Assert.That(iterator.UdpPacket.PayloadLength, Is.EqualTo(payloadLength));
        }
        
        [Test]
        public void FirstAlwaysWithSameData(
            [Random(0, 100, 2)] int packetType, 
            [Random(0, 100, 2)] int packetCounter, 
            [Random(2, 100, 2)] int payloadLength)
        {
            var packet = new UdpPacket((uint)packetType, (uint)packetCounter, (uint)payloadLength);
            var iterator = new UdpPacketIterator(packet);

            var firstData = iterator.First().Data;
            Assert.That(iterator.First().Data, Is.EqualTo(firstData));
            Assert.That(iterator.First().Data, Is.EqualTo(firstData));

            iterator.Next();
            Assert.That(iterator.First().Data, Is.EqualTo(firstData));
            
            iterator.Next();
            iterator.Next();
            Assert.That(iterator.First().Data, Is.EqualTo(firstData));
        }
        
        [Test]
        public void Next(
            [Random(0, 100, 2)] int packetType, 
            [Random(0, 100, 2)] int packetCounter, 
            [Random(5, 100, 2)] int payloadLength)
        {
            var packet = new UdpPacket((uint)packetType, (uint)packetCounter, (uint)payloadLength);
            for (var index = 0; index < 5; index++)
            {
                packet[index] = 10 + index;
            }
            var iterator = new UdpPacketIterator(packet);

            Assert.That(iterator.Next().Data, Is.EqualTo(packet[0].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[1].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[2].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[3].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[4].Data));

            iterator.First();
            
            // Repeat after First
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[1].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[2].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[3].Data));
            Assert.That(iterator.Next().Data, Is.EqualTo(packet[4].Data));
        }

        [Test]
        public void Next_ThrowsAfterLastPayloadField()
        {
            var iterator = new UdpPacketIterator(new UdpPacket(1, 2, 1));

            Assert.That(() => iterator.Next(), Throws.Nothing);
            Assert.That(() => iterator.Next(), Throws.TypeOf<ArgumentOutOfRangeException>());
        }
    }
}
