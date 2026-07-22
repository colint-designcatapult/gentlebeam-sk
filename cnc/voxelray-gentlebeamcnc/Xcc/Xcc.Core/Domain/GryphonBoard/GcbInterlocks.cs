using System.ComponentModel.DataAnnotations;

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
        [Display(Name = "Remote key")]
        RemoteKey = 1 << 18,
        [Display(Name = "Collimator")]
        CollimatorOn = 1 << 19,
    }

}
