using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class RemainTimeCalculationConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                throw new ArgumentException($"'{nameof(RemainTimeCalculationConverter)}': at least two parameters must be specified.");

            if (!double.TryParse(values[0]?.ToString(), out double first))
                throw new ArgumentException($"'{nameof(RemainTimeCalculationConverter)}': the first parameter specified could not be converted to a number.");

            if (!double.TryParse(values[1]?.ToString(), out double second))
                throw new ArgumentException($"'{nameof(RemainTimeCalculationConverter)}': the second parameter specified could not be converted to a number.");

            return Math.Max(0.0, first - second); //if result < 0, return 0
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
