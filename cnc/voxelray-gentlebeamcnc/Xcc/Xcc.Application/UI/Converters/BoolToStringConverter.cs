using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class BoolToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? rawValue, Type targetType, object? parameter, CultureInfo culture)
        {
            if (rawValue is bool value)
            {
                // Determine format from parameter
                string? format = parameter?.ToString()?.ToLowerInvariant();

                return format switch
                {
                    "binary" => value ? "1" : "0",
                    "lower" => value ? "true" : "false",
                    "upper" => value ? "TRUE" : "FALSE",
                    _ => value ? "True" : "False" // Default format
                };
            }

            return string.Empty;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}