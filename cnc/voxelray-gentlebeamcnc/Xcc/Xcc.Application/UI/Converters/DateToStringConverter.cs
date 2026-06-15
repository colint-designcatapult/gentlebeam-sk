using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class DateToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = value as DateOnly?;

            if (dateTime is null) return string.Empty;

            return dateTime.Value.ToString("MM/dd/yyyy", culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Binding.DoNothing;

            if (DateOnly.TryParseExact(value.ToString(), "MM/dd/yyyy", out DateOnly date))
                return date;

            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class DateRangeToStringConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = value as DateOnly?;

            if (dateTime is null) return String.Empty;

            return dateTime.Value.ToString("MM/dd/yyyy", culture);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string message = "Date range is not set";

            if (values is null || values.Length < 2)
                return message;

            var fromDate = values[0] as DateTime?;
            var toDate = values[1] as DateTime?;

            if (fromDate is null)
                return message;

            if(toDate is null)
                return message;

            return $"{fromDate.Value.ToString("MMM d, yyyy", culture)} - {toDate.Value.ToString("MMM d, yyyy", culture)}";
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
