using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;

namespace Xcc.Application.Common;

public class DoubleAttribute : ValidationAttribute
{
    public static NumberStyles DoubleStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign | NumberStyles.AllowLeadingWhite;
    
    public override bool IsValid(object? value)
    {
        var valueAsString = value?.ToString();

        if (valueAsString is null)
            return true;

        if(double.TryParse(valueAsString, DoubleStyle, CultureInfo.CurrentCulture, out var valueAsDouble) == false)
        {
            ErrorMessage = "Please enter a valid number";
            return false;
        }

        if (valueAsString.StartsWith("-") && valueAsDouble == 0)
        {
            ErrorMessage = "Please enter a valid number";
            return false;
        }

        return true;
    }
}

public class DeniedDoubleValuesAttribute(params double[] deniedValues) : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        var valueAsString = value?.ToString();

        if (valueAsString is null)
            return true;

        if (double.TryParse(valueAsString, DoubleAttribute.DoubleStyle, CultureInfo.CurrentCulture, out var valueAsDouble))
        {
            return !deniedValues.Contains(valueAsDouble);
        }

        return true;
    }
}