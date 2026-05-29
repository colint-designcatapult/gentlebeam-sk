using Heracles.Core.Enums;

namespace Heracles.Core.Constants
{
    public static class IcdCodes
    {
        public static IcdCode? GetCode(SiteLocation? location, Pathology? pathology)
        {
            return pathology switch
            {
                Pathology.Bcc => GetBccCode(location),
                Pathology.Scc => GetSccCode(location),
                Pathology.SccIs => GetSccIsCode(location),
                Pathology.Basosquamous => GetBasosquamousCode(location),
                null => null,
                _ => IcdCode.None
            };
        }


        public static IcdCode? GetBccCode(SiteLocation? location)
        {
            switch(location)
            {
                case SiteLocation.Breast:
                    return IcdCode.BCC_Breast;

                case SiteLocation.LeftEar:
                    return IcdCode.BCC_LeftEar;

                case SiteLocation.RightEar:
                    return IcdCode.BCC_RightEar;

                case SiteLocation.Forehead:
                case SiteLocation.Temple:
                case SiteLocation.Zygoma:
                case SiteLocation.PreAuricular:
                case SiteLocation.Cheek:
                case SiteLocation.Chin:
                case SiteLocation.Jaw:
                    return IcdCode.BCC_Face;
                case SiteLocation.Lip:
                    return IcdCode.BCC_Lip;
                case SiteLocation.Neck:
                    return IcdCode.BCC_Neck;
                case SiteLocation.Scalp:
                    return IcdCode.BCC_Scalp;
                case SiteLocation.PostAuricular:
                    return IcdCode.BCC_PostAuricular;
                case SiteLocation.Nose:
                    return IcdCode.BCC_Nose;
                case SiteLocation.Trunk:
                    return IcdCode.BCC_Trunk;
                case SiteLocation.Chest:
                    return IcdCode.BCC_Chest;
                case SiteLocation.Abdomen:
                    return IcdCode.BCC_Abdomen;
                case SiteLocation.Back:
                    return IcdCode.BCC_Back;
                case SiteLocation.LeftLowerLimb:
                    return IcdCode.BCC_LeftLowerLimb;
                case SiteLocation.RightLowerLimb:
                    return IcdCode.BCC_RightLowerLimb;
                case SiteLocation.LeftUpperLimb:
                    return IcdCode.BCC_LeftUpperLimb;
                case SiteLocation.RightUpperLimb:
                    return IcdCode.BCC_RightUpperLimb;
                case SiteLocation.RightUpperEyelid:
                    return IcdCode.BCC_RightUpperEyelid;
                case SiteLocation.RightLowerEyelid:
                    return IcdCode.BCC_RightLowerEyelid;
                case SiteLocation.LeftUpperEyelid:
                    return IcdCode.BCC_LeftUpperEyelid;
                case SiteLocation.LeftLowerEyelid:
                    return IcdCode.BCC_LeftLowerEyelid;

                default:
                    return null;
            };
        }

        public static IcdCode? GetSccCode(SiteLocation? location)
        {
            switch (location)
            {
                case SiteLocation.Breast:
                    return IcdCode.SCC_Breast;
                case SiteLocation.LeftEar:
                    return IcdCode.SCC_LeftEar;
                case SiteLocation.RightEar:
                    return IcdCode.SCC_RightEar;
                case SiteLocation.Forehead:
                case SiteLocation.Temple:
                case SiteLocation.Zygoma:
                case SiteLocation.PreAuricular:
                case SiteLocation.Cheek:
                case SiteLocation.Chin:
                case SiteLocation.Jaw:
                    return IcdCode.SCC_Face;
                case SiteLocation.Lip:
                    return IcdCode.SCC_Lip;
                case SiteLocation.Neck:
                    return IcdCode.SCC_Neck;
                case SiteLocation.Scalp:
                    return IcdCode.SCC_Scalp;
                case SiteLocation.PostAuricular:
                    return IcdCode.SCC_PostAuricular;
                case SiteLocation.Nose:
                    return IcdCode.SCC_Nose;
                case SiteLocation.Trunk:
                    return IcdCode.SCC_Trunk;
                case SiteLocation.Chest:
                    return IcdCode.SCC_Chest;
                case SiteLocation.Abdomen:
                    return IcdCode.SCC_Abdomen;    
                case SiteLocation.Back:
                    return IcdCode.SCC_Back;
                case SiteLocation.LeftLowerLimb:
                    return IcdCode.SCC_LeftLowerLimb;
                case SiteLocation.RightLowerLimb:
                    return IcdCode.SCC_RightLowerLimb;
                case SiteLocation.LeftUpperLimb:
                    return IcdCode.SCC_LeftUpperLimb;
                case SiteLocation.RightUpperLimb:
                    return IcdCode.SCC_RightUpperLimb;
                case SiteLocation.RightUpperEyelid:
                    return IcdCode.SCC_RightUpperEyelid;
                case SiteLocation.RightLowerEyelid:
                    return IcdCode.SCC_RightLowerEyelid;
                case SiteLocation.LeftUpperEyelid:
                    return IcdCode.SCC_LeftUpperEyelid;
                case SiteLocation.LeftLowerEyelid:
                    return IcdCode.SCC_LeftLowerEyelid;

                default:
                    return null;
            };
        }

        public static IcdCode? GetSccIsCode(SiteLocation? location)
        {
            switch (location)
            {
                case SiteLocation.Breast:
                    return IcdCode.SCC_IS_Breast;
                case SiteLocation.LeftEar:
                    return IcdCode.SCC_IS_LeftEar;
                case SiteLocation.RightEar:
                    return IcdCode.SCC_IS_RightEar;
                case SiteLocation.Forehead:
                case SiteLocation.Temple:
                case SiteLocation.Zygoma:
                case SiteLocation.PreAuricular:
                case SiteLocation.Cheek:
                case SiteLocation.Chin:
                case SiteLocation.Jaw:
                    return IcdCode.SCC_IS_Face;
                case SiteLocation.Lip:
                    return IcdCode.SCC_IS_Lip;
                case SiteLocation.Neck:
                    return IcdCode.SCC_IS_Neck;
                case SiteLocation.Scalp:
                    return IcdCode.SCC_IS_Scalp;
                case SiteLocation.PostAuricular:
                    return IcdCode.SCC_IS_PostAuricular;
                case SiteLocation.Nose:
                    return IcdCode.SCC_IS_Nose;
                case SiteLocation.Trunk:
                    return IcdCode.SCC_IS_Trunk;
                case SiteLocation.Chest:
                    return IcdCode.SCC_IS_Chest;
                case SiteLocation.Abdomen:
                    return IcdCode.SCC_IS_Abdomen;
                case SiteLocation.Back:
                    return IcdCode.SCC_IS_Back;
                case SiteLocation.LeftLowerLimb:
                    return IcdCode.SCC_IS_LeftLowerLimb;
                case SiteLocation.RightLowerLimb:
                    return IcdCode.SCC_IS_RightLowerLimb;
                case SiteLocation.LeftUpperLimb:
                    return IcdCode.SCC_IS_LeftUpperLimb;
                case SiteLocation.RightUpperLimb:
                    return IcdCode.SCC_IS_RightUpperLimb;
                case SiteLocation.RightUpperEyelid:
                    return IcdCode.SCC_IS_RightUpperEyelid;
                case SiteLocation.RightLowerEyelid:
                    return IcdCode.SCC_IS_RightLowerEyelid;
                case SiteLocation.LeftUpperEyelid:
                    return IcdCode.SCC_IS_LeftUpperEyelid;
                case SiteLocation.LeftLowerEyelid:
                    return IcdCode.SCC_IS_LeftLowerEyelid;

                default:
                    return null;
            };
        }

        public static IcdCode? GetBasosquamousCode(SiteLocation? location)
        {
            switch (location)
            {
                case SiteLocation.Breast:
                    return IcdCode.BASOSQUAMOUS_Breast;

                case SiteLocation.LeftEar:
                    return IcdCode.BASOSQUAMOUS_LeftEar;

                case SiteLocation.RightEar:
                    return IcdCode.BASOSQUAMOUS_RightEar;

                case SiteLocation.Forehead:
                case SiteLocation.Temple:
                case SiteLocation.Zygoma:
                case SiteLocation.PreAuricular:
                case SiteLocation.Cheek:
                case SiteLocation.Chin:
                case SiteLocation.Jaw:
                    return IcdCode.BASOSQUAMOUS_Face;

                case SiteLocation.Lip:
                    return IcdCode.BASOSQUAMOUS_Lip;

                case SiteLocation.Neck:
                    return IcdCode.BASOSQUAMOUS_Neck;

                case SiteLocation.Scalp:
                    return IcdCode.BASOSQUAMOUS_Scalp;

                case SiteLocation.PostAuricular:
                    return IcdCode.BASOSQUAMOUS_PostAuricular;

                case SiteLocation.Nose:
                    return IcdCode.BASOSQUAMOUS_Nose;

                case SiteLocation.Trunk:
                    return IcdCode.BASOSQUAMOUS_Trunk;

                case SiteLocation.Chest:
                    return IcdCode.BASOSQUAMOUS_Chest;

                case SiteLocation.Abdomen:
                    return IcdCode.BASOSQUAMOUS_Abdomen;

                case SiteLocation.Back:
                    return IcdCode.BASOSQUAMOUS_Back;

                case SiteLocation.LeftLowerLimb:
                    return IcdCode.BASOSQUAMOUS_LeftLowerLimb;

                case SiteLocation.RightLowerLimb:
                    return IcdCode.BASOSQUAMOUS_RightLowerLimb;

                case SiteLocation.LeftUpperLimb:
                    return IcdCode.BASOSQUAMOUS_LeftUpperLimb;

                case SiteLocation.RightUpperLimb:
                    return IcdCode.BASOSQUAMOUS_RightUpperLimb;

                case SiteLocation.RightUpperEyelid:
                    return IcdCode.BASOSQUAMOUS_RightUpperEyelid;

                case SiteLocation.RightLowerEyelid:
                    return IcdCode.BASOSQUAMOUS_RightLowerEyelid;

                case SiteLocation.LeftUpperEyelid:
                    return IcdCode.BASOSQUAMOUS_LeftUpperEyelid;

                case SiteLocation.LeftLowerEyelid:
                    return IcdCode.BASOSQUAMOUS_LeftLowerEyelid;
                default:
                    return null;
            };
        }
    }
}
