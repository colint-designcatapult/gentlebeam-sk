using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class MillisecondsToDurationStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if(double.TryParse(value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double milliseconds))
            {
                return TimeSpan.FromMilliseconds(milliseconds).ToString();
            }
            else
            {
                throw new ArgumentException($"{nameof(MillisecondsToDurationStringConverter)}: value must be of type {nameof(Double)}.");
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;

        public override object? ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
