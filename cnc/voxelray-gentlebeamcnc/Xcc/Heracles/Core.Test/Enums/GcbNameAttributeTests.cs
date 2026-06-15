
using Heracles.Core.Enums;

namespace Heracles.Core.Test.Enums
{
    [TestFixture]
    public class GcbNameAttributeTests
    {
        [Test]
        public void GcbNameAttribute_ShouldThrowException()
        {
            Assert.Throws<ArgumentNullException>(() => new GCBNameAttribute(null));
        }
    }
}
