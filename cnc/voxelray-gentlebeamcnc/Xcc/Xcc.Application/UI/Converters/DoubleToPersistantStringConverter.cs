using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    [ValueConversion(typeof(double), typeof(string))]
    public class DoubleToPersistantStringConverter : MarkupExtension, IValueConverter
    {
        public string StringFormat { set; get; } = "{0:0.00}";

        private string? lastConvertBackString;

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            if (!double.TryParse(value?.ToString(), out double dValue))
                return null;

            var stringValue = lastConvertBackString ?? string.Format(culture, StringFormat, (double)value);
            lastConvertBackString = null;

            return stringValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            var valueAsString = value?.ToString();

            if (double.TryParse(valueAsString, NumberStyles.Float, culture, out double result))
            {
                lastConvertBackString = valueAsString;
                return result;
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class DoubleToPersistantStringPreciseConverter : DoubleToPersistantStringConverter
    { 
        public DoubleToPersistantStringPreciseConverter() 
        {
            StringFormat = "{0}";
        }
    }
}
