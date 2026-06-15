using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using Xcc.Application.Common;

namespace Xcc.Application.UI.Converters
{
    public class EnumValueToDisplayNameConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            Enum? enumValue = value as Enum;
            if (enumValue != null)
            {
                string? displayName = enumValue.GetAttribute<DisplayAttribute>()?.Name;
                return displayName ?? Binding.DoNothing;
            }

            return Binding.DoNothing;
        }

        private static Enum FindValueByDisplayName(Type targetType, string displayName)
        {
            var values = Enum.GetValues(targetType) as Enum[];
            Enum? firstMatchingValue = values?.First(v => displayName.Equals(v.GetAttribute<DisplayAttribute>()?.Name, StringComparison.Ordinal));
            if (firstMatchingValue != null)
            {
                return firstMatchingValue;
            }

            throw new NullReferenceException("EnumValueToDisplayNameConverter value lookup error: can't find the display value");
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var valueString = value as string;
            if (valueString is null)
                return Binding.DoNothing;

            return FindValueByDisplayName(targetType, valueString);
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
