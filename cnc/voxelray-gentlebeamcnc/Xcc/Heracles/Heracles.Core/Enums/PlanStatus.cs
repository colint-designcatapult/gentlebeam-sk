using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum PlanStatus
    {
        [Display(Name = "Unverified")]
        PENDING_APPROVAL = 1,
        [Display(Name = "Verified")]
        APPROVED = 2,
        [Display(Name = "Rejected")]
        REJECTED = 3
    }
}
