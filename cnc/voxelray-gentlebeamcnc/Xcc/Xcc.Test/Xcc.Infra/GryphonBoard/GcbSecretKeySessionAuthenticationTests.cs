using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;
using Xcc.Infra.GryphonBoard;

namespace Xcc.Test.Xcc.Infra.GryphonBoard
{
    internal class GcbSecretKeySessionAuthenticationTests
    {
        readonly GcbSession SESSION = new GcbSession(id: 42, totalPoints: 2);
        const int PACKET_ID_VALUE = 1;
        const int SOME_PAYLOAD_VALUE = 2;

        private IGcbSessionAuthentication auth;
        private UdpPacket somePacket;

        [SetUp]
        public void Setup()
        {
            auth = new GcbSecretKeySessionAuthentication(SESSION);
            somePacket = UdpPacketBuilder.BuildPacket(
                           (uint)GCBPacketType.OperationalPointLoadingCmd,
                           packetCounter: PACKET_ID_VALUE,
                           payload: [SOME_PAYLOAD_VALUE, 0]);
        }

        [Test]
        public void SignTest()
        {
            UdpPacket originalPacket = somePacket;
            UdpPacket packetToSign = new UdpPacket((byte[])somePacket.Buffer.Clone());
            auth.Sign(packetToSign);
            Assert.Multiple(() =>
            {
                Assert.That(originalPacket.Buffer, Is.Not.EquivalentTo(packetToSign.Buffer));
                Assert.That(originalPacket[1], Is.Not.EqualTo(packetToSign[1]));
            });
        }

        [Test]
        public void VerifyTest()
        {
            UdpPacket originalPacket = somePacket;
            UdpPacket signedPacket = auth.Sign(new UdpPacket((byte[])somePacket.Buffer.Clone()));

            Assert.Multiple(() =>
            {
                Assert.That(auth.VerifySignature(originalPacket), Is.False);
                Assert.That(auth.VerifySignature(signedPacket), Is.True);
            });
        }
    }
}
