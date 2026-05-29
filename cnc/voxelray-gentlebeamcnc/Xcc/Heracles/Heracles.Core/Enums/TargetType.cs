using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum TargetType : int
    {
        [Description("No Target")]
        [Display(Name = "")]
        TargetType_None = 0,

        ////target_type_imvb_collimator_1mm_cell = 100,
        //[Description("0")]
        //TargetTypeUnspecified = 0,
        ////[PgName("target_type_imvb_collimator_1mm_cell")]
        ////[EnumMember(Value = "target_type_imvb_collimator_1mm_cell")]  
        //[Description("331")]
        //TargetTypeImvbCollimator1mmCell,
        //[Description("169")]
        //TargetTypeImvbCollimator2mmCell,
        //[Description("127")]
        //TargetTypeImvbCollimator3mmCell,
        //[Description("91")]
        //TargetTypeImvbCollimator4mmCell,
        //[Description("61")]
        //TargetTypeImvbCollimator5mmCell,
        //[Description("7")]
        //TargetTypeImvbCollimator5cmSsd0point5cmField05mmCell,
        //[Description("13")]
        //TargetTypeImvbCollimator5cmSsd1cmField1mmCell,
        //[Description("37")]
        //TargetTypeImvbCollimator5cmSsd1point5cmField1mmCell,
        //[Description("37")]
        //TargetTypeImvbCollimator5cmSsd2cmField1point5mmCell,
        //[Description("97")]
        //TargetTypeImvbCollimator5cmSsd3cmField1point5mmCell,
        //[Description("13")]
        //TargetTypeImvbCollimator6mmSpotLargeCentralCell

        [Description("TargetTypeImvbCollimator5mmCell")]
        [Display(Name = "61_FIELDS")]
        TargetType_61_Fields,

        [Description("TargetTypeImvbCollimator5cmSsd0point5cmField05mmCell")]
        [Display(Name = "7 Cells/30mm SSD")]
        TargetType_30mm_SSD_7_Fields,

        [Description("TargetTypeImvbCollimator6mmSpotLargeCentralCell")]
        [Display(Name = "13 Cells/50mm SSD")]
        TargetType_50mm_SSD_13_Fields,

        [Description("TARGET_TYPE_QC_COLLIMATOR")]
        [Display(Name = "QC applicator")]
        TargetType_QC_Collimator,

        [Description("TargetTypeCircleCollimator15mmSpotSingleCell")]
        [Display(Name = "1.5 cm")]
        TargetType_50mm_SSD_15mm_Field = 15,
        
        [Description("TargetTypeCircleCollimator20mmSpotSingleCell")]
        [Display(Name = "2 cm")]
        TargetType_50mm_SSD_20mm_Field = 20,
        
        [Description("TargetTypeCircleCollimator30mmSpotSingleCell")]
        [Display(Name = "3 cm")]
        TargetType_50mm_SSD_30mm_Field = 30,
        
        [Description("TargetTypeCircleCollimator40mmSpotSingleCell")]
        [Display(Name = "4 cm")]
        TargetType_50mm_SSD_40mm_Field = 40,
        
        [Description("TargetTypeCircleCollimator50mmSpotSingleCell")]
        [Display(Name = "5 cm")]
        TargetType_50mm_SSD_50mm_Field = 50,
    }
}
