using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Xcc.Application.UI.Converters
{
    public class PathToImageSourceConverter : MarkupExtension, IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is not string imageFileName)
                return null;

            if (string.IsNullOrWhiteSpace(imageFileName) || !File.Exists(imageFileName))
                return null;

            return new BitmapImage(new Uri(imageFileName));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
