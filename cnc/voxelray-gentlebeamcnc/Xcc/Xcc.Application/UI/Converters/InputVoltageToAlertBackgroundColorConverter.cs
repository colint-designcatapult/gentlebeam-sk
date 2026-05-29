using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Xcc.Application.UI.Converters
{
    public class InputVoltageToAlertBackgroundColorConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush bg = new SolidColorBrush(Colors.Transparent);

            if (value != null)
                bg = (double)value >= 207.0 && (double)value <= 253 ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.DarkRed);

            return bg;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => 0;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
