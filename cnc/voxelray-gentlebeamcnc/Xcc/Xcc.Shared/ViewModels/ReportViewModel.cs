using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using Xcc.Application.Models;

namespace Xcc.Shared.ViewModels
{
    class ReportViewModel : BindableBase, IDialogAware
    {
        private Report? _report;
        public Report? Report
        {
            get => _report;
            set => SetProperty(ref _report, value);
        }

        private DelegateCommand? _closeDialogCommand;
        public DelegateCommand CloseDialogCommand => _closeDialogCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            },
            CanCloseDialog);

        private DelegateCommand? _resumeCommand;
        public DelegateCommand ResumeCommand => _resumeCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            },
            CanCloseDialog);

        private DelegateCommand? _revertCommand;
        public DelegateCommand RevertCommand => _revertCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Abort));
            },
            CanCloseDialog);


        private DelegateCommand? _confirmCommand;
        public DelegateCommand ConfirmCommand => _confirmCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            },
            CanCloseDialog);


        private DelegateCommand? _postponeCommand;
        public DelegateCommand PostponeCommand => _postponeCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.No));
            },
            CanCloseDialog);

        private DelegateCommand? _stopCommand;
        public DelegateCommand StopCommand => _stopCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Abort));
            },
            CanCloseDialog);


        #region IDialogAware
        public event Action<IDialogResult>? RequestClose;

        public string Title { get; set; } = string.Empty;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("Report", out Report report))
                Report = report;
        }
        #endregion IDialogAware
    }
}
