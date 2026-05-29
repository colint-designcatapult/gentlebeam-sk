using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum SsdType : int
    {
        [Display(Name = "50")]
        SsdType50mm = 1,
        [Display(Name = "30")]
        SsdType30mm = 2,
    }
}
