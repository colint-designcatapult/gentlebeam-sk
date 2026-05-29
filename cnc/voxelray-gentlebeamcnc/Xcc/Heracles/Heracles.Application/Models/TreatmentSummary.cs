using Heracles.Core.Enums;

using System;

namespace Heracles.Application.Models
{
    public interface ITreatmentSummary
    {
        DateTime? LastTreatment { get; set; }
        int FieldIndex { get; set; }
        string FieldName { get; set; }
        Pathology? Pathology { get; set; }
        string Provider { get; set; }
        int TotalFractions { get; set; }
        int TotalPlannedFractions { get; set; }
        double? LastDeliveredDose { get; set; }
        Energy? LastDeliveredEnergy { get; set; }
        double TotalDeliveredDose { get; set; }
        double TotalPlannedDose { get; set; }
    }

    public class TreatmentSummary : ITreatmentSummary
    {
        public DateTime? LastTreatment { get; set; } = null;
        public int FieldIndex { get; set; } = 1;
        public string FieldName { get; set; } = string.Empty;
        public Pathology? Pathology { get; set; } = null;
        public string Provider { get; set; } = string.Empty;
        public int TotalFractions { get; set; } = 0;
        public int TotalPlannedFractions { get; set; } = 0;
        public double? LastDeliveredDose { get; set; } = null;
        public Energy? LastDeliveredEnergy { get; set; } = null;
        public double TotalDeliveredDose { get; set; } = 0;
        public double TotalPlannedDose { get; set; } = 0;
    }
}
