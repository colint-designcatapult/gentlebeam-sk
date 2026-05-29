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
            return true;
        }

        public IList<ILogRecord> Fetch()
        {
            var response = LogCommands.ReadLogPage(_pageSize);
            
            _nextPageToken = response.nextPageToken;

            return response.records.Reverse().ToList();
        }

        public async Task<IList<ILogRecord>> FetchAsync()
        {
            var response = await LogCommands.ReadLogPageAsync(_pageSize);
            
            _nextPageToken = response.nextPageToken;

            return response.records.Reverse().ToList();
        }

        private void LogInternal(string message, LogRecordSeverity messageType, LogRecordType type)
        {
            try
            {
                lock (_lock)
                {
                    var response = LogCommands.CreateRecord(new LogRecord { Message = message, Type = type, Severity = messageType });
                    // Reset flag, as we created the record successfully, so the connection was re-established:
                    _writeOnceServiceError = true; 
                    EventAggregator.GetEvent<LogRecordAddedEvent>().Publish(new LogRecord()
                    {
                        Message = message,
                        Severity = messageType,
                        TimeStamp = response?.TimeStamp ?? DateTime.Now,
                        Type = type
                    });
                }
            }
            catch (DataServiceException ex)
            {
                // todo: what should we do if a log record can't be saved?

                // todo: handle this exception in case when gRPC doesn't work
                //throw new DataServiceException($"Failed to save log record to database.", ex);
                //LogService.Log($"{msg}. {ex.Message}", Xcc.Core.Enums.LogRecordSeverity.Error, Xcc.Core.Enums.LogRecordType.Database);
                //
                if (BackUpLogWriter != null)
                {
                    if (_writeOnceServiceError)
                    {
                        BackUpLogWriter.Log($"Failed to save log record to database. {ex.Message}", messageType, type);
                        _writeOnceServiceError = false;
                    }

                    BackUpLogWriter.Log(message, messageType, type);
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
            return Task.Run(() => LogInternal(message, messageType, type));
        }
    }



}



