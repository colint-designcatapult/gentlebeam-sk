using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;

namespace Xcc.Application.AppLayer.DataAccessControl.AccessErrorLogging
{
    public class DbChildEntryAccessErrorLoggingProxy<TData>(
        ILogWriter logWriter,
        IAsyncChildEntryCommands<TData> actualCommands,
        string dataTypeNameAlias,
        string parentDataAliasType)
        : DbEntryAccessErrorLoggingProxy<TData>(logWriter, actualCommands, dataTypeNameAlias)
        , IAsyncChildEntryCommands<TData>
        where TData : class, IEntry
    {
        public virtual Task<ICollection<TData>> ReadListAsync(long parentId)
        {
            return AwaitForTaskWithExceptionLogging(
                actualCommands.ReadListAsync(parentId),
                $"read all {parentDataAliasType} records for {parentDataAliasType} id={parentId}");
        }
    }
}
