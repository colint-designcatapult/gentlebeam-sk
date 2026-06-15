using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum VisitType : int
    {
        [Display(Name = "Simulation")]
        Simulation = 1,

        [Display(Name = "Treatment")]
        Treatment,

        [Display(Name = "On Treatment Visit")]
        OTV,

        [Display(Name = "Non-Encounter Note")]
        NonEncounterNotes,

        [Display(Name = "Follow Up Visit")]
        FollowUp,

        [Display(Name = "Skin Check")]
        SkinCheck
    }
}
