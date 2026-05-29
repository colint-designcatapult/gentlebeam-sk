using Heracles.Core.Enums;
using System.ComponentModel;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IPrescription : IEntry
    {
        public DateTime CreationDate { get; set; }
        public long SimulationId { get; set; }
        public int FxsPerWeek { get; set; }
        public Energy Energy { get; set; }
        public double DwellTime { get; set; } 
        public TDF Tdf { get; set; }
        public TDF MinTdf { get; set; }
        [Description("Dose per Fx")]
        public double DailyDose { get; set; }
        public double TotalDose { get; set; }
        public int NumberOfFxs { get; set; }
        public Status Status { get; set; }
        //public string ApprovedBy { get; set; }
        public IList<IPlan> Plans { get; set; }
        IPlan? Plan { get;  } // should it really be here?
    }
}
