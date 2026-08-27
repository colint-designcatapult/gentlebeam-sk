using Heracles.Application.Helpers;
using Heracles.Core.Enums;

namespace Heracles.Application.Test.Helpers
{
    public class TargetTypeConverterTests
    {
        [Test]
        public void GetIndexToTreatmentFieldNameMappingTest([Values]TargetType targetType)
        {
            // We don't have any field mapping only for the technical 'None' type and the 61_Fields type
            // QC_Collimator does have mapping (returns TargetType_CircularCell)
            if (targetType == TargetType.TargetType_None ||
                targetType == TargetType.TargetType_61_Fields)
            {
                Assert.That(TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(targetType), Is.Null);
            }
            else
            {
                Assert.That(TargetTypeConverter.GetIndexToTreatmentFieldNameMapping(targetType), Is.Not.Null);
            }
        }

        [Test]
        public void GetBackwardFieldNameMapping_PositiveTest()
        {
            int value = 1;
            var name = TreatmentFieldName.Plus4C;
            var mapping = new Dictionary<int, TreatmentFieldName>() { { value, name } };
            Assert.That(value, Is.EqualTo(TargetTypeConverter.GetBackwardFieldNameMapping(mapping, name)));
        }

        [Test]
        public void GetBackwardFieldNameMapping_NegativeTest()
        {
            int value = 1;
            var name = TreatmentFieldName.Plus4C;
            var mapping = new Dictionary<int, TreatmentFieldName>() { { value, name } };

            Assert.Throws<ArgumentNullException>(() => TargetTypeConverter.GetBackwardFieldNameMapping(null, name));
            Assert.Throws<InvalidOperationException>(() => TargetTypeConverter.GetBackwardFieldNameMapping(mapping, name + 1));
        }
    }
}
