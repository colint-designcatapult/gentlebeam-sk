using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class InterlockStateTests
    {
        [Test]
        public void InterlockState_Ctor(
            [Values(GcbInterlockFlags.Door, GcbInterlockFlags.DriveSystem)] GcbInterlockFlags interlock,
            [Values(false, true)] bool expected,
            [Values(false, true)] bool actual)
        {
            var sut = new InterlockState(interlock, expected, actual);

            Assert.That(sut.Interlock, Is.EqualTo(interlock));
            Assert.That(sut.Expected, Is.EqualTo(expected));
            Assert.That(sut.Actual, Is.EqualTo(actual));
        }
        
        [Test]
        public void FaultEntry_ToString_Door()
        {
            var sut = new InterlockState(GcbInterlockFlags.Door, false, false);
            var res = sut.ToString();
            
            Assert.That(res, Does.Contain("Interlock: Door")); 
            Assert.That(res, Does.Contain("Expected: False")); 
            Assert.That(res, Does.Contain("Actual: False")); 
        }
        
        [Test]
        public void FaultEntry_ToString_DriveSystem()
        {
            var sut = new InterlockState(GcbInterlockFlags.DriveSystem, true, true);
            var res = sut.ToString();
            
            Assert.That(res, Does.Contain("Interlock: DriveSystem")); 
            Assert.That(res, Does.Contain("Expected: True")); 
            Assert.That(res, Does.Contain("Actual: True")); 
        }
    }
}