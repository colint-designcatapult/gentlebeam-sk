using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum Description : int
    {
        [Display(Name = "Infundibulo-cytic")]
        InfundibuloCytic = 1,

        [Display(Name = "Ulcerated long-standing")]
        UIceratedLongStanding,

        [Display(Name = "Adenosquamous")]
        Adenosquamous,

        [Display(Name = "Desmoplastic/Metaplastic")]
        DesmoplasticMetaplastic,

        [Display(Name = "Recurrent Lesion post surgery")]
        RecurrentLesionPostSurgery,

        [Display(Name = "Large Lesion")]
        LargeLesion,

        [Display(Name = "Deep Lesion")]
        DeepLesion,

        [Display(Name = "Rapid growth")]
        RapidGrowth,

        [Display(Name = "Extention into hair follicle")]
        ExtensionIntoHairFollicle
    }
}
