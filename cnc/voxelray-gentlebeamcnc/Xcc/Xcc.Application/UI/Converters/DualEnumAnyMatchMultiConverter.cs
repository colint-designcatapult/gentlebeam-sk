using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

[ContentProperty(nameof(Values))]
public class EnumParamsExtension : MarkupExtension
{
    public List<object> Values { get; } = new List<object>();

    public EnumParamsExtension() { }

    public EnumParamsExtension(object value1, object value2)
    {
        Values.Add(value1);
        Values.Add(value2);
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return Values.ToArray();
    }
}

public class DualEnumAnyMatchMultiConverter :  MarkupExtension, IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not object[] expected || expected.Length != 2)
            return false;
        
        return values[0]?.Equals(expected[0]) == true ||
               values[1]?.Equals(expected[1]) == true;
    }


    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}