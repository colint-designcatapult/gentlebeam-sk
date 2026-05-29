using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbInterlocksNewTests
    {
        [Test]
        public void Ctor_CheckInterlock()
        {
            var sut = new GcbInterlocksNew((int)GcbInterlockFlags.Door | (int)GcbInterlockFlags.DriveSystem);

            Assert.That(sut.CheckInterlock(GcbInterlockFlags.Door), Is.True);
            Assert.That(sut.CheckInterlock(GcbInterlockFlags.DriveSystem), Is.True);
            Assert.That(sut.CheckInterlock(GcbInterlockFlags.BaseEStop), Is.False);
        }

        [Test]
        public void GetOpenInterlocks_ShouldReturnUnsetFlags()
        {
            var interlocks =
                (int)GcbInterlockFlags.Door |
                (int)GcbInterlockFlags.DriveSystem |
                (int)GcbInterlockFlags.BaseEStop |
                (int)GcbInterlockFlags.RemoteEStop |
                (int)GcbInterlockFlags.Kuka1 |
                (int)GcbInterlockFlags.Kuka2 |
                (int)GcbInterlockFlags.WaterLevel |
                (int)GcbInterlockFlags.IonPump |
                (int)GcbInterlockFlags.HvpsStatus |
                (int)GcbInterlockFlags.PeltierCooler |
                (int)GcbInterlockFlags.HeadInterfaceBoard |
                (int)GcbInterlockFlags.Watchdog |
                (int)GcbInterlockFlags.Mcu |
                (int)GcbInterlockFlags.Reserved1 |
                (int)GcbInterlockFlags.Reserved2 |
                (int)GcbInterlockFlags.RemoteKey |
                (int)GcbInterlockFlags.BaseKey;
            var expectedOpened = new List<GcbInterlockFlags>
            {
                GcbInterlockFlags.Timer1,
                GcbInterlockFlags.Timer2,
                GcbInterlockFlags.Master
            };

            var sut = new GcbInterlocksNew(interlocks);
            var res = sut.GetOpenInterlocks().ToList();

            Assert.That(res, Is.EquivalentTo(expectedOpened));
        }
    }
}