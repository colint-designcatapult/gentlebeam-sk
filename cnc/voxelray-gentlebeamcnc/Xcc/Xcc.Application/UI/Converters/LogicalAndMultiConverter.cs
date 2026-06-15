using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Xcc.Application.UI.Converters
{
    public class LogicalAndMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Any(x => (x == null) || ((bool)x == false)) ? false : true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[2] { false, false };
        }
    }
}
