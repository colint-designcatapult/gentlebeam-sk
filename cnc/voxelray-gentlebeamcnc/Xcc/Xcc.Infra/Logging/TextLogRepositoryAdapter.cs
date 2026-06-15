using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Empyrean.Common.Infra.Logging;
using Empyrean.Common.Infra.Settings;
using Prism.Events;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Infra.Logging
{
    public abstract class TextLogRepositoryBase : ILogRepository
    {
        private readonly Empyrean.Common.Infra.Logging.TextLogRepository _textLogRepository;

        public TextLogRepositoryBase(
            ITextLogSettings textLogSettings)
        {
            _textLogRepository = new TextLogRepository(textLogSettings.LogFilename);
        }

        public void Log(string message, LogRecordSeverity severity, LogRecordType type)
        {
            _textLogRepository.Log(message, severity.ToString(), type.ToString());
            RaiseLogRecordAddedEvent(message, severity, type);
        }

        public async Task LogAsync(string message, LogRecordSeverity severity, LogRecordType type)
        {
            await _textLogRepository.LogAsync(message, severity.ToString(), type.ToString());
            RaiseLogRecordAddedEvent(message, severity, type);
        }

        protected abstract void RaiseLogRecordAddedEvent(string message, LogRecordSeverity severity, LogRecordType type);

        public IList<ILogRecord> Fetch()
        {
            return ToLogRecords(_textLogRepository.Fetch());
        }

        public async Task<IList<ILogRecord>> FetchAsync()
        {
            return ToLogRecords(await _textLogRepository.FetchAsync());
        }

        public bool CanFetch()
        {
            return _textLogRepository.CanFetch();
        }

        private IList<ILogRecord> ToLogRecords(IEnumerable<TextLogRecord> records)
        {
            return records.Select(x => (ILogRecord)new LogRecord()
            {
                Message = x.Message,
                Severity = Enum.Parse<LogRecordSeverity>(x.Severity),
                TimeStamp = x.TimeStamp,
                Type = Enum.Parse<LogRecordType>(x.Type)
            }).ToList();

        }
    }

    // TextLogRepositorySlimAdapter without Prism dependency to use in web applications
    public class TextLogRepositorySlimAdapter : TextLogRepositoryBase, ILogRepository
    {
        public TextLogRepositorySlimAdapter(
            ITextLogSettings textLogSettings)
            : base(textLogSettings)
        {
        }

        protected override void RaiseLogRecordAddedEvent(string message, LogRecordSeverity severity, LogRecordType type)
        {
            // Empty
        }
    }

    public class TextLogRepositoryAdapter : TextLogRepositoryBase, ILogRepository
    {
        private readonly IEventAggregator _eventAggregator;

        public TextLogRepositoryAdapter(
            ITextLogSettings textLogSettings,
            IEventAggregator eventAggregator)
            : base(textLogSettings)
        {
            _eventAggregator = eventAggregator;
        }

        protected override void RaiseLogRecordAddedEvent(string message, LogRecordSeverity severity, LogRecordType type)
        {
            _eventAggregator.GetEvent<LogRecordAddedEvent>().Publish(new LogRecord()
            {
                Message = message,
                Severity = severity,
                TimeStamp = DateTime.Now,
                Type = type
            });
        }
    }
}
