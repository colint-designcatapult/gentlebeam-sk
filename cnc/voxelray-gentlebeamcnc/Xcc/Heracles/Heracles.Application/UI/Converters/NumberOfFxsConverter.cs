using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class NumberOfFxsConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string fx = "Fx ";

            if (values.Length > 0 && values[0] != null)
                fx += values[0];       // Delivered Fx number

            if (values.Length > 1 && values[1] != null)
                fx += "/" + values[1]; // Prescribed Fx number

            return fx;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
