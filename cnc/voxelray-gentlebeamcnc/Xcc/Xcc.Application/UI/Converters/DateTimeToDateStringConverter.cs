using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class DateTimeToDateStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = value as DateTime?;

            return dateTime?.ToShortDateString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Binding.DoNothing;

            if (DateOnly.TryParse(value.ToString(), out DateOnly date))
                return date.ToDateTime(new TimeOnly(0));

            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
