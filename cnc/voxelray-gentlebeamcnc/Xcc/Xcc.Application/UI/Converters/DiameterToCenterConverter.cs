using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xcc.Application.UI.Converters
{
    public class DiameterToCenterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var width = (double)values[0];
            var height = (double)values[1];

            Point center = new Point((int)(width / 2.0), (int)(height / 2));

            return center;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { 0.0, 0.0 };
        }
    }
}
