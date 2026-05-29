using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class PatientNamesToSingleStringConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string firstName, lastName;
            string result = string.Empty;

            if (values != null && values[0] != DependencyProperty.UnsetValue && values[1] != DependencyProperty.UnsetValue)
            {
                firstName = values[0] as string;
                lastName = values[1] as string;
                
                if (!string.IsNullOrEmpty(firstName))
                {
                    result += firstName + ", ";
                }
                if (!string.IsNullOrEmpty(lastName))
                {
                    result += lastName;
                }
            }

            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
