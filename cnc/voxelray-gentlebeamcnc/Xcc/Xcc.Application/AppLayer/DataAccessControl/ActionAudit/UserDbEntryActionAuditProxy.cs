using System;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Service;
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
        protected const string ActionDetailsFailed = "failed";

        public IActionAuditService ActionAuditService { get; } = actionAuditService;

        #region IAsyncСRUDCommands<TData>
        public async Task<TData> CreateAsync(TData entry)
        {
            string actionMessage = $"Create a new {dataTypeNameAlias}";
            try
            {
                var result = await actualCommands.CreateAsync(entry);
                ActionAuditService.RegisterAction(actionMessage, $"record {GetRecordInfoString(result)}");
                return result;
            }
            catch (Exception)
            {
                ActionAuditService.RegisterAction(actionMessage, ActionDetailsFailed);
                throw;
            }
        }

        public Task<TData> ReadAsync(long entryId)
        {
            return actualCommands.ReadAsync(entryId);
        }

        public async Task<TData> UpdateAsync(TData oldEntry, TData newEntry)
        {
            string actionMessage = $"Update {dataTypeNameAlias} {GetRecordInfoString(newEntry)}";
            try
            {
                var result = await actualCommands.UpdateAsync(oldEntry, newEntry);
                ActionAuditService.RegisterAction(actionMessage, ActionDetailsDone);
                return result;
            }
            catch (Exception)
            {
                ActionAuditService.RegisterAction(actionMessage, ActionDetailsFailed);
                throw;
            }

        }
        public async Task<bool> DeleteAsync(long entryId)
        {
            string actionMessage = $"Delete {dataTypeNameAlias} {GetRecordInfoString(entryId)}";
            try
            {
                var result = await actualCommands.DeleteAsync(entryId);
                ActionAuditService.RegisterAction(actionMessage, ActionDetailsDone);
                return result;
            }
            catch (Exception)
            {
                ActionAuditService.RegisterAction(actionMessage, ActionDetailsFailed);
                throw;
            }
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
