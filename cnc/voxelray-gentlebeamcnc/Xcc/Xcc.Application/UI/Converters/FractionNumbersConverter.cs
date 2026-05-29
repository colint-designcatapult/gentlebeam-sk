using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class FractionNumberConverter : MarkupExtension, IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string mask = "/";

            if (value is null)
                return mask;

            var str = value.ToString();

            if (string.IsNullOrWhiteSpace(str))
                return mask;

            if (str.Contains(mask))
                str = str.Replace(mask, string.Empty);

            // Determines whether the odd character will be located - on the left or right side of the mask, depending on the number of characters in the source string
            // If the string contains only one symbol, this symbol is odd and will be located on the left side of the mask. For example: '1/'
            // If the string contains an odd number of characters, but more than one - last symbol will be located on the right side of the mask. Example: '1/23'
            // If the string contains an even number of characters - the left and right sides of the mask will have the same number of characters. Example: '12/34'
            var odd = str.Length == 1 ? 1 : 0;
            int leftLength = str.Length / 2 + odd;
            int rightLength = str.Length - leftLength;

            var left = str.Substring(0, leftLength);
            var right = rightLength == 0 ? string.Empty : str.Substring(leftLength, rightLength);

            return $"{left}{mask}{right}";
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
