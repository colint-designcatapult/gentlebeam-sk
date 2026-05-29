using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class FloatToPersistantStringConverter : MarkupExtension, IValueConverter
    {
        public string StringFormat { set; get; } = "{0}";

        private string? _convertBackString;

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!float.TryParse(value?.ToString(), out float dValue))
                return null;

            var stringValue = _convertBackString ?? string.Format(culture, StringFormat, (float)value);
            _convertBackString = null;

            return stringValue;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is not string str)
                return null;

            if (float.TryParse(str, NumberStyles.Float, culture, out float result))
            {
                _convertBackString = str;
                return result;
            }

            return null;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
