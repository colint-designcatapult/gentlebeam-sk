using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbSessionTests
    {
        [Test]
        public void GcbSession_Ctor(
            [Values(0u, 1u)] uint id,
            [Values(0, 1)] int totalPoints)
        {
            var sut = new GcbSession(id, totalPoints);

            Assert.That(sut.Id, Is.EqualTo(id));
            Assert.That(sut.TotalPoints, Is.EqualTo(totalPoints));
        }
    }
}