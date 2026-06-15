using System.ComponentModel.DataAnnotations;

namespace Heracles.Application.Enums
{
    public enum DeepColorMode
    {
        [Display(Name = "Inactive")]
        Inactive,
        [Display(Name="Standby")]
        Standby,
        [Display(Name = "Ready")]
        Ready,
        [Display(Name = "Active")]
        Active
    }
}
