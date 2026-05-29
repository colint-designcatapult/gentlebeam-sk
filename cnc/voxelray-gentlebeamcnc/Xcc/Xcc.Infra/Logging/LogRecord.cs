using System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Infra.Logging
{
    public class LogRecord : ILogRecord
    {
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public LogRecordSeverity Severity { get; set; }
        public LogRecordType Type { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime TimeStamp { get; set; } = DateTime.Now;
    }
}
