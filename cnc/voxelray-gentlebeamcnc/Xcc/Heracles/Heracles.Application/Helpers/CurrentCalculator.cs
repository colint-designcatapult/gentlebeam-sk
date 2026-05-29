using Heracles.Core.Enums;

namespace Heracles.Application.Helpers
{
    public static class CurrentCalculator
    {
        public static double HvpsPower50kV = 200.0;
        public static double HvpsPower70kV = 200.0;
        public static double HvpsPower100kV = 200.0;

        public static double CalculateCurrent(Energy energy)
        {
            // Current = Power / Voltage,
            // where the Power is constant,
            // and the Voltage presents as the Energy
            double voltage = (int)energy;
            var power = energy switch
            {
                Energy.Energy_50 => HvpsPower50kV,
                Energy.Energy_70 => HvpsPower70kV,
                Energy.Energy_100 => HvpsPower100kV,
                _ => throw new System.NotImplementedException($"CurrentCalculator: the energy level {energy} is not supported"),
            };
            return power / voltage;
        }
    }
}
