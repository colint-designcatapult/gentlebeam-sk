
using System.ComponentModel.DataAnnotations;

namespace Xcc.Core.Enums
{
    public enum GCBFaultBit : int
    {
        [Display(Name = "No fault")]
        Reserved = 0,
        [Display(Name = "Interlock Fault")]
        InterlockFault = 1,
        [Display(Name = "HVPS Reported Fault")]
        HvpsReportedFault = 2,
        [Display(Name = "kV Fault")]
        VoltageFault = 3,
        [Display(Name = "mA Fault")]
        CurrentFault = 4,
        [Display(Name = "Filament Fault")]
        FilamentFault = 5,
        [Display(Name = "Grid Fault")]
        GridFault = 6,
        [Display(Name = "Coil Fault")]
        CoilFault = 7,
        [Display(Name = "Ion Pump Fault")]
        IonPumpFault = 8,
        [Display(Name = "Ion Repeller Fault")]
        IonRepellerFault = 9,
        [Display(Name = "Peltier Fault")]
        PeltierFault = 10,
        [Display(Name = "Heatsink Fault")]
        HeatsinkFault = 11,
        [Display(Name = "Coolant Fault")]
        CoolantFault = 12,
        [Display(Name = "Internal Supply Voltage Fault")]
        InternalSupplyVoltageFault = 13,
        [Display(Name = "PC Comm. Fault")]
        PcCommFault = 14,
        [Display(Name = "HVPS Comm. Fault")]
        HvpsCommFault = 15,
        [Display(Name = "Timer Comm. Fault")]
        TimerCommFault = 16,
        [Display(Name = "Head Board Comm. Fault")]
        HeadBoardCommFault = 17,
        [Display(Name = "LED Board Comm. Fault")]
        LedBoardCommFault = 18,
        [Display(Name = "Peltier Controller Comm. Fault")]
        PeltierControllerCommFault = 19,
        [Display(Name = "QC Well Comm. Fault")]
        QcWellCommFault = 20,
        [Display(Name = "ADC Bus Comm. Fault")]
        AdcBusCommFault = 21,

        [Display(Name = "Memory Fault")]
        MemoryFault,

        [Display(Name = "Invalid configuration Fault")]
        InvalidConfigFault,

        [Display(Name = "Fault24")]
        Reserved24,

        [Display(Name = "Fault25")]
        Reserved25,

        [Display(Name = "Fault26")]
        Reserved26,

        [Display(Name = "Fault27")]
        Reserved27,

        [Display(Name = "Fault28")]
        Reserved28,

        [Display(Name = "Fault29")]
        Reserved29,

        [Display(Name = "Fault30")]
        Reserved30,

        [Display(Name = "Fault31")]
        Reserved31,

        [Display(Name = "Fault32")]
        Reserved32
    }
}
