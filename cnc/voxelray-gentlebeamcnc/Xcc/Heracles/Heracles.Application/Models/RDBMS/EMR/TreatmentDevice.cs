using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System;

using Xcc.Application.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public partial class TreatmentDevice : ITreatmentDevice
    {
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;

        public DateTime CreationDate { get; set; }
        public long SimulationId { get; set; }
        public DeviceType DeviceName { get; set; }

        public TreatmentDevice()
        {
        }

        public TreatmentDevice(long simulationId, DeviceType name)
        {
            SimulationId = simulationId;
            DeviceName = name;
        }

        public override string ToString() => DeviceName.GetDisplayName();
    }
}
