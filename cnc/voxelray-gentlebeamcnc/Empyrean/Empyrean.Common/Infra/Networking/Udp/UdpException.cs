namespace Empyrean.Common.Infra.Networking.Udp;

public class UdpException : Exception
{ 
    public UdpException(string message)
        : base(message)
    {
    }

    public UdpException(string message, Exception inner)
        : base(message, inner)
    {
    }
}