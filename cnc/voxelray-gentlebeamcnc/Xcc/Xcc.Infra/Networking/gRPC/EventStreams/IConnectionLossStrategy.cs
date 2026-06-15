namespace Xcc.Infra.Networking.gRPC.EventStreams
{
    public interface IConnectionLossStrategy
    {
        void OnDisconnect();
        void OnConnect();
        bool CanConnect { get; }
        bool Disconnected { get; }
    }
}
