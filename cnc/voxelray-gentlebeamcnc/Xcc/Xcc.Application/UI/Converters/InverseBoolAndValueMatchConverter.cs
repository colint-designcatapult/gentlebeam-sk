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
            if (values != null && values.Length == 2)
            {
                if (values[0] is bool boolValue)
                {
                    if (!boolValue)
                        return true;
                    else
                    {
                        return values[1].Equals(parameter);
                    }
                }
            }

            return DependencyProperty.UnsetValue;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            //todo: not implemented
            return Array.Empty<object>(); 
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
