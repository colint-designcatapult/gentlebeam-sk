using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Xcc.Application.UI.Converters
{
    public class InverseBoolAndValueMatchConverter : MarkupExtension, IMultiValueConverter
    {       
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is not { Length: 2 or 3 } || values[0] is not bool boolValue)
                return DependencyProperty.UnsetValue;

            if (values.Length == 3)
            {
                if (values[2] is not bool operationAllowed)
                    return DependencyProperty.UnsetValue;
                if (!operationAllowed)
                    return false;
            }

            return !boolValue || Equals(values[1], parameter);

        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //todo: not implemented
            return Array.Empty<object>(); 
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
