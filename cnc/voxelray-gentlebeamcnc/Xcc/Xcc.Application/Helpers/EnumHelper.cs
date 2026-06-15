using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using Xcc.Application.UI.Converters;

namespace Xcc.Application.Helpers;

public static class EnumHelper
{
    private static readonly EnumValueToDisplayNameConverter Conv = new EnumValueToDisplayNameConverter();
        
    public static IEnumerable GetValuesSorted(Type enumType)
    {
        if (enumType == null)
            throw new ArgumentNullException(nameof(enumType));
        if (!enumType.IsEnum)
            throw new ArgumentException("Type must be an enum", nameof(enumType));
            
        var values = Enum.GetValues(enumType).Cast<object>();
        return values
            .OrderBy(v => (string)Conv.Convert(v, typeof(string), null!, CultureInfo.CurrentCulture))
            .ToList();
    }
}