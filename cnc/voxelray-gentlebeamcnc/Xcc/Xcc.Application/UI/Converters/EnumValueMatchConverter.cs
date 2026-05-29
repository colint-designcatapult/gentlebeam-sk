using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class EnumValueMatchConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object valueToMatch, CultureInfo culture)
        {
            if (value != null && value.GetType().IsEnum)
                return Enum.Equals(value, valueToMatch);
            else
                return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object valueToMatch, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
                return valueToMatch;
            else
            {
                if (targetType.IsEnum)
                {
                    var enumValues = Enum.GetValues(targetType);

                    if(enumValues.Length > 2)
                        throw new ArgumentException($"Target enum must contain exactly two values {targetType.Name}");

                    foreach (var enumValue in enumValues)
                    {
                        if (!Enum.Equals(valueToMatch, enumValue))
                            return enumValue;
                    }

                    throw new ArgumentException($"Target enum must contain exactly two values {targetType.Name}");
                }
                else
                {
                    throw new ArgumentException($"Target must be an enum type. Current type is {targetType.Name}");
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }

    public class ObjectIsNotEqualConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object valueToMatch, CultureInfo culture)
        {
            return !Equals(value, valueToMatch);
        }

        public object ConvertBack(object value, Type targetType, object valueToMatch, CultureInfo culture) => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
