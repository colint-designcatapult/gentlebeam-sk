using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Application.AppLayer.Service;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;

namespace Xcc.Application.AppLayer.DataAccessControl.ActionAudit
{
    public class UserDbRootEntryActionAuditProxy<TData>(
        IActionAuditService actionAuditService,
        IAsyncRootEntryCommands<TData> actualCommands,
        string dataTypeNameAlias) 
        : UserDbEntryActionAuditProxy<TData>(actionAuditService, actualCommands, dataTypeNameAlias)
        , IAsyncRootEntryCommands<TData>
        where TData : class, IEntry
    {
        public Task<ICollection<TData>> ReadAllAsync()
        {
            return actualCommands.ReadAllAsync();
        }
    }
}
