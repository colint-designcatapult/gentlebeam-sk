using Google.Protobuf;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Xcc.Infra.Persistence.DataAccess.gRPC.Invokers
{
    public abstract class AbstractRootGrpcInvoker<TProtoType> : AbstractGrpcInvoker<TProtoType>, IAsyncRootEntryCommands<TProtoType>
        where TProtoType : class, IMessage<TProtoType>, new()
    {
        protected AbstractRootGrpcInvoker(IGrpcChannelManager grpcSettings)
            : base(grpcSettings)
        {
        }

        public abstract Task<ICollection<TProtoType>> ReadAllAsync();
    }
}
