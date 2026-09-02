using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbSessionTests
    {
        [TestCase(0u)]
        [TestCase(1u)]
        public void ConstructorRetainsSessionId(uint id)
        {
            var sut = new GcbSession(id);

            Assert.That(sut.Id, Is.EqualTo(id));
        }
    }
}