using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class SecondsToMinutesConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int seconds)
        {
            double minutes = seconds / 60.0;
            return $"{minutes:F1}";
        }

        return "N/A";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new Exception($"{nameof(SecondsToMinutesConverter)}: {nameof(ConvertBack)} is not supported for this converter.");

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}