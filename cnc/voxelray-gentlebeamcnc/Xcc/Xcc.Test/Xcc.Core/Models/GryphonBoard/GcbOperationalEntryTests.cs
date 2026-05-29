using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbOperationalEntryTests
    {
        [Test]
        public void GcbOperationalEntry_Defaults()
        {
            var sut = new GcbOperationalEntry();

            Assert.That(sut.PointIndex, Is.EqualTo(0));
            Assert.That(sut.Duration, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.ActualDuration, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.Current, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.Energy, Is.EqualTo(0));
            
            Assert.That(sut.FilamentSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.FocusCoilSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.XCoilSetpoint, Is.EqualTo(0).Within(G.Precision));
            Assert.That(sut.YCoilSetpoint, Is.EqualTo(0).Within(G.Precision));
        }
        
        [Test]
        public void GcbOperationalEntry_GettersSetters()
        {
            int pointIndex = 1;
            float duration = 1.2f;
            float actualDuration = 2.3f; 
            float current = 3.4f; 
            int energy = 5; 
            float filamentSetpoint = 6.7f; 
            float focusCoilSetpoint = 42.3f; 
            float xCoilSetpoint = 58.234f; 
            float yCoilSetpoint = 423.123f; 
                
            var sut = new GcbOperationalEntry
            {
                PointIndex = pointIndex, 
                Duration = duration, 
                ActualDuration = actualDuration, 
                Current = current, 
                Energy = energy, 
                FilamentSetpoint = filamentSetpoint, 
                FocusCoilSetpoint = focusCoilSetpoint, 
                XCoilSetpoint = xCoilSetpoint, 
                YCoilSetpoint = yCoilSetpoint, 
            };

            Assert.That(sut.PointIndex, Is.EqualTo(pointIndex));
            Assert.That(sut.Duration, Is.EqualTo(duration).Within(G.Precision));
            Assert.That(sut.ActualDuration, Is.EqualTo(actualDuration).Within(G.Precision));
            Assert.That(sut.Current, Is.EqualTo(current).Within(G.Precision));
            Assert.That(sut.Energy, Is.EqualTo(energy));
            
            Assert.That(sut.FilamentSetpoint, Is.EqualTo(filamentSetpoint).Within(G.Precision));
            Assert.That(sut.FocusCoilSetpoint, Is.EqualTo(focusCoilSetpoint).Within(G.Precision));
            Assert.That(sut.XCoilSetpoint, Is.EqualTo(xCoilSetpoint).Within(G.Precision));
            Assert.That(sut.YCoilSetpoint, Is.EqualTo(yCoilSetpoint).Within(G.Precision));
        }
        
        [Test]
        public void VersionInfo_ToString()
        {   
            var sut = new VersionInfo
            {
                Major = 10,
                Minor = 13,
                Level = 16,
                FirmwareChecksum = 42,
                Mode = FirmwareMode.Demo,
            };

            var res = sut.ToString();
            
            Assert.That(res, Does.Contain("Version: 10.13"));
            Assert.That(res, Does.Contain("Level: 16"));
            Assert.That(res, Does.Contain("FirmwareChecksum: 42"));
            Assert.That(res, Does.Contain("Mode: Demo"));
            Assert.That(res, Does.Contain(Environment.NewLine));
        }
    }
}