namespace Xcc.Core.Constants
{
    public static class EnergyLevels
    {
        private static int[]? _availableLevels;
        public static int[] AvailableLevels => _availableLevels ??= [50, 60, 70, 80, 90, 100, 110, 120];
    }
}
