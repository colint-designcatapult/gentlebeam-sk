using Empyrean.Common.Core.Domain.DataManagement.Common;
using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.Linq;
using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Prescription : BaseEntry, IPrescription
    {
        public Prescription(IPrescription? p = null)
        {
            p?.CopyProperties(this);
        }

        public Prescription()
        {                        
        }
        
        public DateTime CreationDate { get; set; } = DateTime.Now;

        public long SimulationId { get; set; } = NewEntryId;

        public int FxsPerWeek { get; set; }

        public Energy Energy { get; set; }

        public double DwellTime { get; set; } = 0.0;

        public TDF Tdf { get; set; }

        public TDF MinTdf { get; set; }

        public double DailyDose { get; set; } = 0.0;
        public double TotalDose { get; set; } = 0.0;

        public int NumberOfFxs { get; set; } = 0;

        public Status Status { get; set; } = Status.PENDING_APPROVAL;

        public virtual IList<IPlan> Plans { get; set; } = new List<IPlan>();

        public IPlan? Plan => Plans?.LastOrDefault();
    }
}
