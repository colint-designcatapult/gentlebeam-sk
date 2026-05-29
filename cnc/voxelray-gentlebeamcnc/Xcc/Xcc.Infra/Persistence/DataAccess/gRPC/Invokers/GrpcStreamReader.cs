using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xcc.Core.Infra.gRPC;

namespace Xcc.Infra.Persistence.DataAccess.gRPC.Invokers
{
    public class GrpcStreamReader<TResponse> : IDataStreamReader<TResponse>
        where TResponse : class
    {
        public GrpcStreamReader(
            AsyncServerStreamingCall<TResponse> asyncStreamingCall,
            CancellationToken cancellationToken)
        {
            _asyncStreamReader = asyncStreamingCall.ResponseStream;
            _asyncStreamingCall = asyncStreamingCall;
            CancellationToken = cancellationToken;
        }

        public CancellationToken CancellationToken { get; }

        public async Task<TResponse> ReceiveAsync()
        {
            if (_asyncStreamReader == null)
            {
                throw new ObjectDisposedException("GrpcStreamReader error: ReceiveAsync method call on a disposed object");
            }
            await _asyncStreamReader.MoveNext(CancellationToken);
            return _asyncStreamReader.Current;
        }

        public void Dispose()
        {
            _asyncStreamingCall.Dispose();
        }

        private readonly IAsyncStreamReader<TResponse> _asyncStreamReader;
        private readonly AsyncServerStreamingCall<TResponse> _asyncStreamingCall;
    }
}
