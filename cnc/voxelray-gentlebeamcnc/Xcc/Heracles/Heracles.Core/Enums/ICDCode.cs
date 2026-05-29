using System.ComponentModel.DataAnnotations;

namespace Heracles.Core.Enums
{
    public enum IcdCode : int 
    {
		[Display(Name = "C44.511")]
        BCC_Breast = 1,
		[Display(Name = "C44.521")]
		SCC_Breast = 2,
		[Display(Name = "D04.5")]
		SCC_IS_Breast = 3,
        [Display(Name = "C44.219")]
        BCC_LeftEar = 4,
        [Display(Name = "C44.229")]
        SCC_LeftEar = 5,
        [Display(Name = "D04.22")]
        SCC_IS_LeftEar = 6,
        [Display(Name = "C44.212")]
        BCC_RightEar = 7,
        [Display(Name = "C44.222")]
        SCC_RightEar = 8,
        [Display(Name = "D04.21")]
        SCC_IS_RightEar = 9,

        /// <summary>
        /// C44.319 *Including BCC in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma.
        /// </summary>
        [Display(Name = "C44.319")]        
        BCC_Face = 10,

        /// <summary>
        /// C44.329 *Including SCC in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma.
        /// </summary>
        [Display(Name = "C44.329")]
        SCC_Face = 11,

        /// <summary>
        /// D04.39 *Including SCC-IS in other parts of the face, such as Cheek, Chin, Forehead, Jaw, Pre-Auricular and Zygoma.
        /// </summary>
        [Display(Name = "D04.39")]
        SCC_IS_Face = 12,
        [Display(Name = "C44.01")]
        BCC_Lip = 13,
        [Display(Name = "C44.02")]
        SCC_Lip = 14,
        [Display(Name = "D04.0")]
        SCC_IS_Lip = 15,
        [Display(Name = "C44.41")]
        BCC_Neck = 16,
        [Display(Name = "C44.42")]
        SCC_Neck = 17,
        [Display(Name = "D04.4")]
        SCC_IS_Neck = 18,

        /// <summary>
        /// C44.311 *Including BCC in specific parts of the nose, such as the Ala, the bridge, the tip and root.
        /// </summary>
        [Display(Name = "C44.311")]
        BCC_Nose = 19,

        /// <summary>
        /// C44.321 *Including SCC in specific parts of the nose, such as the Ala, the bridge, the tip and root.
        /// </summary>
        [Display(Name = "C44.321")]
        SCC_Nose = 20,

        /// <summary>
        /// D04.39 *Including SCC-IS in specific parts of the nose, such as the Ala, the bridge, the tip and root.
        /// </summary>
        [Display(Name = "D04.39")]
        SCC_IS_Nose = 21,
        [Display(Name = "C44.41")]
		BCC_Scalp = 22,
        [Display(Name = "C44.42")]
        SCC_Scalp = 23,
        [Display(Name = "D04.4")]
        SCC_IS_Scalp = 24,
        [Display(Name = "C44.41")]
        BCC_PostAuricular = 25,
        [Display(Name = "C44.42")]
        SCC_PostAuricular = 26,
        [Display(Name = "D04.4")]
        SCC_IS_PostAuricular = 27,
        [Display(Name = "C44.519")]
        BCC_Trunk = 28,
        [Display(Name = "C44.529")]
        SCC_Trunk = 29,
        [Display(Name = "D04.5")]
        SCC_IS_Trunk = 30,
        [Display(Name = "C44.519")]
        BCC_Chest = 31,
        [Display(Name = "C44.529")]
        SCC_Chest = 32,
        [Display(Name = "D04.5")]
        SCC_IS_Chest = 33,
        [Display(Name = "C44.519")]
        BCC_Abdomen = 34,
        [Display(Name = "C44.529")]
        SCC_Abdomen = 35,
        [Display(Name = "D04.5")]
        SCC_IS_Abdomen = 36,
        [Display(Name = "C44.519")]
        BCC_Back = 37,
        [Display(Name = "C44.529")]
        SCC_Back = 38,
        [Display(Name = "D04.5")]
        SCC_IS_Back = 39,

        /// <summary>
        /// C44.719 *Including BCC in the left hip.
        /// </summary>
        [Display(Name = "C44.719")]
        BCC_LeftLowerLimb = 40,

        /// <summary>
        /// C44.729 *Including SCC in the left hip.
        /// </summary>
        [Display(Name = "C44.729")]
        SCC_LeftLowerLimb = 41,

        /// <summary>
        /// D04.72 *Including SCC-IS in the left hip.
        /// </summary>
        [Display(Name = "D04.72")]
        SCC_IS_LeftLowerLimb = 42,
        [Display(Name = "C44.712")]
        BCC_RightLowerLimb = 43,
        [Display(Name = "C44.722")]
        SCC_RightLowerLimb = 44,
        [Display(Name = "D04.71")]
        SCC_IS_RightLowerLimb = 45,
        [Display(Name = "C44.619")]
        BCC_LeftUpperLimb = 46,
        [Display(Name = "C44.629")]
        SCC_LeftUpperLimb = 47,
        [Display(Name = "D04.62")]
        SCC_IS_LeftUpperLimb = 48,
        [Display(Name = "C44.612")]
        BCC_RightUpperLimb = 49,
        [Display(Name = "C44.622")]
        SCC_RightUpperLimb = 50,
        [Display(Name = "D04.61")]
        SCC_IS_RightUpperLimb = 51,
        [Display(Name = "C44.1121")]
        BCC_RightUpperEyelid = 52,
        [Display(Name = "C44.1221")]
        SCC_RightUpperEyelid = 53,
        [Display(Name = "D04.111")]
        SCC_IS_RightUpperEyelid = 54,
        [Display(Name = "C44.1122")]
        BCC_RightLowerEyelid = 55,
        [Display(Name = "C44.1222")]
        SCC_RightLowerEyelid = 56,
        [Display(Name = "D04.112")]
        SCC_IS_RightLowerEyelid = 57,
        [Display(Name = "C44.1191")]
        BCC_LeftUpperEyelid = 58,
        [Display(Name = "C44.1291")]
        SCC_LeftUpperEyelid = 59,
        [Display(Name = "D04.121")]
        SCC_IS_LeftUpperEyelid = 60,
        [Display(Name = "C44.1192")]
        BCC_LeftLowerEyelid = 61,
        [Display(Name = "C44.1292")]
        SCC_LeftLowerEyelid = 62,
        [Display(Name = "D04.122")]
        SCC_IS_LeftLowerEyelid = 63,

        [Display(Name = "C44.591")]
        BASOSQUAMOUS_Breast,
        [Display(Name = "C44.299")]
        BASOSQUAMOUS_LeftEar,
        [Display(Name = "C44.292")]
        BASOSQUAMOUS_RightEar,
        [Display(Name = "C44.399")]
        BASOSQUAMOUS_Face,
        [Display(Name = "C44.09")]
        BASOSQUAMOUS_Lip,
        [Display(Name = "C44.49")]
        BASOSQUAMOUS_Neck,

        /// <summary>
        /// Nose (ex. ala, bridge, tip, root)
        /// </summary>
        [Display(Name = "C44.391")]
        BASOSQUAMOUS_Nose,

        [Display(Name = "C44.49")]
        BASOSQUAMOUS_Scalp,

        [Display(Name = "C44.49")]
        BASOSQUAMOUS_PostAuricular,

        [Display(Name = "C44.599")]
        BASOSQUAMOUS_Trunk,

        [Display(Name = "C44.599")]
        BASOSQUAMOUS_Chest,

        [Display(Name = "C44.599")]
        BASOSQUAMOUS_Abdomen,

        [Display(Name = "C44.599")]
        BASOSQUAMOUS_Back,


        /// <summary>
        /// Including hip
        /// </summary>
        [Display(Name = "C44.799")]
        BASOSQUAMOUS_LeftLowerLimb,

        /// <summary>
        /// Including hip
        /// </summary>
        [Display(Name = "C44.792")]
        BASOSQUAMOUS_RightLowerLimb,

        /// <summary>
        /// Including shoulder
        /// </summary>
        [Display(Name = "C44.699")]
        BASOSQUAMOUS_LeftUpperLimb,

        /// <summary>
        /// Including shoulder
        /// </summary>
        [Display(Name = "C44.692")]
        BASOSQUAMOUS_RightUpperLimb,

        [Display(Name = "C44.1992")]
        BASOSQUAMOUS_RightUpperEyelid,

        /// <summary>
        /// Including Canthus
        /// </summary>
        [Display(Name = "C44.1991")]
        BASOSQUAMOUS_RightLowerEyelid,

        [Display(Name = "C44.1922")]
        BASOSQUAMOUS_LeftUpperEyelid,

        /// <summary>
        /// Including Canthus
        /// </summary>
        [Display(Name = "C44.1921")]
        BASOSQUAMOUS_LeftLowerEyelid,

        [Display(Name = "None")]
        None
    }
}
