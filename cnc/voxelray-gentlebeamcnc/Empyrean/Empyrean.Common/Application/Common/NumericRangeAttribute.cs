using System.ComponentModel.DataAnnotations;
using Empyrean.Common.Core.Constants;

namespace Empyrean.Common.Application.Common
{
    public class NumericRangeAttribute : RangeAttribute
    {
        public NumericRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        public NumericRangeAttribute(int minimum, int maximum) : base(minimum, maximum)
        {
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if ((Maximum is double maximum && Math.Abs(maximum - double.MaxValue) < double.Epsilon) ||
                Maximum is int and int.MaxValue)
            {
                ErrorMessage = string.Format(
                    StringConstants.Common.Validation.NumericMinRangeFormatString,
                    validationContext.DisplayName,
                    Minimum);
            }
            else
            {
                ErrorMessage = string.Format(
                    StringConstants.Common.Validation.NumericMinMaxRangeFormatString,
                    validationContext.DisplayName,
                    Minimum, Maximum);
            }

            return base.IsValid(value, validationContext);
        }
    }
}
