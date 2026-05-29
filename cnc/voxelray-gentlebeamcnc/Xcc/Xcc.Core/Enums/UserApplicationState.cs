namespace Xcc.Core.Enums
{
    // States a robot application can be in.
    public enum UserApplicationState
    {
        IDLE, /** Idle - App is ready to get started. */
        RUNNING, /** Running - App is running. */
        MOTIONPAUSED, /** Motion paused - App is paused at its next motion command call. */
        REPOSITIONING, /** Repositioning - Robot repositions to a path, to resume a paused app. */
        ERROR, /** Error - App had an error. */
        UNDEPLOYED, /** Undeployed - App was undeployed. */
        STARTING, /** Starting - App is starting. */
        STOPPING, /** Stopping - App is finishing. */
        UNDEFINED
    }
}
