using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class ActualFieldsCountToTextConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out int count) == false || count == 0)
            {
                return "No fields";
            }

            if(count == 1)
                return $"{count} field";

            return $"{count} fields";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}