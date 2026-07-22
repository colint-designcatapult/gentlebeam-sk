using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Threading;
using Xcc.Core.Models;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Infra.Networking.gRPC.Channels
{
    public class GrpcChannelManager : IGrpcChannelManager
    {
        public GrpcChannelManager(
            ICoreSettings coreSettings,
            IGrpcBearerTokenUserSessionManager sessionManager, 
            CancellationToken globalCancellationToken)
        {
            Hostname = coreSettings.DataCommandsEndPoint.Ip();
            Port = coreSettings.DataCommandsEndPoint.Port ?? throw new Exception("Data commands endpoint port is not specified.");
            _useInsecureGrpc = coreSettings.UseInsecureGrpc;

            if (coreSettings.GrpcTimeout > 0)
            {
                RpcTimeoutMs = (uint)coreSettings.GrpcTimeout;
            }

            _sessionManager = sessionManager;
            Setup();
        }


        #region Properties
        public uint RpcTimeoutMs { get; } = 5000;
        public CallInvoker? Channel { get; private set; }
        public string Hostname { get; }
        public int Port { get; }
        public Metadata Headers => _sessionManager.Headers;
        #endregion Properties


        #region Private fields
        private readonly IGrpcBearerTokenUserSessionManager _sessionManager;
        private readonly bool _useInsecureGrpc;

        private GrpcChannel? _channel;         // Actual channel we hide from modification
        #endregion Private fields


        #region Public methods
        public void ShutdownChannel()
        {
            _sessionManager.CloseUserSession();
            _channel?.ShutdownAsync();
        }

        public DateTime GetRpcDeadline(int timeoutMs = -1)
        {
            return DateTime.UtcNow.AddMilliseconds(timeoutMs > 0 ? timeoutMs : RpcTimeoutMs);
        }

        public void Setup()
        {
            if (_channel != null || Channel != null)
            {
                throw new NullReferenceException("GrpcSettings setup error: channel already exists");
            }

            bool isLoopback = Hostname is "localhost" or "127.0.0.1" or "::1";
            bool useInsecure = true; // always use insecure -- now always on lan lan
            var scheme = useInsecure ? "http" : "https";
            var address = $"{scheme}://{Hostname}:{Port}";

            GrpcChannelOptions channelOptions;
            if (useInsecure)
            {
                // Plain HTTP/2 — no TLS (embedded SQLite server or explicit opt-in via UseInsecureGrpc)
                channelOptions = new GrpcChannelOptions
                {
                    Credentials = ChannelCredentials.Insecure
                };
            }
            else
            {
                var httpClientHandler = new HttpClientHandler();
                // Return `true` to allow certificates that are untrusted/invalid
                httpClientHandler.ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator; // todo: untrusted certificate

                var httpClient = new HttpClient(httpClientHandler);
                httpClient.Timeout = Timeout.InfiniteTimeSpan; // The gRPC default is Infinite, so we set it to our custom client as well

                channelOptions = new GrpcChannelOptions { HttpClient = httpClient };
            }

            _channel = GrpcChannel.ForAddress(address, channelOptions);
            Channel = _channel.CreateCallInvoker();
        }

        public void InterceptChannel(Interceptor interceptor)
        {
            if (_channel == null || Channel == null)
            {
                throw new NullReferenceException("GrpcSettings interception error: channel doesn't exist");
            }
            
            if (interceptor == null)
            {
                throw new NullReferenceException("GrpcSettings interception error: no interceptor to set");
            }
            
            Channel = Channel.Intercept(interceptor);
        }
        #endregion Public methods
    }
}
