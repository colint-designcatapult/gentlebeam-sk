using System.ComponentModel;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Core.Models
{
    public interface IGCBDataStore : INotifyPropertyChanged
    {
        public ISystemTelemetry? SystemTelemetry { set; get; }
    }
}
