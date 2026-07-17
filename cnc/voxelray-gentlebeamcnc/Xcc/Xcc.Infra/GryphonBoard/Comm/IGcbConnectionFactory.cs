using Empyrean.Common.Infra.Networking;

namespace Xcc.Infra.GryphonBoard.Comm;

public interface IGcbCommandConnectionFactory
{
    IAsyncClientConnection GetGcbCommandConnection();
}

public interface IGcbTelemetryConnectionFactory
{
    IAsyncClientConnection GetGcbTelemetryConnection();
}
