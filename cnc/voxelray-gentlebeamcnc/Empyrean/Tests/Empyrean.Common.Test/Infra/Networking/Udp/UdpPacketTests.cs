using Empyrean.Common.Infra.Networking.Udp;

namespace Empyrean.Common.Test.Infra.Networking.Udp
{
    internal class UdpPacketTests
    {
        [Test]
        public void UdpPacketField_ConstructorTest()
        {
            Assert.DoesNotThrow(() => new UdpPacket.Field((int)1));
            Assert.DoesNotThrow(() => new UdpPacket.Field((uint)1));
            Assert.DoesNotThrow(() => new UdpPacket.Field((float)1.0));
            Assert.DoesNotThrow(() => new UdpPacket.Field(new byte[4] { 0, 1, 2, 3 }));
        }

        [Test]
        public void UdpPacketField_BackConversionTest()
        {
            var floatField = new UdpPacket.Field(1.0f);
            Assert.That((float)floatField, Is.EqualTo(1.0f));

            var intField = new UdpPacket.Field(42);
            Assert.That((int)intField, Is.EqualTo(42));

            var uintField = new UdpPacket.Field(UInt32.MaxValue);
            Assert.That((uint)uintField, Is.EqualTo(UInt32.MaxValue));

            var byteValue = new byte[4] { 1, 2, 3, 4 };
            var byteField = new UdpPacket.Field(byteValue);
            Assert.That(byteField.Data, Is.EqualTo(byteValue));
        }

        [Test]
        public void UdpPacketField_ListInitializer_ImplicitConversionTest()
        {
            int intValue = 42;
            float floatValue = 1.0f;
            byte[] byteValue = BitConverter.GetBytes(2.0f);
            List<UdpPacket.Field> fields = new() { floatValue, intValue, byteValue };
            Assert.Multiple(() =>
            {
                Assert.That((float)fields[0], Is.EqualTo(floatValue));
                Assert.That((int)fields[1], Is.EqualTo(intValue));
                Assert.That((byte[])fields[2], Is.EqualTo(byteValue));
            });
        }

        [Test]
        public void ConstructorTest()
        {
            UdpPacket? packet = null;
            Assert.DoesNotThrow(() => packet = new UdpPacket(0, 0, 0));
            Assert.That(packet, Is.Not.Null);
            Assert.That(packet.Buffer, Is.Not.Null);
            Assert.That(packet.Buffer.Count, Is.EqualTo(4*6)); // no payload, so just header + crc
        }

        [Test]
        public void ConstructorFromBufferTest()
        {
            UdpPacket packet = new (0, 1, 2);
            packet[0] = 1; packet[1] = 2.0f; 
            packet.UpdateCRC();

            UdpPacket? parsedPacket = null;
            Assert.DoesNotThrow(() => parsedPacket = new UdpPacket(packet.Buffer));
        }

        [Test]
        public void ConstructorFromBuffer_NegativeTest()
        {
            byte[] shortBuffer = new byte[10];
            
            UdpPacket packet = new(0, 1, 2);
            packet[0] = 1; packet[1] = 2.0f;
            packet.UpdateCRC();
            
            byte[] incompleteBuffer = packet.Buffer.Take(packet.Buffer.Length - 1).ToArray();
            
            byte[] wrongCrcBuffer = (byte[])packet.Buffer.Clone();
            wrongCrcBuffer[0] ^= 0x1;

            Assert.Throws<ArgumentNullException>(() => new UdpPacket(null!));

            Assert.Throws<ArgumentException>(() => new UdpPacket(shortBuffer));
            Assert.Throws<ArgumentException>(() => new UdpPacket(incompleteBuffer));
            Assert.Throws<ArgumentException>(() => new UdpPacket(wrongCrcBuffer));
        }

        [Test]
        public void GetPacketTypeTest()
        {
            uint packetType = 42;
            UdpPacket packet = new(packetType, 0, 0);
            Assert.That(packet.PacketType, Is.EqualTo(packetType));
        }

        [Test]
        public void GetPacketCounterTest()
        {
            uint packetCounter = 42;
            UdpPacket packet = new(0, packetCounter, 0);
            Assert.That(packet.PacketCounter, Is.EqualTo(packetCounter));
        }

        [Test]
        public void GetPacketPayloadSizeTest()
        {
            uint packetPayloadSize = 7;
            UdpPacket packet = new(0, 0, packetPayloadSize);
            Assert.That(packet.PayloadLength, Is.EqualTo(packetPayloadSize));
        }

        [Test]
        public void GetCRCTest()
        {
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 0);
            uint crc = CrcUtils.ComputeChecksum(packet.Buffer.Take(packet.Buffer.Length - 4).ToArray());
            Assert.That(packet.CRC, Is.EqualTo(crc));
        }

        [Test]
        public void PayloadFieldIndexerTest()
        {
            var field1 = BitConverter.GetBytes(1);
            var field2 = BitConverter.GetBytes(2.0f);
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 2);
            Assert.DoesNotThrow(() => packet[0] = field1);
            Assert.DoesNotThrow(() => packet[1] = field2);
        }

        [Test]
        public void PayloadFieldIndexer_OutOfRangeTest()
        {
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => packet[1] = BitConverter.GetBytes(0.0f));
        }

        [Test]
        public void PayloadFieldIndexer_InvalidDataSize_Test()
        {
            const double doubleValue = 0.0;
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 1);
            Assert.Throws<ArgumentException>(() => packet[0] = BitConverter.GetBytes(doubleValue));
            // Index is out of range, but conversion correctness gets checked first anyway:
            Assert.Throws<ArgumentException>(() => packet[1] = BitConverter.GetBytes(doubleValue));
        }

        [Test]
        public void Set_PositiveTest()
        {
            var field1 = BitConverter.GetBytes(1);
            var field2 = BitConverter.GetBytes(2.0f);
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 2);
            Assert.DoesNotThrow(() => packet.Set(0, field1));
            Assert.DoesNotThrow(() => packet.Set(1, field2));
            Assert.That((byte[])packet[0], Is.EqualTo(field1));
            Assert.That((byte[])packet[1], Is.EqualTo(field2));
        }

        [Test]
        public void Set_OutOfRangeTest()
        {
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 1);
            Assert.Throws<ArgumentOutOfRangeException>(() => packet.Set(payloadField: 1, value: 42));
            Assert.Throws<ArgumentOutOfRangeException>(() => packet.Set(payloadField: -1, value: 42));
        }

        [Test]
        public void GetTest()
        {
            int field1 = 1;
            float field2 = 2.0f;
            // Checksum for empty field packets is computed at once:
            UdpPacket packet = new(packetType: 1, packetCounter: 42, payloadLength: 2);
            packet.Set(0, field1);
            packet.Set(1, field2);

            Assert.Multiple(() =>
            {
                Assert.That((int)packet[0], Is.EqualTo(field1));
                Assert.That((float)packet[1], Is.EqualTo(field2));
            });
        }

    }
}
