using Xcc.Core.Enums;
using Xcc.Core.Models;

namespace Xcc.Application.Models
{
    public class Report : IReport
    {
        public Report(ReportType type, string header, string message, double progress = 0d)
        {
            Type = type;
            Header = header;
            Message = message;
            Progress = progress;
        }

        public ReportType Type { get; }
        public string Message { get; }
        public string Header { get; }
        public double Progress { get; }
    }
}
