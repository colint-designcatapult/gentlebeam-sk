using Grpc.Core;

namespace Xcc.Infra.Networking.gRPC.Channels
{
    public interface IGrpcMetadataSource
    {
        Metadata Headers { get; }
    }
}
