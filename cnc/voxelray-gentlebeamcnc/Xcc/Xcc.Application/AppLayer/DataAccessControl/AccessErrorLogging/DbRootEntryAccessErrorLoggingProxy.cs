using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;

namespace Xcc.Application.AppLayer.DataAccessControl.AccessErrorLogging
{
    public class DbRootEntryAccessErrorLoggingProxy<TData>(
        ILogWriter logWriter,
        IAsyncRootEntryCommands<TData> actualCommands,
        string dataTypeNameAlias)
        : DbEntryAccessErrorLoggingProxy<TData>(logWriter, actualCommands, dataTypeNameAlias)
        , IAsyncRootEntryCommands<TData>
    where TData : class, IEntry
    {
        public Task<ICollection<TData>> ReadAllAsync()
        {
            return AwaitForTaskWithExceptionLogging(
                actualCommands.ReadAllAsync(),
                $"read all {dataTypeNameAlias} records");
        }
    }
}
