using Heracles.Application.Helpers;
using Heracles.Core.Enums;

namespace Heracles.Application.Test.Helpers
{
    public class CurrentCalculatorTests
    {
        [TestCase(Energy.Energy_50, 2.0)]
        [TestCase(Energy.Energy_70, 200.0 / 70.0)]
        [TestCase(Energy.Energy_100, 2.6)]
        public void CalculateCurrent_UsesEnergySpecificPower(Energy energy, double expectedCurrent)
        {
            Assert.That(CurrentCalculator.CalculateCurrent(energy), Is.EqualTo(expectedCurrent).Within(1e-9));
        }
    }
}
