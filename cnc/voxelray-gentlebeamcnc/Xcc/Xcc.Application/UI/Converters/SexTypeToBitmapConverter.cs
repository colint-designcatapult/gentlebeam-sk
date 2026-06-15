using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using Xcc.Core.Enums;

namespace Xcc.Application.UI.Converters
{
    public class SexTypeToBitmapConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sexValue = value as Sex?;

            if (sexValue is null)
                return Binding.DoNothing;

            switch (sexValue)
            {
                case Sex.Male:
                    return new BitmapImage( new Uri("pack://application:,,,/Xcc.Application;component/UI/Resources/Images/male.png"));
                case Sex.Female:
                    return new BitmapImage(new Uri("pack://application:,,,/Xcc.Application;component/UI/Resources/Images/female.png"));
                default:
                    return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

}
