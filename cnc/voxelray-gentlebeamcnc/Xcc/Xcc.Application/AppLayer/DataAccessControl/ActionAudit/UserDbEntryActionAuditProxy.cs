using System.Threading.Tasks;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Application.AppLayer.DataAccessControl.ActionAudit
{
    public class UserDbEntryActionAuditProxy<TData>(
        IActionAuditService actionAuditService, 
        IAsyncСRUDCommands<TData> actualCommands,
        string dataTypeNameAlias) : IAsyncСRUDCommands<TData>
        where TData : class, IEntry
    {
        protected const string ActionDetailsDone = "done";

        public IActionAuditService ActionAuditService { get; } = actionAuditService;

        #region IAsyncСRUDCommands<TData>
        public async Task<TData> CreateAsync(TData entry)
        {
            var result = await actualCommands.CreateAsync(entry);
            if (!BaseEntry.IsNullOrBlankEntry(result))
            {
                ActionAuditService.RegisterAction(
                    $"Create a new {dataTypeNameAlias}", $"record {GetRecordInfoString(result)}");
            }
            return result;
        }

        public Task<TData> ReadAsync(long entryId)
        {
            return actualCommands.ReadAsync(entryId);
        }

        public async Task<TData> UpdateAsync(TData oldEntry, TData newEntry)
        {
            var previousEntry = oldEntry ?? await actualCommands.ReadAsync(newEntry.Id);
            var hasRequestedChanges = GenericExtensions.CompareProperties(
                previousEntry, newEntry, toSnakeCase: false)?.Count > 0;

            var result = await actualCommands.UpdateAsync(oldEntry, newEntry);
            if (hasRequestedChanges &&
                GenericExtensions.CompareProperties(previousEntry, result, toSnakeCase: false)?.Count > 0)
            {
                ActionAuditService.RegisterAction(
                    $"Update {dataTypeNameAlias} {GetRecordInfoString(result)}", ActionDetailsDone);
            }
            return result;
        }
        public async Task<bool> DeleteAsync(long entryId)
        {
            var result = await actualCommands.DeleteAsync(entryId);
            if (result)
            {
                ActionAuditService.RegisterAction(
                    $"Delete {dataTypeNameAlias} {GetRecordInfoString(entryId)}", ActionDetailsDone);
            }
            return result;
        }
        #endregion IAsyncСRUDCommands<TData>

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
    }
}
