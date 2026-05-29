using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Xcc.Application.UI.Converters
{
    public class TextBoxWrapToAcceptsReturnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TextWrapping textWrapping = (TextWrapping)value;

            return textWrapping == TextWrapping.Wrap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TextWrapping.NoWrap;
        }
    }
}
