namespace Xcc.Core.Enums
{
    public enum GCBDirectiveCommandNew : int
    {
        Initialize = 1,
        StagePlan = 2,
        Stop = 3,
        ClearFaults = 4,
        ClearPlan = 5,
        ResetTimers = 6
    }

    public enum GCBDirectiveCommand
    {
        Stop = 0,
        FullWarmup = 1,
        Start = 2,
        Standby = 3,
        Shutdown = 4,
        ClearFaults = 5,
        ResetAuthentication = 6,
        ClearPlanIDs = 7, 
        FastWarmup = 8,
    }


}
