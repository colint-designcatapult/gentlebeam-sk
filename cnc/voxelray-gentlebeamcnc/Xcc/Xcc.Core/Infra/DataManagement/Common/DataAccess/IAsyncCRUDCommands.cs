using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xcc.Core.Infra.DataManagement.Common.DataAccess
{
    public interface IAsyncСRUDCommands<TData>
    {
        Task<TData> CreateAsync(TData entry);
        Task<TData> ReadAsync(long entryId);
        Task<TData> UpdateAsync(TData oldEntry, TData newEntry);
        Task<bool> DeleteAsync(long entryId);
    }

    public interface IAsyncRootEntryCommands<TData> : IAsyncСRUDCommands<TData>
    {
        Task<ICollection<TData>> ReadAllAsync();
    }

    public interface IAsyncChildEntryCommands<TData> : IAsyncСRUDCommands<TData>
    {
        Task<ICollection<TData>> ReadListAsync(long parentId);
    }

    public interface IAsyncApprovalCommands<TData>
    {
        Task<TData> ApproveAsync(long entryId, string username, string password);
    }
}
