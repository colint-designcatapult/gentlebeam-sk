using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Xcc.Application.Common;

public class IntAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (int.TryParse(value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out _) == false)
        {
            ErrorMessage = "Please enter a valid integer number";
            return false;
        }

        return true;
    }
}