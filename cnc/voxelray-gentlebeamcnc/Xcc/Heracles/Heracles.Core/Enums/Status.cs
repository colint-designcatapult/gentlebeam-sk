using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum Status
    {
        [Display(Name = "Pending for Verification")]
        PENDING_APPROVAL = 1,
        [Display(Name = "Verified")]
        APPROVED = 2,
        [Display(Name = "Rejected")]
        REJECTED = 3
    }
}
