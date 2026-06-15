using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum DeviceType : int
    {
        [Display(Name = "Prefabricated Shield")]
        PrefabricatedShield = 1,

        [Display(Name = "Custom Shield")]
        CustomFabrication = 2,

        [Display(Name = "Head Holder")]
        HeadHolder = 3,

        [Display(Name = "Pillow")]
        Pillow = 4,

        [Display(Name = "Lead External Eye Shields")]
        ExternalEye = 5,

        [Display(Name = "Lead Glasses")]
        LeadGlasses = 6,

        [Display(Name = "Lead Internal Eye Shield")]
        InternalEye = 7,

        [Display(Name = "Lead Intranasal Bullet")]
        IntraNasal = 8,

        [Display(Name = "Lead Ear Canal")]
        EarCanal = 9,

        [Display(Name = "Lead Mastoid Shield")]
        Mastoid = 10,

        [Display(Name = "Lead Dental Shield")]
        DentalPacemaker = 11,

        [Display(Name = "Lead Pacemaker Shield")]
        PacemakerShield = 12,

        [Display(Name = "Gamma Putty")]
        GammaPutty = 13,

        [Display(Name = "Lead Apron")]
        LeadApron = 14,

        [Display(Name = "Thyroid Shield")]
        Thyroid = 15,

        [Display(Name = "No Shield")]
        NoShield = 16
    }
}
