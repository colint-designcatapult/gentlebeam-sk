using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Infra.Persistence.DataAccess.gRPC
{
    public abstract class ChildEntryCommandWrapper<TClientType, TProtoType, InvokerType>
        : AbstractDataCommandWrapper<TClientType, TProtoType, InvokerType>
        , IAsyncChildEntryCommands<TClientType>
        where TProtoType : class
        where TClientType : class, IEntry
        where InvokerType : class, IAsyncChildEntryCommands<TProtoType>
    {
        protected ChildEntryCommandWrapper(
            InvokerType invoker,
            Func<TClientType, TProtoType> toProtos,
            Func<TProtoType, TClientType> fromProtos)
            : base(invoker, toProtos, fromProtos)
        {
        }

        public async Task<ICollection<TClientType>> ReadListAsync(long parentId)
        {
            try
            {
                var list = await Invoker.ReadListAsync(parentId);
                ICollection<TClientType> result = new List<TClientType>();
                foreach (var item in list)
                {
                    result.Add(ConvertFromProto(item));
                }
                return result;
            }
            catch (Exception e)
            {
                string msg = $"Failed to get list of {typeof(TProtoType).Name} entries by parent id={parentId}";
                throw new DataServiceException(msg, e);
            }
        }
    }
}
