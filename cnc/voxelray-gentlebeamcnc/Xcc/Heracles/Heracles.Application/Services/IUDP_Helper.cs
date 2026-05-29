namespace Heracles.Application.Services
{
    public interface IUDP_Helper
    {
        public void Initialize(string hostName, int port);
        public void Send(byte[] txBuff);
    }
}
