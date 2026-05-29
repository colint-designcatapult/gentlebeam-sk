
using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum PatientStatus : int
    {
        [Display(Name = "Active")]
        Active = 1,
        [Display(Name = "Inactive")]
        Inactive = 2,
        [Display(Name = "Expired")]
        Expired = 3,
    }
}
