using System.ComponentModel;
using System.Collections.Generic;
using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Core.Models
{
    public interface IGCBDataStore : INotifyPropertyChanged
    {
        public ISystemTelemetry? SystemTelemetry { set; get; }
        IReadOnlyList<FaultEntry> ActiveFaults { get; }
        void ApplyFaultUpdate(FaultUpdate update);
        void ReplaceFaults(FaultSnapshot snapshot);
    }
}
