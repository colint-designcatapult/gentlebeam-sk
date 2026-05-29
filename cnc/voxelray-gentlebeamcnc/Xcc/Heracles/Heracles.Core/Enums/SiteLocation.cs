using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum SiteLocation : int
    {
        [Display(Name = "Breast")]
        Breast = 1,
        [Display(Name = "Left Ear")]
        LeftEar,
        [Display(Name = "Right Ear")]
        RightEar,
        [Display(Name = "Forehead")]
        Forehead,
        [Display(Name = "Temple")]
        Temple,
        [Display(Name = "Zygoma")]
        Zygoma,
        [Display(Name = "Pre-Auricular")]
        PreAuricular,
        [Display(Name = "Cheek")]
        Cheek,
        [Display(Name = "Chin")]
        Chin,
        [Display(Name = "Jaw")]
        Jaw,
        [Display(Name = "Lip")]
        Lip,
        [Display(Name = "Neck")]
        Neck,
        [Display(Name = "Nose")]
        Nose,
        [Display(Name = "Scalp")]
        Scalp,
        [Display(Name = "PostAuricular")]
        PostAuricular,
        [Display(Name = "Trunk")]
        Trunk,
        [Display(Name = "Chest")]
        Chest,
        [Display(Name = "Abdomen")]
        Abdomen,
        [Display(Name = "Back")]
        Back,
        [Display(Name = "Left Lower Limb")]
        LeftLowerLimb,
        [Display(Name = "Right Lower Limb")]
        RightLowerLimb,
        [Display(Name = "Left Upper Limb")]
        LeftUpperLimb,
        [Display(Name = "Right Upper Limb")]
        RightUpperLimb,
        [Display(Name = "Right Upper Eyelid")]
        RightUpperEyelid,
        [Display(Name = "Right Lower Eyelid")]
        RightLowerEyelid,
        [Display(Name = "Left Upper Eyelid")]
        LeftUpperEyelid,
        [Display(Name = "Left Lower Eyelid")]
        LeftLowerEyelid
    }
}
