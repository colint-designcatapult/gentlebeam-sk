namespace Heracles.Core.Models.EMR
{
    public interface ITreatmentField : ITreatmentFieldBase
    {
        IPlan Plan { get; set; }
        long PlanId { get; set; }
        double CalculatedDose { get; set; }
        double Current { get; set; }
        bool IsActive { get; set; }
        int DisplayValue { get; set; }
    }
}
