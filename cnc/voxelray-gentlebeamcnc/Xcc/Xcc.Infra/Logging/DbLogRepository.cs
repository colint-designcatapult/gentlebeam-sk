using Empyrean.Common.Infra.Settings;
using Empyrean.Common.Infra.Threading;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xcc.Core.Enums;
using Xcc.Core.Exceptions;
using Xcc.Core.Logging;

namespace Xcc.Infra.Logging
{
    public class DbLogRepository : ILogRepository
    {
        IEventAggregator EventAggregator { get; }
        public ILogWriter? BackUpLogWriter { get; }

        /// <summary>
        /// The maximum number of logs to return. The service may return fewer than this value. If unset or zero, all logs will be returned.
        /// </summary>
        private int _pageSize = 10;
        /// <summary>
        /// A page token, received from a previous ListLogs call. Provide this to retrieve the subsequent page.
        /// </summary>
        private string _nextPageToken = string.Empty;
        private bool _hasFetchedPage;

        public ILogCommands LogCommands { get; }

        private bool _writeOnceServiceError = true;
        private object _lock = new object();

        private TaskQueue _taskQueue = new TaskQueue(1);

        public DbLogRepository(
            ILogCommands logCommands,
            ILogSettings logSettings,
            IEventAggregator eventAggregator,
            ILogWriter? backUpLogWriter = null)
        {
            EventAggregator = eventAggregator;
            LogCommands = logCommands;
            BackUpLogWriter = backUpLogWriter;

            if (logSettings.LogPageSize > 0)
                _pageSize = logSettings.LogPageSize;
        }

        public bool CanFetch()
        {
            return !_hasFetchedPage || !string.IsNullOrEmpty(_nextPageToken);
        }

        public IList<ILogRecord> Fetch()
        {
            var records = new List<ILogRecord>();
            do
            {
                var response = LogCommands.ReadLogPage(_pageSize);
                records.AddRange(response.records);
                _nextPageToken = response.nextPageToken;
                _hasFetchedPage = true;
            }
            while (!string.IsNullOrEmpty(_nextPageToken));

            return records;
        }
        public async Task<IList<ILogRecord>> FetchAsync()
        {
            var records = new List<ILogRecord>();
            do
            {
                var response = await LogCommands.ReadLogPageAsync(_pageSize);
                records.AddRange(response.records);
                _nextPageToken = response.nextPageToken;
                _hasFetchedPage = true;
            }
            while (!string.IsNullOrEmpty(_nextPageToken));

            return records;
        }

        private void LogInternal(string message, LogRecordSeverity messageType, LogRecordType type)
        {
            try
            {
                lock (_lock)
                {
                    var response = LogCommands.CreateRecord(new LogRecord { Message = message, Type = type, Severity = messageType });
                    _hasFetchedPage = false;
                    _nextPageToken = string.Empty;
                    EventAggregator.GetEvent<LogRecordAddedEvent>().Publish(new LogRecord()
                    {
                        Message = message,
                        Severity = messageType,
                        TimeStamp = response?.TimeStamp ?? DateTime.Now,
                        Type = type
                    });
                }
            }
            catch (Exception ex)
            {
                if (_writeOnceServiceError)
                {
                    _writeOnceServiceError = false;
                    EventAggregator.GetEvent<LogPersistenceFailedEvent>().Publish(
                        $"Failed to save log records to the database. {ex.Message}");
                }

                try
                {
                    if (BackUpLogWriter != null)
                    {
                        BackUpLogWriter.Log(
                            $"Failed to save log record to database. {ex.Message}",
                            messageType,
                            type);
                        BackUpLogWriter.Log(message, messageType, type);
                    }
                }
                catch
                {
                    // Logging failure handling must not recursively fail logging.
                }
            }
        }

        public void Log(string message, LogRecordSeverity severity, LogRecordType type)
        {
            _ = _taskQueue.Enqueue(() => LogAsyncQueue(message, severity, type));
        }

        public Task LogAsync(string message, LogRecordSeverity messageType, LogRecordType type)
        {
            return _taskQueue.Enqueue(() => LogAsyncQueue(message, messageType, type));
        }

        private Task LogAsyncQueue(string message, LogRecordSeverity messageType, LogRecordType type)
        {
            LogInternal(message, messageType, type);
            return Task.CompletedTask;
        }
    }



}



