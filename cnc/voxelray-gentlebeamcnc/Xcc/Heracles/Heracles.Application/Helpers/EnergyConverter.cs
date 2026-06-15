using Heracles.Core.Enums;
using System.ComponentModel.DataAnnotations;
using Xcc.Application.Common;

namespace Heracles.Application.Helpers
{
    public static class EnergyConverter
    {
        public static int Convert(Energy energy)
        {
            return int.Parse(energy.GetAttribute<DisplayAttribute>().Name);
        }
    }
}
