using Com.Empyreanmed.Heracles.Auth.V1;
using Grpc.Core;
using System;
using System.Threading.Tasks;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Heracles.Application.Commands.gRPC.Common
{
    public class GrpcAuthCommands : IAuthCommands
    {
        public GrpcAuthCommands(IGrpcChannelManager grpcSettings)
        {
            GrpcSettings = grpcSettings;
        }

        public IGrpcChannelManager GrpcSettings { get; }

        public async Task<string> AuthenticateUserAsync(string username, string password)
        {
            var client = new AuthService.AuthServiceClient(GrpcSettings.Channel);

            var callOptions = new CallOptions(deadline: GrpcSettings.GetRpcDeadline());
            var response = await client.LoginAsync(new LoginRequest { Username = username, Password = password }, callOptions);

            if (!response.HasJwtToken)
            {
                throw new Exception("AuthenticationError: no bearer token received");
            }
            return response.JwtToken;
        }
    }
}
