using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class BoolToAsteriskConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object rawValue, Type targetType, object parameter, CultureInfo culture)
        {
            if(rawValue is bool value)
            {
                return value ? "*" : String.Empty;
            }

            return String.Empty;
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
