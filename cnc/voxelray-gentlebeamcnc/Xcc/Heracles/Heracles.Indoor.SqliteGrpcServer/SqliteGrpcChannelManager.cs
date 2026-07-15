using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using System;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Heracles.Indoor.SqliteGrpcServer;

/// <summary>
/// A <see cref="IGrpcChannelManager"/> that connects to the embedded
/// <see cref="SqliteGrpcServerHost"/> running on localhost.
/// </summary>
public sealed class SqliteGrpcChannelManager : IGrpcChannelManager
{
    private readonly GrpcChannel _channel;

    public SqliteGrpcChannelManager(int port = SqliteGrpcServerHost.DefaultPort)
    {
        var address = $"http://localhost:{port}";
        _channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        {
            Credentials = ChannelCredentials.Insecure,
        });
        Channel = _channel.CreateCallInvoker();
    }

    public CallInvoker? Channel { get; private set; }

    // No real auth headers needed for the embedded local server
    public Metadata Headers { get; } = new Metadata();

    // Long timeout — local in-process calls are instant but some ops (streaming) run long
    public uint RpcTimeoutMs { get; } = 30_000;

    public DateTime GetRpcDeadline(int timeoutMs = -1)
        => DateTime.UtcNow.AddMilliseconds(timeoutMs > 0 ? timeoutMs : RpcTimeoutMs);

    public void InterceptChannel(Interceptor interceptor)
    {
        if (Channel is null) throw new InvalidOperationException("Channel not initialised.");
        Channel = Channel.Intercept(interceptor);
    }

    public void ShutdownChannel()
        => _channel.ShutdownAsync().GetAwaiter().GetResult();
}
