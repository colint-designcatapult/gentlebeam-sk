using Heracles.Core.Enums;

namespace Heracles.Application.UI.Converters
{
    public class EnergyEnumToIntConverter
    {
        public static int Convert(Energy energy)
        {
            return System.Convert.ToInt32(EnergyEnumToStringConverter.ToString(energy));
        }
    }
}
