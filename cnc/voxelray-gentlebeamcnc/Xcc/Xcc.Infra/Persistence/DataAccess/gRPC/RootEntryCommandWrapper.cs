using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Infra.Persistence.DataAccess.gRPC
{
    public abstract class RootEntryCommandWrapper<TClientType, TProtoType, InvokerType>
        : AbstractDataCommandWrapper<TClientType, TProtoType, InvokerType>
        , IAsyncRootEntryCommands<TClientType>
        where TProtoType : class
        where TClientType : class, IEntry
        where InvokerType : class, IAsyncRootEntryCommands<TProtoType>
    {
        protected RootEntryCommandWrapper(
            InvokerType invoker,
            Func<TClientType, TProtoType> toProtos,
            Func<TProtoType, TClientType> fromProtos)
            : base(invoker, toProtos, fromProtos)
        {
        }

        public async Task<ICollection<TClientType>> ReadAllAsync()
        {
            try
            {
                var list = await Invoker.ReadAllAsync();
                ICollection<TClientType> result = new List<TClientType>();
                foreach (var item in list)
                {
                    result.Add(ConvertFromProto(item));
                }
                return result;
            }
            catch (Exception e)
            {
                string msg = $"Failed to get list of all {typeof(TProtoType).Name} entries";
                throw new DataServiceException(msg, e);
            }
        }
    }
}
