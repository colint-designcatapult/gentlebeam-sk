using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class ObjectToVisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility Invisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return Invisibility;

            if (parameter is null)
                return Visibility.Visible;

            return value.Equals(parameter) ? Visibility.Visible : Invisibility;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new Exception($"{nameof(ObjectToVisibilityConverter)}: {nameof(ConvertBack)} is not supported for this converter.");

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class ObjectToInvisibilityConverter : MarkupExtension, IValueConverter
    {
        public Visibility Invisibility { get; set; } = Visibility.Collapsed;

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is null)
                return Visibility.Visible;
             
            if (parameter is null)
                return Invisibility;

            return value.Equals(parameter) ? Invisibility : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new Exception($"{nameof(ObjectToInvisibilityConverter)}: {nameof(ConvertBack)} is not supported for this converter.");

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

}
