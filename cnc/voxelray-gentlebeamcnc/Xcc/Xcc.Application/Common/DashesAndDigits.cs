using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Xcc.Core.Constants;

namespace Xcc.Application.Common;

public class DashesAndDigits : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not string valueAsString)
            return false;

        if (string.IsNullOrEmpty(valueAsString))
            return true;

        if (valueAsString.StartsWith('-'))
        {
            ErrorMessage = StringConstants.Common.Validation.NumericStartsWithDashError;
            return false;
        }

        if (valueAsString.EndsWith('-'))
        {
            ErrorMessage = StringConstants.Common.Validation.NumericEndsWithDashError;
            return false;
        }

        if (Regex.IsMatch(valueAsString, "^([0-9-]+)$") == false)
        {
            ErrorMessage = StringConstants.Common.Validation.NumericInvalidCharacterError;
            return false;
        }

        //Strings containing "-" twice in a row counts as non-valid
        if (Regex.IsMatch(valueAsString, @"[-]{2,}") == true)
        {
            ErrorMessage = StringConstants.Common.Validation.NumericTwoDashesError;
            return false;
        }

        return true;
    }
}