using System.ComponentModel.DataAnnotations;

namespace Xcc.Core.Enums
{
    public enum GCBFaultDetails : int
    {
        [Display(Name = "")]
        Reserved = 0,
        [Display(Name = "ADC bus setup issue")]
        AcdBusSetupIssue = 1,
        [Display(Name = "ADC bus timeout issue")]
        AcdBusTimeoutIssue = 2,
        [Display(Name = "ADC bus NACK received")]
        AcdBusNACK = 3,
        [Display(Name = "Timer bus timeout")]
        TimerBusTimeout = 4,
        [Display(Name = "Timer #1 checksum error")]
        Timer1ChecksumError = 5,
        [Display(Name = "Timer #2 checksum error")]
        Timer2ChecksumError = 6,
        [Display(Name = "Timer #1 NACK")]
        Timer1NACK = 7,
        [Display(Name = "Timer #2 NACK")]
        Timer2NACK = 8,
        [Display(Name = "Head board comm timeout")]
        HeadBoardCommTimeout = 9,
        [Display(Name = "Head board comm checksum error")]
        HeadBoardCommChecksumError = 10,
        [Display(Name = "HVPS comm timeout")]
        HvpsCommTimeout = 11,
        [Display(Name = "HVPS comm checksum error")]
        HvpsCommChecksumError = 12,
        [Display(Name = "HVPS comm buffer overrun error")]
        HvpsCommBuferOverrunError = 13,
        [Display(Name = "Filament fault startup error")]
        FilamentFaultStartupError = 14,
        [Display(Name = "Filament fault target ramp timeout")]
        FilamentFaultTargetRampTimeout = 15,
        [Display(Name = "Filament fault setpoint overcurrent error")]
        FilamentFaultSetpointOvercurrentError = 16,
        [Display(Name = "Filament fault feedback overcurrent error")]
        FilamentFaultFeedbackOvercurrentError = 17,
        [Display(Name = "KV fault target ramp timeout")]
        KVFaultTargetRampTimeout = 18,
        [Display(Name = "KV fault target out of tolerance")]
        KVFaultTargetOutOfTolerance = 19,
        [Display(Name = "KV fault undesired HV error")]
        KVFaultUndesiredHvError = 20,
        [Display(Name = "mA fault target out of tolerance")]
        MAFaultTargetOutOfTolerance = 21,
        [Display(Name = "mA/grid fault unwanted emission")]
        MAGridFaultUnwantedEmission = 22,
        [Display(Name = "Peltier fault comm timeout")]
        PeltierFaultCommTimeout = 23,
        [Display(Name = "Peltier fault comm checksum error")]
        PeltierFaultCommChecksumError = 24,
        [Display(Name = "Ion repeller fault overcurrent")]
        IonRepellerFaultOvercurrent = 25,
        [Display(Name = "Ion repeller fault out of tolerance")]
        IonRepellerFaultOutOfTolerance = 26,
        [Display(Name = "X coil fault current out of tolerance")]
        XCoilFaultCurrentOutOfTolerance = 27,
        [Display(Name = "X coil fault voltage out of tolerance")]
        XCoilFaultVoltageOutOfTolerance = 28,
        [Display(Name = "Y coil fault current out of tolerance")]
        YCoilFaultCurrentOutOfTolerance = 29,
        [Display(Name = "Y coil fault voltage out of tolerance")]
        YCoilFaultVoltageOutOfTolerance = 30,
        [Display(Name = "Focus coil fault current out of tolerance")]
        FocusCoilFaultCurrentOutOfTolerance = 31,
        [Display(Name = "Focus coil fault voltage out of tolerance")]
        FocusCoilFaultVoltageOutOfTolerance = 32,
        [Display(Name = "Coolant fault overtemperature")]
        CoolantFaultOvertemperature = 33,
        [Display(Name = "Coolant fault low flow")]
        CoolantFaultLowFlow = 34,
        [Display(Name = "Coolant fault overpressure")]
        CoolantFaultOverpressure = 35
    }
}
