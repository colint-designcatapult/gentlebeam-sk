using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class HeightConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double totalHeight = 0;

            foreach (var item in values) 
            {
                if (double.TryParse(item.ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out double height))
                { 
                    totalHeight += height;
                }
            }

            return totalHeight;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
