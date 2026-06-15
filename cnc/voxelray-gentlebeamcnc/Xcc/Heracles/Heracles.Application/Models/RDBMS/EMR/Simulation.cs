using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Empyrean.Common.Core.Domain.DataManagement.Common;
using Xcc.Core.Common;

namespace Heracles.Application.Models.RDBMS.EMR
{
    public class Simulation : BaseEntry, ISimulation
    {
        public DateTime CreationDate { get; set; }

        public long DiagnosisId { get; set; }

        public long VisitId { get; set; }

        public string PerformedBy { get; set; }

        public double? LesionDepth { get; set; }

        public double? LesionSizeL { get; set; }

        public double? LesionSizeW { get; set; }

        public double? MarginSizeL { get; set; }
        public double? MarginSizeW { get; set; }

        public double? ShieldSizeL { get; set; }
        public double? ShieldSizeW { get; set; }

        public double? ApplicatorSize { get; set; }

        public SimulationStatus Status { get; set; }
        public TargetType TargetType { get; set; }

        public string SetupNote { get; set; } = string.Empty;
        public ICollection<ITreatmentDevice> TreatmentDevices { get; set; } = new ObservableCollection<ITreatmentDevice>();
        public ICollection<IPatientPosition> PatientPositions { get; set; } = new ObservableCollection<IPatientPosition>();

        public Simulation()
        {
        }

        public Simulation(ISimulation s = null)
        {
            s.CopyProperties(this);
        }
    }
}
