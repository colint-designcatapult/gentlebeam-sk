using System.Globalization;
using System.Windows.Controls;
using Xcc.Core.Constants;

namespace Xcc.Application.UI.ValidationRules
{
    public class FloatRangeRule : ValidationRule
    {
        public float Min { get; set; } = float.MinValue;
        public float Max { get; set; } = float.MaxValue;
        public string? InvalidRangeMessage { get; set; }

        public FloatRangeRule()
        {
        }

        public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
        {
            NumberFormatInfo numberFormat = cultureInfo.NumberFormat;

            var stringValue = value?.ToString();

            if(string.IsNullOrWhiteSpace(stringValue))
                return new ValidationResult(false, StringConstants.Common.Validation.StringIsNullOrEmpty);

            if (stringValue.EndsWith(numberFormat.NumberDecimalSeparator))
                stringValue = stringValue[..^1];


            if (float.TryParse(stringValue, NumberStyles.Float, cultureInfo, out float floatValue) == false)
                return new ValidationResult(false, StringConstants.Common.Validation.NotANumberError);


            if ((floatValue < Min) || (floatValue > Max))
            {
                return new ValidationResult(false, InvalidRangeMessage ?? $"{StringConstants.Common.Validation.ValueRangeRequest}: {Min}-{Max}");
            }

            return ValidationResult.ValidResult;
        }
    }
}