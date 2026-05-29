using System.Diagnostics;
using Empyrean.Common.Infra.Threading;

namespace Empyrean.Common.Infra.Logging
{
    public class TextLogRepository : ITextLogRepository
    {
        private readonly object _lock = new object();
        private readonly TaskQueue _taskQueue = new TaskQueue(1);
        private readonly string _filename;

        public TextLogRepository(string? filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new Exception($"Failed to initialize log service: log file is not specified");

            _filename = filename;

            try
            {
                if (File.Exists(_filename) == false)
                {
                    var fileStream = File.Create(_filename);
                    fileStream.Close();
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to initialize log service: log file doesn't exist and could not be created. {_filename}", e);
            }
        }
        
        private void LogInternal(string message, string severity, string type)
        {
            try
            {
                lock (_lock)
                {
                    TextLogRecord textLogRecord = new
                    (
                        message.Replace(Environment.NewLine, " "),
                        severity,
                        type
                    );

                    using StreamWriter writer = File.AppendText(_filename);
                    writer.WriteLine(textLogRecord.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void Log(string message, string messageType, string type)
        {
            _ = _taskQueue.Enqueue(() => LogAsyncQueue(message, messageType, type));
        }

        public Task LogAsync(string message, string messageType, string type)
        {
            return _taskQueue.Enqueue(() => LogAsyncQueue(message, messageType, type));
        }

        private Task LogAsyncQueue(string message, string messageType, string type)
        {
            return Task.Run(() => LogInternal(message, messageType, type));
        }


        public IList<TextLogRecord> Fetch()
        {
            string[]? logFileContent = null;
            List<TextLogRecord> records = new();

            try
            {
                logFileContent = File.ReadAllLines(_filename);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to fetch log from the file: failed to read log file {_filename}", e);
            }

            uint line = 0;

            foreach (var record in logFileContent)
            {
                try
                {
                    if (string.IsNullOrEmpty(record))
                        continue;
                    //var logRecord = LogRecordStore.Parse(record);
                    //records.Add(logRecord);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    //throw new Exception($"An error occured while trying to fetch log from the file: {AppSettings.LogFilename}. Line number: {line}. Error: {e.Message}", e);
                }

                line++;
            }

            return records;
        }

        public async Task<IList<TextLogRecord>> FetchAsync()
        {
            string[]? logFileContent = null;

            try
            {
                logFileContent = await File.ReadAllLinesAsync(_filename);
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to fetch log from the file: failed to read log file {_filename}", e);
            }

            try
            {
                return await ParseRecordsAsync(logFileContent);
            }
            catch (Exception e)
            {
                throw new Exception($"An error occured while trying to fetch log from the file: {string.Empty}. Error: {e.Message}", e);
            }
        }

        public bool CanFetch() => true;

        private static async Task<IList<TextLogRecord>> ParseRecordsAsync(string[] records)
        {
            List<TextLogRecord> temp = [];

            await Task.Run(() =>
            {
                foreach (var record in records)
                {
                    try
                    {
                        var logRecord = TextLogRecord.Parse(record);
                        if(logRecord is not null)
                        {
                            temp.Add(logRecord);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to parse a log record: {ex}");
                        return;
                    }
                }
            });

            return temp;
        }
    }
}
