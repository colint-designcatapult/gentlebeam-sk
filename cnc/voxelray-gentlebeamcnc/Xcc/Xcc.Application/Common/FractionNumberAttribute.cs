using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Xcc.Core.Constants;

namespace Xcc.Application.Common;

public partial class FractionNumberAttribute : ValidationAttribute
{
    public const string Pattern = @"^(\d+)/(\d+)$";

    public override bool IsValid(object? value)
    {
        var valueAsString = value?.ToString();

        if (string.IsNullOrWhiteSpace(valueAsString))
        {
            ErrorMessage = StringConstants.EMR.Prescription.Validation.FractionIsRequired;
            return false;
        }

        Match match = FractionNumberRegex().Match(valueAsString);

        if (match.Success == false)
        {
            ErrorMessage = StringConstants.EMR.Prescription.Validation.FractionFormatErrorMessage;
            return false;
        }

        int num1 = int.Parse(match.Groups[1].Value);
        int num2 = int.Parse(match.Groups[2].Value);

        if (num1 > num2)
        {
            ErrorMessage = string.Format(StringConstants.EMR.Prescription.Validation.FractionCompareErrorMessage, num1, num2);
            return false;
        }


        return true;
    }


    [GeneratedRegex(Pattern)]
    private static partial Regex FractionNumberRegex();
}