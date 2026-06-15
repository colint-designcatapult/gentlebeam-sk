using Empyrean.Common.Infra.Networking.Udp;

namespace Empyrean.Common.Test.Infra.Networking.Udp
{
    internal class UdpPacketBuilderTests
    {
        [Test]
        public void ConstructorTest()
        {
            UdpPacketBuilder? builder = null;
            Assert.DoesNotThrow(() => builder = new (packetType: 0, packetCounter: 1, payloadFields: 2));

            var packet = builder?.Packet;
            Assert.That(packet, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(packet.PacketType, Is.EqualTo(0));
                Assert.That(packet.PacketCounter, Is.EqualTo(1));
                Assert.That(packet.PayloadLength, Is.EqualTo(2));
            });
        }
        
        [Test]
        public void AddTest()
        {
            const int intValue = 0xf00d;
            const float floatValue = 1.0f;
            byte[] byteValue = { 0x01, 0x02, 0x03, 0x04 };
            
            UdpPacketBuilder builder = new(packetType: 0, packetCounter: 1, payloadFields: 3);
            builder.Add(intValue);
            builder.Add(floatValue);
            builder.Add(byteValue);

            Assert.That((int)builder.Packet[0], Is.EqualTo(intValue));
            Assert.That((float)builder.Packet[1], Is.EqualTo(floatValue));
            Assert.That((byte[])builder.Packet[2], Is.EqualTo(byteValue));
        }

        [Test]
        public void Check_NegativeTest()
        {
            UdpPacketBuilder builder = new(packetType: 0, packetCounter: 1, payloadFields: 1);

            // Field is not set, should throw:
            Assert.Throws<Exception>(() => builder.Check());
        }

        [Test]
        public void Check_PositiveTest()
        {
            UdpPacketBuilder builder = new(packetType: 0, packetCounter: 1, payloadFields: 1);
            builder.Add(0);

            // Field is not set, should throw:
            Assert.DoesNotThrow(() => builder.Check());
        }

        [Test]
        public void SignTest()
        {
            UdpPacketBuilder builder = new(packetType: 0, packetCounter: 1, payloadFields: 1);
            builder.Add(1);

            // Field is not set, should throw:
            Assert.DoesNotThrow(() => builder.Sign());
            var crc = builder.Packet.CRC;
            var calculatedCRC = CrcUtils.ComputeChecksum(builder.Buffer.Take(builder.Buffer.Length - 4).ToArray());

            Assert.That(crc, Is.EqualTo(calculatedCRC));
        }

        [Test]
        public void BuildPacketTest()
        {
            const int intValue = 0xf00d;
            const float floatValue = 1.0f;
            byte[] byteValue = { 0x01, 0x02, 0x03, 0x04 };

            UdpPacket? packet = null;
                
            Assert.DoesNotThrow(() => packet = UdpPacketBuilder.BuildPacket(packetType: 0, packetCounter: 1, payload: [intValue, floatValue, byteValue]));

            Assert.That(packet, Is.Not.Null);
            Assert.That((int)packet[0], Is.EqualTo(intValue));
            Assert.That((float)packet[1], Is.EqualTo(floatValue));
            Assert.That((byte[])packet[2], Is.EqualTo(byteValue));
        }

        [Test]
        public void BuildRawPacketTest()
        {
            const int intValue = 0xf00d;
            const float floatValue = 1.0f;
            byte[] byteValue = { 0x01, 0x02, 0x03, 0x04 };

            // Build reference packet step by step:
            UdpPacketBuilder builder = new(packetType: 0, packetCounter: 1, payloadFields: 3);
            builder.Add(intValue);
            builder.Add(floatValue);
            builder.Add(byteValue);
            builder.Sign();
            var referencePacket = builder.Buffer;

            // Check that with BuildRawPacket we get the same result:
            byte[] packet = UdpPacketBuilder.BuildRawPacket(packetType: 0, packetCounter: 1, payload: [intValue, floatValue, byteValue]);

            Assert.That(packet, Is.EqualTo(referencePacket));
        }
    }
}
