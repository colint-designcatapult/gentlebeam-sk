using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace Xcc.Application.UI.Mvvm
{
    public abstract class DialogViewModelBase : BindableBase, IDialogAware
    {
        private string _title = string.Empty;
        public virtual string Title
        {
            get => _title; 
            set => SetProperty(ref _title, value);
        }

        public event Action<IDialogResult>? RequestClose;

        protected virtual void CloseDialog(DialogParameters parameters)
        {
            OnRequestClose(new DialogResult(ButtonResult.OK, parameters));
        }

        protected virtual void CloseDialog()
        {
            OnRequestClose(new DialogResult(ButtonResult.OK));
        }

        protected virtual void CancelDialog()
        {
             OnRequestClose(new DialogResult(ButtonResult.Cancel));
        }

        public virtual void OnRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed() {}

        protected virtual void SetDialogParameters(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("Title", out string title))
                Title = title;
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            SetDialogParameters(parameters);
        }

        private DelegateCommand? _closeDialogCommand;
        public DelegateCommand CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand(
            () =>
            {
                OnRequestClose(new DialogResult(ButtonResult.Cancel));
            });
    }
}
