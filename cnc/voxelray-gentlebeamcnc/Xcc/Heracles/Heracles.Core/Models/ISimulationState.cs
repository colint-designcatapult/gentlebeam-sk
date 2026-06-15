using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System.Collections.ObjectModel;

using Xcc.Core.Models;

namespace Heracles.Core.Models
{
    public interface ISimulationState : ISimulation, IDirtyFlaggedBindableBase
    {
        public ObservableCollection<DeviceType> TreatmentDevices { get; set; }
        public ObservableCollection<PatientPosition> PatientPositions { get; set; }
    }
}
