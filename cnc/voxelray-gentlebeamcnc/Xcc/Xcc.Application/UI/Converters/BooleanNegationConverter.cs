using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class BooleanNegationConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return true; // default enabled if not a bool
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}