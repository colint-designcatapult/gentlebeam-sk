
using Heracles.Core.Constants;
using Heracles.Core.Enums;

namespace Heracles.Core.Test.Constants
{
    [TestFixture]
    public class IcdCodesTests
    {
        //[TestCase(SiteLocation.Abdomen, IcdCode.BCC_Abdomen)]
        //[TestCase(SiteLocation.Back, IcdCode.BCC_Back)]
        //[TestCase(SiteLocation.Breast, IcdCode.BCC_Breast)]
        //[TestCase(SiteLocation.Cheek, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.Forehead, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.Temple, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.Zygoma, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.PreAuricular, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.Chin, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.Jaw, IcdCode.BCC_Face)]
        //[TestCase(SiteLocation.Nose, IcdCode.BCC_Nose)]
        //[TestCase(SiteLocation.Neck, IcdCode.BCC_Neck)]
        //[TestCase(SiteLocation.LeftEar, IcdCode.BCC_LeftEar)]
        //[TestCase(SiteLocation.RightEar, IcdCode.BCC_RightEar)]
        //[TestCase(SiteLocation.Lip, IcdCode.BCC_Lip)]
        //[TestCase(SiteLocation.Scalp, IcdCode.BCC_Scalp)]
        //[TestCase(SiteLocation.PostAuricular, IcdCode.BCC_PostAuricular)]
        //[TestCase(SiteLocation.Trunk, IcdCode.BCC_Trunk)]
        //[TestCase(SiteLocation.Chest, IcdCode.BCC_Chest)]
        //[TestCase(SiteLocation.LeftLowerLimb, IcdCode.BCC_LeftLowerLimb)]
        //[TestCase(SiteLocation.RightLowerLimb, IcdCode.BCC_RightLowerLimb)]
        //[TestCase(SiteLocation.LeftUpperLimb, IcdCode.BCC_LeftUpperLimb)]
        //[TestCase(SiteLocation.RightUpperLimb, IcdCode.BCC_RightUpperLimb)]
        //[TestCase(SiteLocation.RightUpperEyelid, IcdCode.BCC_RightUpperEyelid)]
        //[TestCase(SiteLocation.RightLowerEyelid, IcdCode.BCC_RightLowerEyelid)]
        //[TestCase(SiteLocation.LeftUpperEyelid, IcdCode.BCC_LeftUpperEyelid)]
        //[TestCase(SiteLocation.LeftLowerEyelid, IcdCode.BCC_LeftLowerEyelid)]
        //[TestCase(null, null)]
        //public void GetBccCode_ShouldReturnCorrectCode(SiteLocation? location, IcdCode? expectedCode)
        //{
        //    var result = IcdCodes.GetBccCode(location);

        //    Assert.AreEqual(expectedCode, result);
        //}
        
        [Test]
        public void GetBccCode_ShouldReturnNull()
        {
            SiteLocation? invalidLocation = (SiteLocation)123456;
            var result = IcdCodes.GetBccCode(invalidLocation);

            Assert.IsNull(result);
        }

        [Test]
        public void GetSccCode_ShouldReturnNull()
        {
            SiteLocation? invalidLocation = (SiteLocation)123456;
            var result = IcdCodes.GetSccCode(invalidLocation);

            Assert.IsNull(result);
        }

        [Test]
        public void GetSccIsCode_ShouldReturnNull()
        {
            SiteLocation? invalidLocation = (SiteLocation)123456;
            var result = IcdCodes.GetSccIsCode(invalidLocation);

            Assert.IsNull(result);
        }

        [Test]
        public void GetBasosquamousCode_ShouldReturnNull()
        {
            SiteLocation? invalidLocation = (SiteLocation)123456;
            var result = IcdCodes.GetBasosquamousCode(invalidLocation);

            Assert.IsNull(result);
        }

        [TestCase(SiteLocation.Abdomen, Pathology.Keloid, IcdCode.None)]
        [TestCase(SiteLocation.Abdomen, null, null)]
        [TestCase(SiteLocation.Abdomen, Pathology.Scc, IcdCode.SCC_Abdomen)]
        [TestCase(SiteLocation.Back, Pathology.Scc, IcdCode.SCC_Back)]
        [TestCase(SiteLocation.Breast, Pathology.Scc, IcdCode.SCC_Breast)]
        [TestCase(SiteLocation.Cheek, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.Forehead, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.Temple, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.Zygoma, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.PreAuricular, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.Chin, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.Jaw, Pathology.Scc, IcdCode.SCC_Face)]
        [TestCase(SiteLocation.Nose, Pathology.Scc, IcdCode.SCC_Nose)]
        [TestCase(SiteLocation.Neck, Pathology.Scc, IcdCode.SCC_Neck)]
        [TestCase(SiteLocation.LeftEar, Pathology.Scc, IcdCode.SCC_LeftEar)]
        [TestCase(SiteLocation.RightEar, Pathology.Scc, IcdCode.SCC_RightEar)]
        [TestCase(SiteLocation.Lip, Pathology.Scc, IcdCode.SCC_Lip)]
        [TestCase(SiteLocation.Scalp, Pathology.Scc, IcdCode.SCC_Scalp)]
        [TestCase(SiteLocation.PostAuricular, Pathology.Scc, IcdCode.SCC_PostAuricular)]
        [TestCase(SiteLocation.Trunk, Pathology.Scc, IcdCode.SCC_Trunk)]
        [TestCase(SiteLocation.Chest, Pathology.Scc, IcdCode.SCC_Chest)]
        [TestCase(SiteLocation.LeftLowerLimb, Pathology.Scc, IcdCode.SCC_LeftLowerLimb)]
        [TestCase(SiteLocation.RightLowerLimb, Pathology.Scc, IcdCode.SCC_RightLowerLimb)]
        [TestCase(SiteLocation.LeftUpperLimb, Pathology.Scc, IcdCode.SCC_LeftUpperLimb)]
        [TestCase(SiteLocation.RightUpperLimb, Pathology.Scc, IcdCode.SCC_RightUpperLimb)]
        [TestCase(SiteLocation.RightUpperEyelid, Pathology.Scc, IcdCode.SCC_RightUpperEyelid)]
        [TestCase(SiteLocation.RightLowerEyelid, Pathology.Scc, IcdCode.SCC_RightLowerEyelid)]
        [TestCase(SiteLocation.LeftUpperEyelid, Pathology.Scc, IcdCode.SCC_LeftUpperEyelid)]
        [TestCase(SiteLocation.LeftLowerEyelid, Pathology.Scc, IcdCode.SCC_LeftLowerEyelid)]
        [TestCase(null, Pathology.Scc, null)]

        [TestCase(SiteLocation.Abdomen, Pathology.Bcc, IcdCode.BCC_Abdomen)]
        [TestCase(SiteLocation.Back, Pathology.Bcc, IcdCode.BCC_Back)]
        [TestCase(SiteLocation.Breast, Pathology.Bcc, IcdCode.BCC_Breast)]
        [TestCase(SiteLocation.Cheek, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.Forehead, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.Temple, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.Zygoma, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.PreAuricular, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.Chin, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.Jaw, Pathology.Bcc, IcdCode.BCC_Face)]
        [TestCase(SiteLocation.Nose, Pathology.Bcc, IcdCode.BCC_Nose)]
        [TestCase(SiteLocation.Neck, Pathology.Bcc, IcdCode.BCC_Neck)]
        [TestCase(SiteLocation.LeftEar, Pathology.Bcc, IcdCode.BCC_LeftEar)]
        [TestCase(SiteLocation.RightEar, Pathology.Bcc, IcdCode.BCC_RightEar)]
        [TestCase(SiteLocation.Lip, Pathology.Bcc, IcdCode.BCC_Lip)]
        [TestCase(SiteLocation.Scalp, Pathology.Bcc, IcdCode.BCC_Scalp)]
        [TestCase(SiteLocation.PostAuricular, Pathology.Bcc, IcdCode.BCC_PostAuricular)]
        [TestCase(SiteLocation.Trunk, Pathology.Bcc, IcdCode.BCC_Trunk)]
        [TestCase(SiteLocation.Chest, Pathology.Bcc, IcdCode.BCC_Chest)]
        [TestCase(SiteLocation.LeftLowerLimb, Pathology.Bcc, IcdCode.BCC_LeftLowerLimb)]
        [TestCase(SiteLocation.RightLowerLimb, Pathology.Bcc, IcdCode.BCC_RightLowerLimb)]
        [TestCase(SiteLocation.LeftUpperLimb, Pathology.Bcc, IcdCode.BCC_LeftUpperLimb)]
        [TestCase(SiteLocation.RightUpperLimb, Pathology.Bcc, IcdCode.BCC_RightUpperLimb)]
        [TestCase(SiteLocation.RightUpperEyelid, Pathology.Bcc, IcdCode.BCC_RightUpperEyelid)]
        [TestCase(SiteLocation.RightLowerEyelid, Pathology.Bcc, IcdCode.BCC_RightLowerEyelid)]
        [TestCase(SiteLocation.LeftUpperEyelid, Pathology.Bcc, IcdCode.BCC_LeftUpperEyelid)]
        [TestCase(SiteLocation.LeftLowerEyelid, Pathology.Bcc, IcdCode.BCC_LeftLowerEyelid)]
        [TestCase(null, Pathology.Bcc, null)]

        [TestCase(SiteLocation.Abdomen, Pathology.SccIs, IcdCode.SCC_IS_Abdomen)]
        [TestCase(SiteLocation.Back, Pathology.SccIs, IcdCode.SCC_IS_Back)]
        [TestCase(SiteLocation.Breast, Pathology.SccIs, IcdCode.SCC_IS_Breast)]
        [TestCase(SiteLocation.Cheek, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.Forehead, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.Temple, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.Zygoma, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.PreAuricular, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.Chin, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.Jaw, Pathology.SccIs, IcdCode.SCC_IS_Face)]
        [TestCase(SiteLocation.Nose, Pathology.SccIs, IcdCode.SCC_IS_Nose)]
        [TestCase(SiteLocation.Neck, Pathology.SccIs, IcdCode.SCC_IS_Neck)]
        [TestCase(SiteLocation.LeftEar, Pathology.SccIs, IcdCode.SCC_IS_LeftEar)]
        [TestCase(SiteLocation.RightEar, Pathology.SccIs, IcdCode.SCC_IS_RightEar)]
        [TestCase(SiteLocation.Lip, Pathology.SccIs, IcdCode.SCC_IS_Lip)]
        [TestCase(SiteLocation.Scalp, Pathology.SccIs, IcdCode.SCC_IS_Scalp)]
        [TestCase(SiteLocation.PostAuricular, Pathology.SccIs, IcdCode.SCC_IS_PostAuricular)]
        [TestCase(SiteLocation.Trunk, Pathology.SccIs, IcdCode.SCC_IS_Trunk)]
        [TestCase(SiteLocation.Chest, Pathology.SccIs, IcdCode.SCC_IS_Chest)]
        [TestCase(SiteLocation.LeftLowerLimb, Pathology.SccIs, IcdCode.SCC_IS_LeftLowerLimb)]
        [TestCase(SiteLocation.RightLowerLimb, Pathology.SccIs, IcdCode.SCC_IS_RightLowerLimb)]
        [TestCase(SiteLocation.LeftUpperLimb, Pathology.SccIs, IcdCode.SCC_IS_LeftUpperLimb)]
        [TestCase(SiteLocation.RightUpperLimb, Pathology.SccIs, IcdCode.SCC_IS_RightUpperLimb)]
        [TestCase(SiteLocation.RightUpperEyelid, Pathology.SccIs, IcdCode.SCC_IS_RightUpperEyelid)]
        [TestCase(SiteLocation.RightLowerEyelid, Pathology.SccIs, IcdCode.SCC_IS_RightLowerEyelid)]
        [TestCase(SiteLocation.LeftUpperEyelid, Pathology.SccIs, IcdCode.SCC_IS_LeftUpperEyelid)]
        [TestCase(SiteLocation.LeftLowerEyelid, Pathology.SccIs, IcdCode.SCC_IS_LeftLowerEyelid)]
        [TestCase(null, Pathology.SccIs, null)]

        [TestCase(SiteLocation.Abdomen, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Abdomen)]
        [TestCase(SiteLocation.Back, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Back)]
        [TestCase(SiteLocation.Breast, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Breast)]
        [TestCase(SiteLocation.Cheek, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.Forehead, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.Temple, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.Zygoma, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.PreAuricular, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.Chin, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.Jaw, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Face)]
        [TestCase(SiteLocation.Nose, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Nose)]
        [TestCase(SiteLocation.Neck, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Neck)]
        [TestCase(SiteLocation.LeftEar, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_LeftEar)]
        [TestCase(SiteLocation.RightEar, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_RightEar)]
        [TestCase(SiteLocation.Lip, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Lip)]
        [TestCase(SiteLocation.Scalp, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Scalp)]
        [TestCase(SiteLocation.PostAuricular, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_PostAuricular)]
        [TestCase(SiteLocation.Trunk, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Trunk)]
        [TestCase(SiteLocation.Chest, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_Chest)]
        [TestCase(SiteLocation.LeftLowerLimb, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_LeftLowerLimb)]
        [TestCase(SiteLocation.RightLowerLimb, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_RightLowerLimb)]
        [TestCase(SiteLocation.LeftUpperLimb, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_LeftUpperLimb)]
        [TestCase(SiteLocation.RightUpperLimb, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_RightUpperLimb)]
        [TestCase(SiteLocation.RightUpperEyelid, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_RightUpperEyelid)]
        [TestCase(SiteLocation.RightLowerEyelid, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_RightLowerEyelid)]
        [TestCase(SiteLocation.LeftUpperEyelid, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_LeftUpperEyelid)]
        [TestCase(SiteLocation.LeftLowerEyelid, Pathology.Basosquamous, IcdCode.BASOSQUAMOUS_LeftLowerEyelid)]
        [TestCase(null, Pathology.Basosquamous, null)]
        public void GetCode_ShouldReturnCorrectCode(SiteLocation? location, Pathology? pathology, IcdCode? expectedCode)
        {
            var result = IcdCodes.GetCode(location, pathology);

            Assert.AreEqual(expectedCode, result);
        }
    }
}