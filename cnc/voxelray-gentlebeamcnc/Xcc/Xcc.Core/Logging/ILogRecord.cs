using System;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Enums;

namespace Xcc.Core.Logging
{
    public interface ILogRecord : IEntry
    {
        public LogRecordSeverity Severity { get; set; }
        public LogRecordType Type { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
