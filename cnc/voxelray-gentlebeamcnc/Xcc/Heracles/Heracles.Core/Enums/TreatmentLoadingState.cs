namespace Heracles.Core.Enums
{
    public enum TreatmentLoadingState : int
    {
        /// <summary>
        /// Initial / final state
        /// </summary>
        Unloaded,
        /// <summary>
        /// sent to External, wait for loading
        /// </summary>
        PendingLoad, 
        /// <summary>
        /// Loaded to the main control board
        /// </summary>
        Loaded,
        /// <summary>
        /// sent to External for partial treatment, wait for loading
        /// </summary>
        PartialPendingLoad
    }
}
