using Xcc.Core.Enums;

namespace Xcc.Core.Models
{
    public interface IReport
    {
        public string Message { get; }
        public string Header { get; }
        public ReportType Type { get; }
        public double Progress { get; }
    }
}
