using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class MultiplyConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d && double.TryParse(parameter?.ToString(), out var factor))
                return d * factor;

            if (value is GridLength gl && double.TryParse(parameter?.ToString(), out factor))
                return new GridLength(gl.Value * factor, gl.GridUnitType);

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

}
