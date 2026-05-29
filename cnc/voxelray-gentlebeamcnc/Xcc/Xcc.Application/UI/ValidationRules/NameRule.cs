using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Xcc.Core.Constants;

namespace Xcc.Application.UI.ValidationRules
{
    public class NameRule : ValidationRule
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

            var name = (string)value;

            //Strings with trailing or leading "-" " " "'" counts as non-valid. Example: "Joe-" "d'" " Billy "
            if (name.EndsWith(' '))
                return new ValidationResult(false, StringConstants.Common.Validation.NameEndsWithWhitespaceError);

            if (name.StartsWith('-'))
                return new ValidationResult(false, StringConstants.Common.Validation.NameStartsWithDashError);

            if (name.EndsWith('-'))
                return new ValidationResult(false, StringConstants.Common.Validation.NameEndsWithDashError);

            if (name.StartsWith('\''))
                return new ValidationResult(false, StringConstants.Common.Validation.NameStartsWithApostropheError);

            if (name.EndsWith('\''))
                return new ValidationResult(false, StringConstants.Common.Validation.NameEndsWithApostropheError);

            if (name.StartsWith('.'))
                return new ValidationResult(false, StringConstants.Common.Validation.NameStartsWithPeriodError);

            //Valid string examples: "Joe" "Jr." "Billy-Joe" "d'Artagnan". Letters can be any letter from any language.
            //Strings containing "," counts as non-valid.
            if (Regex.IsMatch((string)value, @"^[\p{L}\.' -]+$") == false)
                return new ValidationResult(false, StringConstants.Common.Validation.NameInvalidCharacterError);

            //Strings containing any "-" " " "'" twice in a row counts as non-valid
            if (Regex.IsMatch((string)value, @"[\.' -]{2,}") == true)
                return new ValidationResult(false, StringConstants.Common.Validation.NameInvalidError);

            return ValidationResult.ValidResult;
        }
    }
}
