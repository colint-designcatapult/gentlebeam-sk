using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Exceptions;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Infra.Persistence.DataAccess.gRPC
{
    public abstract class AbstractDataCommandWrapper<TClientType, TProtoType, InvokerType> : IAsyncСRUDCommands<TClientType>
            where TProtoType : class
            where TClientType : class, IEntry
            where InvokerType : class, IAsyncСRUDCommands<TProtoType>
    {
        public AbstractDataCommandWrapper(
            InvokerType invoker,
            Func<TClientType, TProtoType> toProtos,
            Func<TProtoType, TClientType> fromProtos)
        {
            ConvertToProto = toProtos;
            ConvertFromProto = fromProtos;
            Invoker = invoker;
        }

        protected InvokerType Invoker { get; }
        protected Func<TClientType, TProtoType> ConvertToProto { get; }
        protected Func<TProtoType, TClientType> ConvertFromProto { get; }


        #region IAsyncСRUDCommands
        public async Task<TClientType> CreateAsync(TClientType entry)
        {
            try
            {
                TProtoType response = await Invoker.CreateAsync(ConvertToProto(entry));

                return ConvertFromProto(response);
            }
            catch (Exception e)
            {
                string msg = $"Failed to save a new {typeof(TProtoType).Name}";
                throw new DataServiceException(msg, e);
            }
        }

        public async Task<TClientType> ReadAsync(long id)
        {
            try
            {
                TProtoType response = await Invoker.ReadAsync(id);

                return ConvertFromProto(response);
            }
            catch (Exception e)
            {
                string msg = $"Failed to load a {typeof(TProtoType).Name}";
                throw new DataServiceException(msg, e);
            }
        }

        public async Task<TClientType> UpdateAsync(TClientType oldValue, TClientType newValue)
        {
            try
            {
                TProtoType? protosOldValue = oldValue == null ? null : ConvertToProto(oldValue);
                TProtoType protosNewValue = ConvertToProto(newValue);

                TProtoType response = await Invoker.UpdateAsync(protosOldValue!, protosNewValue);
                return ConvertFromProto(response);
            }
            catch (Exception e)
            {
                string msg = $"Failed to update {typeof(TProtoType).Name}";
                throw new DataServiceException(msg, e);
            }
        }

        public Task<bool> DeleteAsync(long entryId)
        {
            try
            {
                return Invoker.DeleteAsync(entryId);
            }
            catch (Exception e)
            {
                string msg = $"Failed to delete a {typeof(TProtoType).Name}";
                throw new DataServiceException(msg, e);
            }
        }

        #endregion IAsyncСRUDCommands
    }
}
