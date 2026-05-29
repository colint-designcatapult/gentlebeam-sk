using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System.Collections.ObjectModel;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models
{
    public interface ITreatmentBindable : IEntry
    {
        int Fraction { get; set; }
        DateTime CreationDate { get; set; }
        double LesionDepth { get; set; }
        Energy Energy { get; set; }
        double DailyDose { get; set; }
        double CumulativeDose { get; set; }
        string PerformedBy { get; set; }
        ObservableCollection<IActualTreatmentField> ActualTreatmentFields { get; }
        bool IsComplete { get; }
    }
}
