using Xcc.Core.Helpers;

namespace Xcc.Test.Xcc.Core.Helpers
{
    public class MathHelpersTests
    {
        [Test]
        public void ConvertDegreesToRadians()
        {
            Assert.That(MathHelpers.ConvertDegreesToRadians(0), Is.EqualTo(0).Within(G.Precision));
            Assert.That(MathHelpers.ConvertDegreesToRadians(90), Is.EqualTo(Math.PI / 2).Within(G.Precision));
            Assert.That(MathHelpers.ConvertDegreesToRadians(180), Is.EqualTo(Math.PI).Within(G.Precision));
            Assert.That(MathHelpers.ConvertDegreesToRadians(360), Is.EqualTo(2 * Math.PI).Within(G.Precision));
            Assert.That(MathHelpers.ConvertDegreesToRadians(-45), Is.EqualTo(-Math.PI / 4).Within(G.Precision));
            Assert.That(MathHelpers.ConvertDegreesToRadians(-90), Is.EqualTo(-Math.PI / 2).Within(G.Precision));
            Assert.That(MathHelpers.ConvertDegreesToRadians(-180), Is.EqualTo(-Math.PI).Within(G.Precision));
        }
        
        [Test]
        public void ConvertRadiansToDegrees()
        {
            Assert.That(MathHelpers.ConvertRadiansToDegrees(0), Is.EqualTo(0).Within(G.Precision));
            Assert.That(MathHelpers.ConvertRadiansToDegrees(Math.PI / 2), Is.EqualTo(90).Within(G.Precision));
            Assert.That(MathHelpers.ConvertRadiansToDegrees(Math.PI), Is.EqualTo(180).Within(G.Precision));
            Assert.That(MathHelpers.ConvertRadiansToDegrees(2 * Math.PI), Is.EqualTo(360).Within(G.Precision));
            Assert.That(MathHelpers.ConvertRadiansToDegrees(-Math.PI / 4), Is.EqualTo(-45).Within(G.Precision));
            Assert.That(MathHelpers.ConvertRadiansToDegrees(-Math.PI / 2), Is.EqualTo(-90).Within(G.Precision));
            Assert.That(MathHelpers.ConvertRadiansToDegrees(-Math.PI), Is.EqualTo(-180).Within(G.Precision));
        }
    }
}