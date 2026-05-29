using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbActionCompletionEventArgsTests
    {
        [TestCase(GcbActionType.NewSession)]
        [TestCase(GcbActionType.StartBeamOn)]
        [TestCase(GcbActionType.OnePointCompleted)]
        [TestCase(GcbActionType.Stop)]
        public void GcbActionCompletionEventArgs_GettersSetters(GcbActionType actionType)
        {
            var sut = new GcbActionCompletionEventArgs { ActionType = actionType};

            Assert.That(sut.ActionType, Is.EqualTo(actionType));
        }
    }
}