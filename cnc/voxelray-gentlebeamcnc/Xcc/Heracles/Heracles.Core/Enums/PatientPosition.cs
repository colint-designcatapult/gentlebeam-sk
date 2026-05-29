using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum PatientPosition : int
    {
        [Display(Name = "Prone")]
        Prone = 1,
        [Display(Name = "Supine")]
        Supine,
        [Display(Name = "Sitting")]
        Sitting,
        [Display(Name = "Laying on RIGHT Side")]
        LyingRT,
        [Display(Name = "Laying on LEFT Side")]
        LyingLT,
        [Display(Name = "Head turned LEFT")]
        HeadLeft,
        [Display(Name = "Head turned RIGHT")]
        HeadRight,
    }
}
