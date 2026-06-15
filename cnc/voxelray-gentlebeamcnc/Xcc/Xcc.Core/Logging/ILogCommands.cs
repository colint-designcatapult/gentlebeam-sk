using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xcc.Core.Logging
{
    public struct LogPage<TLogType>
    {
        public ICollection<TLogType> records;
        public string nextPageToken;
    }

    public interface ILogCommands<TLogType>
    {
        LogPage<TLogType> ReadLogPage(int pageSize);
        Task<LogPage<TLogType>> ReadLogPageAsync(int pageSize);

        TLogType CreateRecord(TLogType record);
        Task<TLogType> CreateRecordAsync(TLogType record);
    }

    public interface ILogCommands : ILogCommands<ILogRecord>
    { }
}
