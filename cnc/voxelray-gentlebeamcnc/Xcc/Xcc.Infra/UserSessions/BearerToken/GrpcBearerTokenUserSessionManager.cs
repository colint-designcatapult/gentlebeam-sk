using Grpc.Core;
using System;
using System.Threading;
using Xcc.Core.Constants;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Xcc.Infra.UserSessions.BearerToken
{
    public interface IGrpcBearerTokenUserSessionManager 
        : IBearerTokenUserSessionManager, IGrpcMetadataSource
    {
    }

    public class GrpcBearerTokenUserSessionManager(CancellationToken globalCancellationToken)
    : BearerTokenUserSessionManager(globalCancellationToken)
    , IGrpcBearerTokenUserSessionManager
    {
        private Metadata? _headers = null;
        #region Properties
        public Metadata Headers { 
            get => _headers ?? throw new SessionAuthorizationException(StringConstants.Common.Authorization.SessionAuthExpirationError); 
            private set => _headers = value; 
        }
        #endregion Properties


        #region Private methods
        protected override void SetUserSession(BearerTokenUserSession userSession)
        {
            base.SetUserSession(userSession);
            SetupHeadersMetadata(userSession.AuthBearerToken);
        }

        private void SetupHeadersMetadata(string bearerToken)
        {
            Metadata? newHeaders = null;
            if (!string.IsNullOrEmpty(bearerToken))
            {
                newHeaders = new Metadata();
                newHeaders.Add(new Metadata.Entry("Authorization", $"Bearer {bearerToken}"));
            }
            _headers = newHeaders;
        }
        #endregion Private methods
    }

    [Serializable]
    public class SessionAuthorizationException : Exception
    {
        public SessionAuthorizationException()
        {
        }

        public SessionAuthorizationException(string? message) : base(message)
        {
        }

        public SessionAuthorizationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
