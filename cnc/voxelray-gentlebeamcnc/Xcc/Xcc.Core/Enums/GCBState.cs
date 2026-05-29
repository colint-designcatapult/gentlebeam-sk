using System;

namespace Xcc.Core.Enums
{
    public enum GcbStateNew : int
    {
        NoComm = -1,
        Startup = 0,
        Cold = 1,
        ColdFault = 2,
        DailyWarmup = 3,
        Warmup = 4,
        WarmupFault = 5,
        Primed = 6,
        Staging = 7,
        Staged = 8,
        HvpsCheck = 9,
        HVSetup = 10,
        Ready = 11,
        Launching = 12, 
        Emission = 13,
        Termination = 14,
        Discharge = 15,
        Fault = 16,
        SystemCrash = 17,
        LaunchingForImaging = 18,
        WaitForKey = 19, // Imaging - wait for HW key to trigger acquisition
        Imaging = 20,
        /// <summary>
        /// State between Cold and WarmUp. Can be reached from Conditioning
        /// </summary>
        StandBy = 21 
    }
}
