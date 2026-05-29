using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class BooleanToColumnWidthConverter : MarkupExtension, IValueConverter
    {
        public GridLength Width { get; set; } = new GridLength(1.0, GridUnitType.Star);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return (bool)value ? Width : 0;
            }

            return Width;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class ObjectIsNullToColumnWidthConverter : MarkupExtension, IValueConverter
    {
        public GridLength Width { get; set; } = new GridLength(1.0, GridUnitType.Star);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? Width : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
