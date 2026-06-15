using Heracles.Core.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Heracles.Application.UI.Converters
{
    public class EnergyEnumToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Energy energy)
            {
                return ToString(energy);
            }

            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Binding.DoNothing;

            if (Enum.TryParse(typeof(Energy), "Energy_" + value.ToString(), out var result))
            {
                return result;
            }
            else
            {
                return Binding.DoNothing;
            }
        }

        public static string ToString(Energy energy)
        {
            if (!Enum.IsDefined(energy))
                return "0";

            return energy.ToString().Replace("Energy_", "");
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
