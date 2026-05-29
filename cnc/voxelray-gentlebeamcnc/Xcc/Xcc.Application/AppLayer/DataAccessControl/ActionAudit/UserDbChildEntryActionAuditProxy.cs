using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Application.AppLayer.DataAccessControl.ActionAudit
{
    public abstract class UserDbChildEntryActionAuditProxy<TData>(
        IActionAuditService actionAuditService,
        IAsyncChildEntryCommands<TData> actualCommands,
        string dataTypeNameAlias,
        string parentDataAliasType)
        : UserDbEntryActionAuditProxy<TData>(actionAuditService, actualCommands, dataTypeNameAlias)
        , IAsyncChildEntryCommands<TData>
        where TData : class, IEntry
    {
        public virtual Task<ICollection<TData>> ReadListAsync(long parentId)
        {
            return actualCommands.ReadListAsync(parentId);
        }

        protected override string GetRecordInfoString(TData entry)
        {
            return $"id={entry.Id} ({parentDataAliasType} id={GetParentId(entry)})";
        }

        protected abstract long GetParentId(TData entry);
    }
}
