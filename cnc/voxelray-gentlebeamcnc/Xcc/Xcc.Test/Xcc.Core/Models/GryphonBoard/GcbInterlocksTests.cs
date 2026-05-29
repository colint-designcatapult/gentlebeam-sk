using Xcc.Core.Domain.GryphonBoard;

namespace Xcc.Test.Xcc.Core.Models.GryphonBoard
{
    public class GcbInterlocksTests
    {
        [Test]
        public void Defaults()
        {
            var sut = new GcbInterlocks();

            Assert.That(sut.DoorOpened, Is.False);
            Assert.That(sut.DriveSystemUnlocked, Is.False);
            Assert.That(sut.BaseEStopEngaged, Is.False);
            Assert.That(sut.RemoteEStopEngaged, Is.False);
            Assert.That(sut.Kuka1, Is.False);
            Assert.That(sut.Kuka2, Is.False);
            Assert.That(sut.WaterLevel, Is.False);
            Assert.That(sut.IonPumpHV, Is.False);
            Assert.That(sut.Timer1Expired, Is.False);
            Assert.That(sut.Timer2Expired, Is.False);
            Assert.That(sut.HVPS, Is.False);
            Assert.That(sut.CoolerAlarm, Is.False);
            Assert.That(sut.WaterTemperature, Is.False);
            Assert.That(sut.Watchdog, Is.False);
            Assert.That(sut.MCU, Is.False);
            Assert.That(sut.UnusedInterlock, Is.False);
            Assert.That(sut.BaseKey, Is.False);
            Assert.That(sut.RemoteKey, Is.False);
            Assert.That(sut.HeadInterfaceBoard, Is.False);
        }
        
        [Test]
        public void GetterSetter()
        {            
            bool doorOpened = true;
            bool driveSystemUnlocked = true;
            bool baseEStopEngaged = true;
            bool remoteEStopEngaged = true;
            bool kuka1 = true;
            bool kuka2 = true;
            bool waterLevel = true;
            bool ionPumpHV = true;
            bool timer1Expired = true;
            bool timer2Expired = true;
            bool hpvs = true;
            bool coolerAlarm = true;
            bool waterTemperature = true;
            bool watchdog = true;
            bool mcu = true;
            bool unusedInterlock = true;
            bool baseKey = true;
            bool remoteKey = true;
            bool headInterfaceBoard = true;
            
            var sut = new GcbInterlocks
            {
                DoorOpened = doorOpened,
                DriveSystemUnlocked = driveSystemUnlocked,
                BaseEStopEngaged = baseEStopEngaged,
                RemoteEStopEngaged = remoteEStopEngaged,
                Kuka1 = kuka1,
                Kuka2 = kuka2,
                WaterLevel = waterLevel,
                IonPumpHV = ionPumpHV,
                Timer1Expired = timer1Expired,
                Timer2Expired = timer2Expired,
                HVPS = hpvs,
                CoolerAlarm = coolerAlarm,
                WaterTemperature = waterTemperature,
                Watchdog = watchdog,
                MCU = mcu,
                UnusedInterlock = unusedInterlock,
                BaseKey = baseKey,
                RemoteKey = remoteKey,
                HeadInterfaceBoard = headInterfaceBoard,
            };
        
            Assert.That(sut.DoorOpened, Is.EqualTo(doorOpened));
            Assert.That(sut.DriveSystemUnlocked, Is.EqualTo(driveSystemUnlocked));
            Assert.That(sut.BaseEStopEngaged, Is.EqualTo(baseEStopEngaged));
            Assert.That(sut.RemoteEStopEngaged, Is.EqualTo(remoteEStopEngaged));
            Assert.That(sut.Kuka1, Is.EqualTo(kuka1));
            Assert.That(sut.Kuka2, Is.EqualTo(kuka2));
            Assert.That(sut.WaterLevel, Is.EqualTo(waterLevel));
            Assert.That(sut.IonPumpHV, Is.EqualTo(ionPumpHV));
            Assert.That(sut.Timer1Expired, Is.EqualTo(timer1Expired));
            Assert.That(sut.Timer2Expired, Is.EqualTo(timer2Expired));
            Assert.That(sut.HVPS, Is.EqualTo(hpvs));
            Assert.That(sut.CoolerAlarm, Is.EqualTo(coolerAlarm));
            Assert.That(sut.WaterTemperature, Is.EqualTo(waterTemperature));
            Assert.That(sut.Watchdog, Is.EqualTo(watchdog));
            Assert.That(sut.MCU, Is.EqualTo(mcu));
            Assert.That(sut.UnusedInterlock, Is.EqualTo(unusedInterlock));
            Assert.That(sut.BaseKey, Is.EqualTo(baseKey));
            Assert.That(sut.RemoteKey, Is.EqualTo(remoteKey));
            Assert.That(sut.HeadInterfaceBoard, Is.EqualTo(headInterfaceBoard));
        }
        
        [Test]
        public void Create()
        {
            uint bits = (uint)(
                (int)GcbInterlockFlags.Door |
                (int)GcbInterlockFlags.RemoteEStop |
                (int)GcbInterlockFlags.WaterLevel |
                (int)GcbInterlockFlags.Timer1 |
                (int)GcbInterlockFlags.Timer2
            );
            var sut = GcbInterlocks.Create(bits);
        
            Assert.That(sut.DoorOpened, Is.True);
            Assert.That(sut.DriveSystemUnlocked, Is.False);
            Assert.That(sut.BaseEStopEngaged, Is.False);
            Assert.That(sut.RemoteEStopEngaged, Is.True);
            Assert.That(sut.Kuka1, Is.False);
            Assert.That(sut.Kuka2, Is.False);
            Assert.That(sut.WaterLevel, Is.True);
            Assert.That(sut.IonPumpHV, Is.False);
            Assert.That(sut.Timer1Expired, Is.True);
            Assert.That(sut.Timer2Expired, Is.True);
            Assert.That(sut.HVPS, Is.False);
            Assert.That(sut.CoolerAlarm, Is.False);
            Assert.That(sut.WaterTemperature, Is.False);
            Assert.That(sut.Watchdog, Is.False);
            Assert.That(sut.MCU, Is.False);
            Assert.That(sut.UnusedInterlock, Is.False);
            Assert.That(sut.BaseKey, Is.False);
            Assert.That(sut.RemoteKey, Is.False);
            Assert.That(sut.HeadInterfaceBoard, Is.False);
        }
    }
}