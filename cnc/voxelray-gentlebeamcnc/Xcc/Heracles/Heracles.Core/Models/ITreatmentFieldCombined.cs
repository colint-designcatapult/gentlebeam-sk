using Heracles.Core.Enums;

namespace Heracles.Core.Models
{
    public interface ITreatmentFieldCombined
    {
        long Id { get; set; }

        double Actual { get; set; }

        Energy Energy { get; set; }

        double DwellTime { get; set; }
        
        TreatmentFieldName Name { get; set; }
    }
}
