using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Xcc.Core.Constants;

namespace Xcc.Application.UI.ValidationRules
{
    public class DigitsAndDashesRule : ValidationRule
    {
        public bool IsRequired { get; set; } = false;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                if (IsRequired)
                    return new ValidationResult(false, StringConstants.Common.Validation.FieldRequiredError);
                else
                    return ValidationResult.ValidResult;
            }

            var str = (string)value;

            if (str.StartsWith('-'))
                return new ValidationResult(false, StringConstants.Common.Validation.NumericStartsWithDashError);

            if (str.EndsWith('-'))
                return new ValidationResult(false, StringConstants.Common.Validation.NumericEndsWithDashError);

            if (Regex.IsMatch(str, "^([0-9-]+)$") == false)
                return new ValidationResult(false, StringConstants.Common.Validation.NumericInvalidCharacterError);

            //Strings containing "-" twice in a row counts as non-valid
            if (Regex.IsMatch(str, @"[-]{2,}") == true)
                return new ValidationResult(false, StringConstants.Common.Validation.NumericTwoDashesError);

            return ValidationResult.ValidResult;
        }
    }
}
