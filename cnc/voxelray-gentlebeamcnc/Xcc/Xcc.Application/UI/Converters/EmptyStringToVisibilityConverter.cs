using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

public class EmptyStringToVisibilityConverter : MarkupExtension, IValueConverter
{
    public Visibility Invisibility { get; set; } = Visibility.Collapsed;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Invisibility : Visibility.Visible;
        }

        return Invisibility;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new Exception($"{nameof(EmptyStringToVisibilityConverter)}: {nameof(ConvertBack)} is not supported for this converter.");

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}