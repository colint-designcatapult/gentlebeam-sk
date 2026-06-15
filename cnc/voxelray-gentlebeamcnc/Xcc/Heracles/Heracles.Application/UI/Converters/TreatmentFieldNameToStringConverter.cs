using Heracles.Core.Constants;
using Heracles.Core.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class TreatmentFieldNameToStringConverter : MarkupExtension, IMultiValueConverter
    {
        public object Convert(object[] values, Type type, object parameter, CultureInfo culture)
        {
            if (values is null || values.Length < 2)
                throw new ArgumentException("ITreatmentField and TargetType values must specified for this converter.");

            if (values[0] is not TreatmentFieldName treatmentFieldName)
                throw new ArgumentException("TreatmentFieldName value is not specified for this converter.");

            if (values[1] is not TargetType targetType)
                throw new ArgumentException("TargetType value is not specified for this converter.");


            if(targetType is 
               TargetType.TargetType_50mm_SSD_15mm_Field or
               TargetType.TargetType_50mm_SSD_20mm_Field or
               TargetType.TargetType_50mm_SSD_30mm_Field or
               TargetType.TargetType_50mm_SSD_40mm_Field or
               TargetType.TargetType_50mm_SSD_50mm_Field)
            {
                return $"Ø {(int)targetType} mm";
            }

            var mapping = targetType switch
            {
                TargetType.TargetType_50mm_SSD_13_Fields => Mappings.TargetType_13CellsCentralLarge,
                TargetType.TargetType_30mm_SSD_7_Fields => Mappings.TargetType_30mmSsd7Fields,
                TargetType.TargetType_61_Fields => Mappings.TargetType_61Head,
                _ => Mappings.TargetType_None
            };
            
            foreach (var map in mapping)
            { 
                if(map.Value == treatmentFieldName)
                    return map.Key.ToString();
            }

            return Binding.DoNothing;
            //throw new ArgumentException($"Value {treatmentFieldName} is not found in Mappings.TargetType_13CellsCentralLarge.");
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotSupportedException();

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
