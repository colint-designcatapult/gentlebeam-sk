using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class InterlockFaultEntryTests
    {
        [Test]
        public void InterlockFaultEntry_Ctor()
        {
            var states = new List<InterlockState>
            {
                new InterlockState(GcbInterlockFlags.IonPump, true, false),
                new InterlockState(GcbInterlockFlags.Door, false, true)
            };
            
            var sut = new InterlockFaultEntry(states);
            var res = sut.FailedInterlocks;

            Assert.That(sut.FailedInterlocks, Is.SameAs(states));
        }
        
        [Test]
        public void InterlockFaultEntry_ToString()
        {   
            var states = new List<InterlockState>
            {
                new InterlockState(GcbInterlockFlags.IonPump, true, false),
                new InterlockState(GcbInterlockFlags.Door, false, true)
            };
            
            var sut = new InterlockFaultEntry(states);
            var res = sut.ToString();

            Assert.That(res, Does.Contain(states[0].ToString()));
            Assert.That(res, Does.Contain(states[1].ToString()));
        }
    }
}