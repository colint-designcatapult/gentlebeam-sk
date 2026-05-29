using System.Globalization;
using System.Windows.Controls;
using Xcc.Core.Constants;

namespace Xcc.Core.ValidationRules
{
    public class DoubleRangeRule : ValidationRule
    {
        public double Min { get; set; }
        public double Max { get; set; }
        public string? InvalidRangeMessage { get; set; }

        public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
        {
            NumberFormatInfo numberFormat = cultureInfo.NumberFormat;

            var stringValue = value?.ToString();

            if (string.IsNullOrWhiteSpace(stringValue))
                return new ValidationResult(false, StringConstants.Common.Validation.StringIsNullOrEmpty);

            if (stringValue.EndsWith(numberFormat.NumberDecimalSeparator))
                stringValue = stringValue[..^1];


            if(double.TryParse(stringValue, NumberStyles.Float, cultureInfo, out double doubleValue) == false)
                return new ValidationResult(false, StringConstants.Common.Validation.NotANumberError);


            if ((doubleValue < Min) || (doubleValue > Max))
            {
                return new ValidationResult(false, InvalidRangeMessage ?? $"{StringConstants.Common.Validation.ValueRangeRequest}: {Min}-{Max}");
            }

            return ValidationResult.ValidResult;
        }
    }
}