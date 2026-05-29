using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum Pathology : int
    {
        [Display(Name = "BCC")]
        Bcc = 1,
        [Display(Name = "SCC")]
        Scc = 2,
        [Display(Name = "SCC_IS")]
        SccIs = 3,
        [Display(Name = "KELOID")]
        Keloid = 4,
        [Display(Name = "Basosquamous")]
        Basosquamous
    }
};