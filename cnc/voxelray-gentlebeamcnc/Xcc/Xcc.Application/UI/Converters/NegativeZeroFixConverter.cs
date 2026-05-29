using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class NegativeZeroFixConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
        {
            if (Math.Abs(d) < 0.005)
                d = 0.0;

            return d.ToString("0.00", culture);
        }

        return value;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new Exception($"{nameof(NegativeZeroFixConverter)}: {nameof(ConvertBack)} is not supported for this converter.");

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}