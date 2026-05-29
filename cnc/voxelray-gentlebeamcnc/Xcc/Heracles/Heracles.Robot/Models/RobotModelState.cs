namespace Heracles.Robot.Models
{
    public enum RobotModelState //TODO: move to another enums
    {
        Initial = 0,

        TreatmentHeadGrabInProgress,
        TreatmentHeadGrabFinished,

        TreatmentInProgress,
        TreatmentFinished,

        TreatmentHeadQcInProgress,
        TreatmentHeadReleaseInProgress,

        ImagingHeadGrabInProgress,
        ImagingHeadGrabFinished,

        ImagingInProgress,
        ImagingFinished,

        ImagingHeadReleaseInProgress,
    }
}
