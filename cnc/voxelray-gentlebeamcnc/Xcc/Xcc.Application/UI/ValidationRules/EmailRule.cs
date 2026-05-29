using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Xcc.Core.Constants;

namespace Xcc.Application.UI.ValidationRules
{
    public class EmailRule : ValidationRule
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

            var email = (string)value;

            //Valid string examples: "v_example@mail.com" "123@exa-mple.c" "billy-joe@e.mail.d12"
            if (Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") == false)
                return new ValidationResult(false, StringConstants.Common.Validation.EmailParseError);

            return ValidationResult.ValidResult;
        }
    }
}
