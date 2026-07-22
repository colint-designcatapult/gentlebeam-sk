using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class BooleanToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility Invisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null || value == DependencyProperty.UnsetValue)
                return Invisibility;

            if (value is bool valueAsBool)
            {
                return valueAsBool ? Visibility.Visible : Invisibility;
            }

            throw new ArgumentException($"{nameof(BooleanToVisibilityConverter)}: value must be of type {nameof(Boolean)}.");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
