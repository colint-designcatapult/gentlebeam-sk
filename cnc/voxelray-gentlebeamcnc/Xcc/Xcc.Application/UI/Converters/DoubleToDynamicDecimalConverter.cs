using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToDynamicDecimalConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!double.TryParse(value?.ToString(), out double d))
                return null;

            int digits = ParseDigitsFromParameter(parameter);
            string format = "{0:0." + new string('0', digits) + "}";
            return string.Format(culture, format, d);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (double.TryParse(value?.ToString(), NumberStyles.Float, culture, out double result))
                return result;
            return null;
        }

        private int ParseDigitsFromParameter(object? parameter)
        {
            return int.TryParse(parameter?.ToString(), out int digits) && digits >= 0 ? digits : 2;
        }
    }

}
