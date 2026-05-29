namespace Xcc.Core.Enums
{
    public enum RoboticArmAppState
    {
        IDLE,          // App is ready to get started 
        RUNNING,       // App is running 
        MOTIONPAUSED,  // App is paused at its next motion command call 
        REPOSITIONING, // Robot repositions to a path, to resume a paused app 
        ERROR,         // App had an error 
        UNDEPLOYED,    // App was undeployed 
        STARTING,      // App is starting 
        STOPPING,      // App is finishing 
        UNDEFINED
    }
}
