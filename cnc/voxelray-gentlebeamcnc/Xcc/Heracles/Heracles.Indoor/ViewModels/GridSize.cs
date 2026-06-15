using System.ComponentModel.DataAnnotations;

namespace Heracles.Indoor.ViewModels
{
    public enum GridSize
    {
        [Display(Name = "25 - Fine")]
        Fine25 = 25,

        [Display(Name = "50 - Fine")]
        Fine50 = 50,

        [Display(Name = "100 - Standard")]
        Standard100 = 100,

        [Display(Name = "200 - Coarse")]
        Coarse200 = 200,
    }
}
