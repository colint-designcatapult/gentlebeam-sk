using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Xcc.Application.Common;

public class DeniedValuesAttribute<T>(params T[] deniedValues) : ValidationAttribute
    where T : struct
{
    HashSet<string> deniedValueStrings = deniedValues.Select(x => x.ToString()!).Distinct().ToHashSet();
    public override bool IsValid(object? value)
    {
        var valueAsString = value?.ToString();

        if (valueAsString is null)
            return true;

        return !deniedValueStrings.Contains(valueAsString);
    }
}