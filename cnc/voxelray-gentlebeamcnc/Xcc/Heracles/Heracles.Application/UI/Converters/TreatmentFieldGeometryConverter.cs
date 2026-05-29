using Heracles.Core.Enums;
using Heracles.Core.Models.EMR;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class TreatmentFieldToGeometryConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type type, object parameter, CultureInfo culture)
        {
            var resource = new ResourceDictionary
            {
                Source = new Uri("/Heracles.Application;component/UI/UserControls/HoneycombList.xaml", UriKind.RelativeOrAbsolute)
            };

            if(values is null || values.Length < 2)
                throw new ArgumentException("ITreatmentField and TargetType values must specified for this converter.");

            if (values[0] is not ITreatmentField treatmentField)
                throw new ArgumentException("ITreatmentField value is not specified for this converter.");

            if (values[1] is not TargetType targetType)
                throw new ArgumentException("TargetType value is not specified for this converter.");

            switch (treatmentField.Name)
            {
                case TreatmentFieldName.PlusC 
                    when targetType is TargetType.TargetType_50mm_SSD_13_Fields:
                    return resource["LargeCentralCell"];

                case TreatmentFieldName.PlusC when 
                    targetType is 
                        TargetType.TargetType_50mm_SSD_15mm_Field or 
                        TargetType.TargetType_50mm_SSD_20mm_Field or 
                        TargetType.TargetType_50mm_SSD_30mm_Field or 
                        TargetType.TargetType_50mm_SSD_40mm_Field or
                        TargetType.TargetType_50mm_SSD_50mm_Field:
                    return resource["CircleIcon"];

                default:
                    return resource["HexCell"];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
