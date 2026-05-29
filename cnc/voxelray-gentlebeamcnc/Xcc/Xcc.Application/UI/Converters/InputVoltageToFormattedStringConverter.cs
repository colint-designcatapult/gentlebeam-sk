using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class InputVoltageToFormattedStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "0 VAC";

            if (value != null)
                result = ((double)value).ToString($"{0} VAC");

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 0.0;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
