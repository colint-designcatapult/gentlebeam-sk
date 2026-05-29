using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using System.Threading.Tasks;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Infra.Networking.gRPC.Channels;

namespace Xcc.Infra.Persistence.DataAccess.gRPC.Invokers
{
    public abstract class AbstractGrpcInvoker<TProtoType> : AbstractBaseGrpcInvoker<TProtoType>, IAsyncСRUDCommands<TProtoType>
            where TProtoType : class, IMessage<TProtoType>, new()
    {
        protected AbstractGrpcInvoker(IGrpcChannelManager grpcSettings)
            :base(grpcSettings) { }

        public abstract Task<TProtoType> CreateAsync(TProtoType entry);
        public abstract Task<TProtoType> ReadAsync(long entryId);
        public abstract Task<TProtoType> UpdateAsyncWithMask(TProtoType entry, FieldMask mask);
        public abstract Task<bool> DeleteAsync(long entryId);
        public Task<TProtoType> UpdateAsync(TProtoType oldEntry, TProtoType newEntry)
        {
            return UpdateAsyncWithMask(newEntry, GetMask(oldEntry, newEntry));
        }
    }
}
