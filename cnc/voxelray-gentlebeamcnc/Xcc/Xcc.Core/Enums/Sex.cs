using System.ComponentModel.DataAnnotations;

namespace Xcc.Core.Enums
{
    public enum Sex
    {
        [Display(Name = "Male")]
        Male = 1,
        [Display(Name = "Female")]
        Female = 2,
        [Display(Name = "Intersex")]
        Intersex = 3
    }
}
