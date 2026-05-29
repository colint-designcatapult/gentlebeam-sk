using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum Energy : int
    {
        [Display(Name="50")]
        Energy_50 = 50,
        [Display(Name = "70")]
        Energy_70 = 70,
        [Display(Name = "100")]
        Energy_100 = 100,
    }
}
