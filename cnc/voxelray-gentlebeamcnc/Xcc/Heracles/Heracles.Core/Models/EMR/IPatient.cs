using Heracles.Core.Enums;

namespace Heracles.Core.Models.EMR
{
    public interface IPatient : Xcc.Core.Models.RDBMS.EMR.IPatient
    {
        string Address { get; set; }
        string City { get; set; }
        string State { get; set; }
        string Country { get; set; }
        string Phone { get; set; }
        string Email { get; set; }
        string Ethnicity { get; set; }
        string Race { get; set; }
        DateOnly? DOB { get; set; }
        string Picture { get; set; }
        string ProviderId { get; set; }
        string Zip { get; set; }
        IVisit? Visit { get; set; }
        string Notes { get; set; }
        public PatientStatus Status { get; set; }
    }
}
