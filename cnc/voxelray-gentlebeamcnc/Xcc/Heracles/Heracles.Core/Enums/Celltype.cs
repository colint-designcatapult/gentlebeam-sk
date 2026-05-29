using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum Celltype : int
    {
        [Display(Name = "Aberrant")]
        Aberrant = 1,

        [Display(Name = "Adenoid")]
        Adenoid,

        [Display(Name = "Atypical Basaloid Proliferation")]
        AtypicalBasaloidProliferation,

        [Display(Name = "Basosquamous (Metatypical)")]
        BasosquamousMetatypical,

        [Display(Name = "Adnexal Differentiation")]
        AdnexalDifferentiation,

        [Display(Name = "Squamous Differentiation")]
        SquamousDifferentiation,

        [Display(Name = "Clear Ring")]
        ClearRing,

        [Display(Name = "Cystic Cell")]
        CysticCellCarcinoma,

        [Display(Name = "Fibroepithelioma of Pinkus")]
        FibroepitheliomaOfPinkus,

        [Display(Name = "Infiltrative")]
        Infiltrative,

        [Display(Name = "Keratotic ")]
        Keratotic,

        [Display(Name = "Micro Nodular")]
        MicroNodular,

        [Display(Name = "Mixed Pattern (BCC + SCC)")]
        /// <summary>
        /// Mixed Pattern cell type (BCC + SCC).
        /// </summary>
        MixedPattern,

        [Display(Name = "Morphoeic/Sclerosing/Fibrosing")]
        MorphoeicSclerosingFibrosing,

        [Display(Name = "Nodular (Classic Basal-Cell)")]
        NodularClassicBasalCell,

        [Display(Name = "Nodulocystic")]
        Nodulocystic,

        [Display(Name = "Pigmented")]
        Pigmented,

        [Display(Name = "Pleomorphic")]
        Pleomorphic,

        [Display(Name = "Polypoid")]
        Polypoid,

        [Display(Name = "Pore-like")]
        PoreLike,

        [Display(Name = "Rodent Ulcer (Jacobi Ulcer)")]
        RodentUlcerJacobiUlcer,

        [Display(Name = "Superficial (Multicentric)")]
        SuperficialMulticentric,

        [Display(Name = "Acantholytic")]
        Acantholytic,

        [Display(Name = "Adenoid/Pseudoglandular")]
        AdenoidPseudoglandular,

        [Display(Name = "Atypical Squamous Proliferation")]
        AtypicalSquamousProliferation,

        [Display(Name = "Basaloid")]
        Basaloid,

        [Display(Name = "Clear-cell")]
        ClearCell,

        [Display(Name = "Erythroplasia")]
        Erythroplasia,

        [Display(Name = "Intraepidermal")]
        Intraepidermal,

        [Display(Name = "Invasive")]
        Invasive,

        [Display(Name = "Keratoacanthoma")]
        Keratoacanthoma,

        [Display(Name = "Large Cell Keratinizing")]
        LargeCellKeratinizing,

        [Display(Name = "Large Cell Non-Keratinizing")]
        LargeCellNonKeratinizing,

        [Display(Name = "Metaplasia")]
        Metaplasia,

        [Display(Name = "Moderately Differentiated")]
        ModeratelyDifferentiated,

        [Display(Name = "Poorly Differentiated")]
        PoorlyDifferentiated,

        [Display(Name = "Papillary Carcinoma")]
        PapillaryCarcinoma,
        
        [Display(Name = "Signet-ring")]        
        SignetRing,

        [Display(Name = "Small Cell Keratinizing")]
        SmallCellKeratinizing,

        [Display(Name = "Superficial")]
        Superficial,

        [Display(Name = "Spindle")]
        SpindleCell,

        [Display(Name = "Verrucous")]
        Verrucous,

        [Display(Name = "Well-Differentiated")]
        WellDifferentiated,

        [Display(Name = "Superficially Invasive")]
        SuperficiallyInvasive,

        [Display(Name = "Other")]
        Other,
                    
        [Display(Name = "None")]
        None
    }
}
