using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using Xcc.Application.Common;
using Xcc.Core.Enums;

namespace Heracles.Application.UI.Converters
{
    public class PatientIdTypeConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Binding.DoNothing;

            switch (value.ToString())
            {
                case "SOCIAL_SECURITY_NUMBER":
                    return PatientIdType.Ssn;
                case "PASSPORT_NUMBER":
                    return PatientIdType.Passport;
                default:
                    return Binding.DoNothing;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var patientIdType = value as PatientIdType?;

            if (patientIdType.HasValue == false)
                return Binding.DoNothing;
            
            return patientIdType.GetAttributeOfType<DescriptionAttribute>().Description;
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
