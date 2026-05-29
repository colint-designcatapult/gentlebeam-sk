using Heracles.Core.Enums;

namespace Heracles.Core.Models.EMR
{
    /// <summary>
    /// maintains the DICOM Image Series
    /// </summary>
    public interface ISeries : Xcc.Core.Models.RDBMS.EMR.ISeries
    {
        public long DiagnosisId { get; set; }

        public long VisitId { get; set; }

        public ImageType Type { get; set; }

        public string Location { get; set; }

        public double LesionDepth { get; set; }

        public string Modality { get; set; }

        public string Description { get; set; }

        public int NumberOfInstances { get; set; }
    }
}
