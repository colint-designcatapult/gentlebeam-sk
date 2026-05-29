using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace Xcc.Shared.ViewModels
{
    class DatePickerDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand? _closeCommand;
        public DelegateCommand CloseCommand => _closeCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            });

        private DelegateCommand? _acceptCommand;
        public DelegateCommand ApplyCommand => _acceptCommand ??= new(
            () =>
            {
                DialogParameters parameters = new()
                {
                    { "FromDate", FromDate },
                    { "ToDate", ToDate }
                };
           
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, parameters));
            });

        private DateTime _fromDate = new(DateTime.Today.Year, 1, 1);
        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        private DateTime _toDate = DateTime.Today.AddDays(1);
        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        #region IDialogAware
        public event Action<IDialogResult>? RequestClose;

        public string Title { get; set; } = "Date Picker";

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("FromDate", out DateTime fromDate))
                FromDate = fromDate;

            if (parameters.TryGetValue("ToDate", out DateTime toDate))
                ToDate = toDate;
        }
        #endregion IDialogAware
    }
}
