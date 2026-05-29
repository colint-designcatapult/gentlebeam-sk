using Google.Protobuf;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Xcc.Infra.Persistence.DataAccess.gRPC.Invokers
{
    public abstract class AbstractChildGrpcInvoker<TProtoType> : AbstractGrpcInvoker<TProtoType>, IAsyncChildEntryCommands<TProtoType>
        where TProtoType : class, IMessage<TProtoType>, new()
    {
        protected AbstractChildGrpcInvoker(IGrpcChannelManager grpcSettings) : base(grpcSettings)
        {
        }

        public abstract Task<ICollection<TProtoType>> ReadListAsync(long parentId);
    }
}
