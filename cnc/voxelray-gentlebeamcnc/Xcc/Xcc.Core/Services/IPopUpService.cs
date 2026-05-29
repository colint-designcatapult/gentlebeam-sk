using System;
using Xcc.Core.Enums;

namespace Xcc.Core.Services
{
    public enum DialogBoxIconType
    {
        None,
        Choice,
        Error,
        Warning,
    }

    public enum DialogBoxResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
    }


    public interface IPopUpService
    {        
        public void LogAndShowMessage(string title, string message, ReportType reportType, LogRecordSeverity severity, LogRecordType logRecordType);
        public void LogAndShowError(string title, string message, Exception? exception = null);
        void ShowMessage(string title, string message, ReportType reportType);

        DialogBoxResult YesNoCancelDialog(
            string title, string message, 
            string yesButtonText = "Yes", string noButtonText = "No", 
            string cancelButtonText = "Cancel", 
            DialogBoxIconType iconType = DialogBoxIconType.Choice);

        DialogBoxResult YesNoDialog(
            string title, string message,
            string yesButtonText = "Yes", string noButtonText = "No",
            DialogBoxIconType iconType = DialogBoxIconType.Choice);

        DialogBoxResult YesCancelDialog(
            string title, string message,
            string yesButtonText = "Yes",
            string cancelButtonText = "Cancel",
            DialogBoxIconType iconType = DialogBoxIconType.Choice);
        void ShowDialog(string dialogName);
    }
}
