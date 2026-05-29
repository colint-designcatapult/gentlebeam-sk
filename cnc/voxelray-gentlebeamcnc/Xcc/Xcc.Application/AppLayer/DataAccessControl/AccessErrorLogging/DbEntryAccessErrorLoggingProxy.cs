using System;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;

namespace Xcc.Application.AppLayer.DataAccessControl.AccessErrorLogging
{
    public class DbEntryAccessErrorLoggingProxy<TData>(
        ILogWriter logWriter,
        IAsyncСRUDCommands<TData> actualCommands,
        string dataTypeNameAlias) : IAsyncСRUDCommands<TData>
        where TData : class, IEntry
    {
        #region IAsyncСRUDCommands<TData>
        public Task<TData> CreateAsync(TData entry)
        {
            string actionDescription = $"create a new {dataTypeNameAlias}";
            return AwaitForTaskWithExceptionLogging(actualCommands.CreateAsync(entry), actionDescription);
        }

        public Task<TData> ReadAsync(long entryId)
        {
            string actionDescription = $"read {dataTypeNameAlias} data";
            return AwaitForTaskWithExceptionLogging(actualCommands.ReadAsync(entryId), actionDescription);
        }

        public Task<TData> UpdateAsync(TData oldEntry, TData newEntry)
        {
            string actionDescription = $"update {dataTypeNameAlias} {GetRecordInfoString(newEntry)}";
            return AwaitForTaskWithExceptionLogging(actualCommands.UpdateAsync(oldEntry, newEntry), actionDescription);
        }

        public Task<bool> DeleteAsync(long entryId)
        {
            string actionDescription = $"delete {dataTypeNameAlias} {GetRecordInfoString(entryId)}";
            return AwaitForTaskWithExceptionLogging(actualCommands.DeleteAsync(entryId), actionDescription);
        }
        #endregion IAsyncСRUDCommands<TData>

        #region Protected methods
        #region Virtual methods
        protected virtual string GetRecordInfoString(long entryId)
        {
            return $"id={entryId}";
        }

        protected virtual string GetRecordInfoString(TData entry)
        {
            return GetRecordInfoString(entry.Id);
        }
        #endregion Virtual methods
        protected async Task<TResult> AwaitForTaskWithExceptionLogging<TResult>(Task<TResult> task, string actionDescription)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                string actionMessage = $"Data Access Error. Failed to {actionDescription}. {ex.Message}. {ex.InnerException?.Message}";
                _ = logWriter.LogAsync(actionMessage, Core.Enums.LogRecordSeverity.Error, Core.Enums.LogRecordType.System);
                throw;
            }
        }
        #endregion Private methods
    }
}
