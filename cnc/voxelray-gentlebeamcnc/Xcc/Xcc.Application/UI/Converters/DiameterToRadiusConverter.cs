using System;
using System.Globalization;
using System.Windows.Data;

namespace Xcc.Application.UI.Converters
{
    public class DiameterToRadiusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var width = (double)values[0];
            var height = (double)values[1];

            return Math.Min(width, height) / 2.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { 0.0, 0.0 };
        }
    }
}
