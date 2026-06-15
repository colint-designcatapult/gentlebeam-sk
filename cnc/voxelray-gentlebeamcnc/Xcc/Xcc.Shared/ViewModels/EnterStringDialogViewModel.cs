using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using Xcc.Application.UI.Mvvm;

namespace Xcc.Shared.ViewModels
{
    public class EnterStringDialogViewModel : DialogViewModelBase
    {
        public EnterStringDialogViewModel() { }

        private string _value = String.Empty;
        public string Value 
        {
            get => _value;
            set 
            { 
                SetProperty(ref _value, value);
                AcceptCommand?.RaiseCanExecuteChanged();
            }
        }

        private string _errorMessage = String.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
            }
        }

        private string _message = String.Empty;
        public string Message
        {
            get => _message;
            set
            {
                SetProperty(ref _message, value);
            }
        }

        Func<string, Tuple<bool, string>>? _validationCallback;

        private DelegateCommand? _acceptCommand;
        public DelegateCommand AcceptCommand => _acceptCommand ??= new DelegateCommand(
            () =>
            {
                DialogParameters parameters = new DialogParameters
                {
                    { "Value", Value.Trim() }
                };

                CloseDialog(parameters);
            },
            () => !string.IsNullOrWhiteSpace(Value) && ValidateValue());

        private DelegateCommand? _cancelCommand;
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(CancelDialog);


        protected override void SetDialogParameters(IDialogParameters parameters)
        {
            base.SetDialogParameters(parameters);
            
            if (parameters.TryGetValue("ValidationCallback", out Func<string, Tuple<bool, string>> validationCallback))
                _validationCallback = validationCallback;

            if (parameters.TryGetValue("Message", out string message))
                Message = message;

        }

        private bool ValidateValue()
        {
            if (_validationCallback is null)
                return true;

            var result = _validationCallback(Value.Trim());
            ErrorMessage = result.Item2;
            return result.Item1;
        }
    }
}
