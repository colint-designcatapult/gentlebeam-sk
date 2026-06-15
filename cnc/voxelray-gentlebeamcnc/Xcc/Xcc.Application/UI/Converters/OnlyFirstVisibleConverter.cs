using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters;

/// <summary>
/// Converts multiple Visibility values to Visibility.Visible,
/// if only the first one is Visible
/// </summary>
public class OnlyFirstVisibleConverter : MarkupExtension, IMultiValueConverter
{
    public Visibility Invisibility { get; set; } = Visibility.Collapsed;

    public object Convert(object[]? values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values is null || values.Length == 0)
            return Invisibility;

        Visibility first = values[0] is Visibility v ? v : Invisibility;
        if (first == Invisibility)
            return Invisibility;

        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] is not Visibility visibility)
                continue;
                
            // if any other value is Visible, return Invisibility
            if (visibility == Visibility.Visible)
                return Invisibility;
        }

        return first == Visibility.Visible ? Visibility.Visible : Invisibility;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}