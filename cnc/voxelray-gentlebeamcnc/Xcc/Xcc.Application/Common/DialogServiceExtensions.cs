using Prism.Services.Dialogs;
using System;
using System.Threading.Tasks;
using Xcc.Application.ViewModels.Approval;
using Xcc.Core.Enums;

namespace Xcc.Application.Common
{
    public static class DialogServiceExtensions
    {
        /// <summary>
        /// Asynchronously shows a ReportView dialog with ReportType.Error. Doesn't block execution of the calling method.
        /// </summary>
        public static void ReportErrorAsync(this IDialogService dialogService, string title, string message, Action<IDialogResult>? closedCallback = null)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var report = new Xcc.Application.Models.Report(
                    Xcc.Core.Enums.ReportType.Error,
                    title,
                    message);

                DialogParameters parameters = new()
                {
                    { "Report", report }
                };
                dialogService.ShowDialog("ReportView", parameters, closedCallback);
            });
        }

        /// <summary>
        /// Asynchronously shows a ReportView dialog with the specified report type. Doesn't block execution of the calling method.
        /// </summary>
        public static void ReportAsync(this IDialogService dialogService, string title, string message, ReportType reportType, Action<IDialogResult>? closedCallback = null)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(() =>
            {
                var report = new Xcc.Application.Models.Report(
                    reportType,
                    title,
                    message);

                DialogParameters parameters = new()
                {
                    { "Report", report }
                };
                dialogService.ShowDialog("ReportView", parameters, closedCallback);
            });
        }

        /// <summary>
        /// Synchronously shows a ReportView dialog with ReportType.Error. Block execution of the calling method.
        /// </summary>
        public static void ReportError(this IDialogService dialogService, string title, string message, Action<IDialogResult>? closedCallback = null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var report = new Xcc.Application.Models.Report(
                    Xcc.Core.Enums.ReportType.Error,
                    title,
                    message);

                DialogParameters parameters = new()
                {
                    { "Report", report }
                };
                dialogService.ShowDialog("ReportView", parameters, closedCallback);
            });
        }

        /// <summary>
        /// Synchronously shows a ReportView with the specified report type. Block execution of the calling method.
        /// </summary>
        public static void Report(this IDialogService dialogService, string title, string message, ReportType reportType, Action<IDialogResult>? closedCallback = null)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var report = new Xcc.Application.Models.Report(
                    reportType,
                    title,
                    message);

                DialogParameters parameters = new()
                {
                    { "Report", report }
                };
                dialogService.ShowDialog("ReportView", parameters, closedCallback);
            });
        }

        /// <summary>
        /// Synchronously shows a ReportView with the specified report type. Blocks execution of the calling method.
        /// Returns true if user has confirmed the choice.
        /// </summary>
        public static bool Confirmation(this IDialogService dialogService, string title, string message)
        {
            bool confirmed = false;
            dialogService.Report(title, message, ReportType.Confirmation, (result) =>
            {
                confirmed = (result.Result == ButtonResult.OK);
            });
            return confirmed;
        }

        public static async Task<bool> ConfirmationAsync(this IDialogService dialogService, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            dialogService.Report(title, message, ReportType.Confirmation, (result) =>
            {
                bool confirmed = (result.Result == ButtonResult.OK);
                tcs.SetResult(confirmed);
            });

            return await tcs.Task;
        }

        public static void ApprovalDialog(this IDialogService dialogService, IApprovalAction action)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                DialogParameters parameters = new()
                {
                    { ApprovalViewModel.ApprovalActionParameterName,  action }
                };
                dialogService.ShowDialog("ApprovalView", parameters, result => { });
            });
        }
    }
}
