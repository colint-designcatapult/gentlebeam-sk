namespace Empyrean.Common.Infra.Logging
{
    public interface ILogWriter
    {
        /// <summary>
        /// Adds new log record to collection and writes to the store.
        /// </summary>
        public void Log(string message, string messageType, string type);
        
        /// <summary>
        /// Asynchronously adds new log record to collection and writes to the store.
        /// </summary>
        public Task LogAsync(string message, string messageType, string type);
    }

    public interface ITextLogReader
    {
        /// <summary>
        /// Fetches all records from the store.
        /// </summary>
        public IList<TextLogRecord> Fetch();
        
        /// <summary>
        /// Asynchronously fetches all records from the store.
        /// </summary>
        public Task<IList<TextLogRecord>> FetchAsync();
        
        /// <summary>
        /// Checks if records can be fetched from the store.
        /// </summary>
        public bool CanFetch();
    }

    public interface ITextLogRepository: ILogWriter, ITextLogReader
    {
    }
}
