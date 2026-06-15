using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Xcc.Core.Domain.GryphonBoard
{
    public enum GcbInterlockFlags : int
    {
        [Display(Name ="Door")]
        Door = 1 << 0,
        [Display(Name = "Drive system")]
        DriveSystem = 1 << 1,
        [Display(Name = "Base e-stop")]
        BaseEStop = 1 << 2,
        [Display(Name = "Remote e-stop")]
        RemoteEStop = 1 <<3,
        [Display(Name = "KUKA #1")]
        Kuka1 = 1 << 4,
        [Display(Name = "KUKA #2")]
        Kuka2 = 1<< 5,
        [Display(Name = "Water level")]
        WaterLevel = 1 << 6,
        [Display(Name = "Ion pump")]
        IonPump = 1 << 7,
        [Display(Name = "Timer #1")]
        Timer1 = 1 << 8,
        [Display(Name = "Timer #2")]
        Timer2 = 1 << 9,
        [Display(Name = "HVPS status")]
        HvpsStatus = 1 << 10,
        [Display(Name = "Peltier cooler")]
        PeltierCooler = 1 << 11,
        [Display(Name = "Head interface board")]
        HeadInterfaceBoard = 1 << 12,
        [Display(Name = "Watchdog")]
        Watchdog = 1 << 13,
        [Display(Name = "MCU (output pin)")]
        Mcu = 1 << 14,
        [Display(Name = "")]
        Reserved1 = 1 << 15,
        [Display(Name = "Master fault")]
        Master = 1 << 16,
        [Display(Name = "")]
        Reserved2 = 1 << 17,
        [Display(Name = "Door")]
        RemoteKey = 1 << 18,
        [Display(Name = "Door")]
        BaseKey = 1 << 19,
        [Display(Name = "Collimator")] // 'virtual' separate value to display BaseKey as Collimator in Heracles
        HeraclesCollimator = 1 << 20,
    }

    public class GcbInterlocks
    {
        public bool DoorOpened { get; set; }
        public bool DriveSystemUnlocked { get; set; }
        public bool BaseEStopEngaged { get; set; }
        public bool RemoteEStopEngaged { get; set; }
        public bool Kuka1 { get; set; }
        public bool Kuka2 { get; set; }
        public bool WaterLevel { get; set; }
        public bool IonPumpHV { get; set; }
        public bool Timer1Expired { get; set; }
        public bool Timer2Expired { get; set; }
        public bool HVPS { get; set; }
        public bool CoolerAlarm { get; set; }
        public bool WaterTemperature { get; set; }
        public bool Watchdog { get; set; }
        public bool MCU { get; set; }
        public bool UnusedInterlock { get; set; }
        public bool BaseKey { get; set; }
        public bool RemoteKey { get; set; }
        public bool HeadInterfaceBoard { get; set; }
        public bool HeraclesCollimator { get; set; } // Heracles only, in fact the BaseKey value is used

        /*
            Interlock Bit (LSb = 0)	Details
            0	Door
            1	Drive system
            2	Base e-stop
            3	Remote e-stop
            4	KUKA #1
            5	KUKA #2
            6	Water level
            7	Ion pump
            8	Timer #1
            9	Timer #2
            10	HVPS status
            11	Peltier cooler
            12	Head interface board
            13	Watchdog
            14	MCU (output pin)
            15	Spare (reserved)
            16	Master 
            17	N/A
            18	Remote key
            19	Base key (Collimator in Heracles, mirrored to bit #20)
         */
        public static GcbInterlocks Create(uint interlockStatus)
        {
            return new GcbInterlocks()
            {
                DoorOpened = (interlockStatus & (int)GcbInterlockFlags.Door) != 0,
                DriveSystemUnlocked = (interlockStatus & (int)GcbInterlockFlags.DriveSystem) != 0,
                BaseEStopEngaged = (interlockStatus & (int)GcbInterlockFlags.BaseEStop) != 0,
                RemoteEStopEngaged = (interlockStatus & (int)GcbInterlockFlags.RemoteEStop) != 0,
                Kuka1 = (interlockStatus & (int)GcbInterlockFlags.Kuka1) != 0,
                Kuka2 = (interlockStatus & (int)GcbInterlockFlags.Kuka2) != 0,
                WaterLevel = (interlockStatus & (int)GcbInterlockFlags.WaterLevel) != 0,
                IonPumpHV = (interlockStatus & (int)GcbInterlockFlags.IonPump) != 0,
                Timer1Expired = (interlockStatus & (int)GcbInterlockFlags.Timer1) != 0,
                Timer2Expired = (interlockStatus & (int)GcbInterlockFlags.Timer2) != 0,
                HVPS = (interlockStatus & (int)GcbInterlockFlags.HvpsStatus) != 0,
                CoolerAlarm = (interlockStatus & (int)GcbInterlockFlags.PeltierCooler) != 0,
                HeadInterfaceBoard = (interlockStatus & (int)GcbInterlockFlags.HeadInterfaceBoard) != 0,
                Watchdog = (interlockStatus & (int)GcbInterlockFlags.Watchdog) != 0,
                //MCU = (interlockStatus & (1 << 14)) != 0,
                //UnusedInterlock = (interlockStatus & (1 << 15)) != 0,
                //Master = (interlockStatus & (1 << 16)) != 0,
                //NA = (interlockStatus & (1 << 17)) != 0,
                RemoteKey = (interlockStatus & (int)GcbInterlockFlags.RemoteKey) != 0,
                BaseKey = (interlockStatus & (int)GcbInterlockFlags.BaseKey) != 0,
                HeraclesCollimator = (interlockStatus & (int)GcbInterlockFlags.BaseKey) != 0,
            };
        }
    }

    public class GcbInterlocksNew
    {
        private readonly int _interlockFlags;

        public GcbInterlocksNew(int interlockFlags)
        {
            _interlockFlags = interlockFlags;
        }

        public bool CheckInterlock(GcbInterlockFlags interlockFlag)
        {
            return (interlockFlag != GcbInterlockFlags.HeraclesCollimator) 
                ? (_interlockFlags & (int)interlockFlag) != 0
                : CheckInterlock(GcbInterlockFlags.BaseKey);
        }

        public IEnumerable<GcbInterlockFlags> GetOpenInterlocks()
        {
            return Enum.GetValues<GcbInterlockFlags>().Where(x => !CheckInterlock(x));
        }
    }
}
