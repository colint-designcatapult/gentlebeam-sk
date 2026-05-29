using Empyrean.Common.Infra.Networking.Udp;

namespace Xcc.Infra.GryphonBoard
{
    public interface IGcbSessionAuthentication
    {
        public UdpPacket Sign(UdpPacket packet);
        public bool VerifySignature(UdpPacket packet);
    }
}
