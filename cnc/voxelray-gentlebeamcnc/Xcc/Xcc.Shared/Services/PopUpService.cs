using System;
using Prism.Services.Dialogs;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Core.Services;
using Xcc.Shared.ViewModels;

namespace Xcc.Shared.Services
{
    public class PopUpService(IDialogService dialogService, ILogWriter logWriter): IPopUpService
    {
        public IDialogService DialogService { get; } = dialogService;
        public ILogWriter LogWriter { get; } = logWriter;

        public void LogAndShowError(string title, string message, Exception? exception = null)
        {
            var report = new Xcc.Application.Models.Report(
                    ReportType.Error,
                    title,
                    message);

            DialogParameters parameters = new() { { "Report", report } };

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                DialogService.ShowDialog("ReportView", parameters, result => { }));

            string? exMessage = exception?.Message;
            if (!string.IsNullOrEmpty(exMessage))
            {
                if (message.EndsWith(exMessage))
                    exMessage = ". ";
                else
                    exMessage =  ". " + exMessage + ". ";
            }

            _ = LogWriter.LogAsync($"{title}: {message}{exMessage}{exception?.InnerException?.Message}", LogRecordSeverity.Error, LogRecordType.Error);
        }
        public void LogAndShowMessage(string title, string message, ReportType reportType, LogRecordSeverity severity, LogRecordType logRecordType)
        {
            var report = new Xcc.Application.Models.Report(
                    reportType,
                    title,
                    message);

            DialogParameters parameters = new() { { "Report", report } };

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                DialogService.ShowDialog("ReportView", parameters, result => { }));

            _ = LogWriter.LogAsync($"{title}: {message}", severity, logRecordType);
        }

        public void ShowMessage(string title, string message, ReportType reportType)
        {
            var report = new Xcc.Application.Models.Report(
                    reportType,
                    title,
                    message);

            DialogParameters parameters = new() { { "Report", report } };

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                DialogService.ShowDialog("ReportView", parameters, result => { }));
        }

        public void ShowDialog(string dialogName)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                DialogService.ShowDialog(dialogName));
        }

        public DialogBoxResult DialogBox(
            string title, string message,
            DialogBoxButton? leftButton = null,
            DialogBoxButton? centralButton = null,
            DialogBoxButton? rightButton = null,
            DialogBoxIconType iconType = DialogBoxIconType.None)
        {
            string? iconName = iconType switch
            {
                DialogBoxIconType.None => null,
                DialogBoxIconType.Choice => "DialogQuestionIcon",
                DialogBoxIconType.Error => "DialogErrorIcon",
                DialogBoxIconType.Warning => "ExclamationIcon",
                _ => throw new ArgumentNullException(nameof(iconType)),
            };

            DialogParameters parameters = new() {
                { "DialogTitle", title },
                { "DialogMessage", message },
                { "DialogIcon",  iconName},
                { "LeftButton", leftButton},
                { "CentralButton", centralButton },
                { "RightButton", rightButton },
            };

            ButtonResult buttonPressed = ButtonResult.None;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                DialogService.ShowDialog("DialogBoxView", parameters, result => buttonPressed = result.Result));
            return (DialogBoxResult)buttonPressed;
        }

        public DialogBoxResult YesNoCancelDialog(
            string title, string message,
            string yesButtonText = "Yes",
            string noButtonText = "No",
            string cancelButtonText = "Cancel",
            DialogBoxIconType iconType = DialogBoxIconType.Choice
            )
        {
            return DialogBox(
                title, message,
                new DialogBoxButton(yesButtonText, ButtonResult.Yes, isDefault: true),
                new DialogBoxButton(noButtonText, ButtonResult.No),
                new DialogBoxButton(cancelButtonText, ButtonResult.Cancel, isCancel: true),
                iconType);
        }

        public DialogBoxResult YesNoDialog(
            string title, string message,
            string yesButtonText = "Yes",
            string noButtonText = "No",
            DialogBoxIconType iconType = DialogBoxIconType.Choice
            )
        {
            return DialogBox(
                title, message,
                new DialogBoxButton(yesButtonText, ButtonResult.Yes, isDefault: true),
                new DialogBoxButton(noButtonText, ButtonResult.No),
                iconType: iconType);
        }


        public DialogBoxResult YesCancelDialog(
            string title, string message,
            string yesButtonText = "Yes",
            string cancelButtonText = "Cancel",
            DialogBoxIconType iconType = DialogBoxIconType.Choice)
        {
            return DialogBox(
                title, message,
                new DialogBoxButton(yesButtonText, ButtonResult.Yes, isDefault: true),
                new DialogBoxButton(cancelButtonText, ButtonResult.Cancel, isCancel: true),
                iconType: iconType);
        }
    }
}
