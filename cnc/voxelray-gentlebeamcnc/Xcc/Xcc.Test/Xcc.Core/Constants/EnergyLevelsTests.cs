using System.Globalization;
using Xcc.Core.Constants;
using Xcc.Core.ValidationRules;

namespace Xcc.Test.Xcc.Core.Constants
{
    public class EnergyLevelsTests
    {
        [Test]
        public void Defaults()
        {
            var expected = new[] { 50, 60, 70, 80, 90, 100, 110, 120 };
            var actual = EnergyLevels.AvailableLevels;

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Cached()
        {
            var firstCall = EnergyLevels.AvailableLevels;
            var secondCall = EnergyLevels.AvailableLevels;

            Assert.That(firstCall, Is.SameAs(secondCall));
        }
    }
}