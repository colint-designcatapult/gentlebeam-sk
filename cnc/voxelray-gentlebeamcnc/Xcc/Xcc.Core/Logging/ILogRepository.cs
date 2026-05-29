using System.Collections.Generic;
using System.Threading.Tasks;
using Xcc.Core.Enums;

namespace Xcc.Core.Logging
{
    public interface ILogWriter
    {
        /// <summary>
        /// Adds new log record to collection and writes to the store.
        /// </summary>
        public void Log(string message, LogRecordSeverity severity, LogRecordType type);
        /// <summary>
        /// Asynchronously adds new log record to collection and writes to the store.
        /// </summary>
        public Task LogAsync(string message, LogRecordSeverity messageType, LogRecordType type);
    }

    public interface ILogReader
    {
        /// <summary>
        /// Fetches all records from the store.
        /// </summary>
        public IList<ILogRecord> Fetch();
        /// <summary>
        /// Asynchronously fetches all records from the store.
        /// </summary>
        public Task<IList<ILogRecord>> FetchAsync();
        /// <summary>
        /// Checks if records can be fetched from the store.
        /// </summary>
        public bool CanFetch();
    }

    public interface ILogRepository: ILogWriter, ILogReader
    {
    }
}
