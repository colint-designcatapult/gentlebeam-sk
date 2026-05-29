using Xcc.Core.Domain.DataManagement.Common;

namespace Heracles.Core.Models.EMR
{
    public interface IActualTreatmentField : IHasTreatmentFieldName, IEntry
    {
        long TreatmentId { get; set; }

        int Completed { get; set; }

        int ResumePartial { get; set; }

        ICollection<IEmissionTreatmentField> EmissionTreatmentFields { get; set; } 

        double ActualDose { get; set; }

        double ActualCurrent { get; set; }

        public int DisplayValue { get; set; }

        double ActualDuration { get; set; }

        double ActualEnergy { get; set; }

        DateTime CreationDate { get; set; }
    }
}