using System.ComponentModel.DataAnnotations;

namespace Heracles.Indoor.ViewModels
{
    public enum SensorDistance
    {
        [Display(Name = "50 - Fine")]
        D50 = 50,

        [Display(Name = "100 - Standard")]
        D100 = 100,

        [Display(Name = "200 - Coarse")]
        D200 = 200,
    }
}
