using Empyrean.Common.Infra.Networking.Udp;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Infra.GryphonBoard
{
    public class GcbSecretKeySessionAuthentication : IGcbSessionAuthentication
    {
        private const uint secretKey = 0x12345678;
        private readonly GcbSession session;
        public GcbSecretKeySessionAuthentication(GcbSession session)
        {
            this.session = session;
        }

        public UdpPacket Sign(UdpPacket packet)
        {
            int authPosition;
            uint authenticationSignature;
            CalcSignatureAndPosition(packet, sessionKey: session.Id, out authPosition, out authenticationSignature);

            // Sign packet:
            packet[authPosition] = authenticationSignature;
            return packet.UpdateCRC();
        }

        public bool VerifySignature(UdpPacket packet)
        {
            int authPosition;
            uint code;
            CalcSignatureAndPosition(packet, sessionKey: session.Id, out authPosition, out code);

            // Check packet signature
            return packet[authPosition] == code;
        }

        private static void CalcSignatureAndPosition(
            UdpPacket packet,
            uint sessionKey,
            out int authPosition,
            out uint authenticationSignature)
        {
            authPosition = (int)packet.PayloadLength - 1;
            long longCode = secretKey + sessionKey + packet.PacketType;
            for (int i = 0; i < authPosition; i++)
            {
                longCode += (uint)packet[i];
            }
            authenticationSignature = (uint)longCode;
        }
    }
}
