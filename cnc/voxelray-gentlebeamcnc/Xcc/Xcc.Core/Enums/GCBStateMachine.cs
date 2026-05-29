namespace Xcc.Core.Enums
{
    public enum GCBStateMachine
    {
        COLD = 0,
        COLD_FAULT,
        STANDBY,
        WARMUP,
        WARMUP_FAULT,
        PRIMED,
        HVPS_CHECK,
        SETUP,
        READY,
        BEAM_ON,
        CURRENT_CONTROL,
        TERMINATION,
        DISCHARGE,
        FAULT,
        WARMUP_HOLD
    }
}
