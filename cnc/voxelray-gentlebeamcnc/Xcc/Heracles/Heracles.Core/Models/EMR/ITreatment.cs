using Heracles.Core.Enums;
using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface ITreatment : IEntry
    {
        ICollection<IActualTreatmentField> ActualTreatmentFields { get; }
        DateTime CreationDate { get; set; }
        double CumulativeDose { get; set; }
        double DailyDose { get; set; }
        double LesionDepth { get; set; }
        int Fraction { get; set; }
        string PerformedBy { get; set; }
        IPlan Plan { get; }
        long PlanId { get; }
        long VisitId { get; set; }

        IActualTreatmentField GetField(TreatmentFieldName name);
        bool IsComplete();
        bool PerformedWithin(TimeSpan timeInterval);
    }
}
