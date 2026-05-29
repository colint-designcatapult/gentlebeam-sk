namespace Xcc.Core.Enums
{
    public enum ReportType
    {
        Progress,
        Info,
        Error,
        Confirmation, //OK CANCEL
        ConfirmationResumeRevert, // RESUME REVERT
        ConfirmationResumeRevertCancel, // RESUME REVERT CANCEL
        ConfirmationPostpone, // OK POSTPONE
        ConfirmationStop // OK STOP
    }
}
