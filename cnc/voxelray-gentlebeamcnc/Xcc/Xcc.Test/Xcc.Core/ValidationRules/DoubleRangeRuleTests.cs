using System.Globalization;
using Xcc.Core.Constants;
using Xcc.Core.ValidationRules;

namespace Xcc.Test.Xcc.Core.ValidationRules
{
    public class DoubleRangeRuleTests
    {
        [SetUp]
        public void SetUp()
        {
            G.SetupCulture();
        }
        
        [Test]
        public void DoubleRangeRule_Defaults()
        {
            var sut = new DoubleRangeRule();

            Assert.That(sut.Min, Is.EqualTo(0));
            Assert.That(sut.Max, Is.EqualTo(0));
            Assert.That(sut.InvalidRangeMessage, Is.Null);
        }

        [Test]
        public void Validate_OK_WhenInRange(
            [Values("5", "5.", "5.1")] string value)
        {
            var sut = new DoubleRangeRule { Min = 0, Max = 10 };
            var res = sut.Validate(value, G.Culture);

            Assert.That(res.IsValid, Is.True);
            Assert.That(res.ErrorContent, Is.Null);
        }

        [Test]
        public void Validate_Error_OutOfRange(
            [Values("-1", "11")] string value)
        {
            var sut = new DoubleRangeRule { Min = 0, Max = 10 };
            var res = sut.Validate(value, G.Culture);

            Assert.That(res.IsValid, Is.False);
            Assert.That(res.ErrorContent, Does.Contain(StringConstants.Common.Validation.ValueRangeRequest));
        }
        
        [Test]
        public void Validate_Error_EmptyString(
            [Values("", " ", "  ", "\n")] string value)
        {
            var sut = new DoubleRangeRule();
            var res = sut.Validate(value, G.Culture);

            Assert.That(res.IsValid, Is.False);
            Assert.That(res.ErrorContent, Is.EqualTo(StringConstants.Common.Validation.StringIsNullOrEmpty));
        }

        [Test]
        public void Validate_Error_NotANumber(
            [Values("abc", "1.asd")] string value)
        {
            var sut = new DoubleRangeRule();
            var res = sut.Validate("abc", G.Culture);

            Assert.That(res.IsValid, Is.False);
            Assert.That(res.ErrorContent, Is.EqualTo(StringConstants.Common.Validation.NotANumberError));
        }

        [Test]
        public void Validate_Error_OutOfRange_CustomMessage(
            [Values("-1", "11")] string value,
            [Values("Custom message", "ErrorRange")] string customMessage)
        {
            var sut = new DoubleRangeRule { Min = 0, Max = 10, InvalidRangeMessage = customMessage };
            var res = sut.Validate(value, G.Culture);

            Assert.That(res.IsValid, Is.False);
            Assert.That(res.ErrorContent, Is.EqualTo(customMessage));
        }
    }
}