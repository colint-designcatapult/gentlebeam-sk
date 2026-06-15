using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;

using System;

using Xcc.Application.Common;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class PatientPositionEntry : IPatientPosition
    {
        public long Id { get; set; } = BaseEntry.NEW_ENTRY_ID;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public PatientPosition Position { get; set; }
        public long SimulationId { get; set; }

        public PatientPositionEntry()
        {
        }

        public PatientPositionEntry(long simulationId, PatientPosition value)
        {
            SimulationId = simulationId;
            Position = value;
        }

        public override string ToString() => Position.GetDisplayName();
    }
}