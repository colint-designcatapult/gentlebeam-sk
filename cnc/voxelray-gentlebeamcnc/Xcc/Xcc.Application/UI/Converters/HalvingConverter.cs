using System;
using System.Globalization;
using System.Windows.Data;

namespace Xcc.Application.UI.Converters
{
    public class HalvingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? (double) value / 2.0 : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0.0;
        }
    }
}
