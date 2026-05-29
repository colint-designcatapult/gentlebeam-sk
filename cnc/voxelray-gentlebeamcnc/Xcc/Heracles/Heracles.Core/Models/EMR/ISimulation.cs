using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface ISimulation : IEntry
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

        public string SetupNote { get; set; }

        public SimulationStatus Status { get; set; }

        public TargetType TargetType { get; set; }
    }
}
