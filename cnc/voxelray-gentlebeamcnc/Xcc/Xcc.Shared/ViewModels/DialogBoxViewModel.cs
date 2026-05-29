using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows;

namespace Xcc.Shared.ViewModels
{
    class DialogBoxViewModel : BindableBase, IDialogAware
    {
        #region Properties
        private string _dialogTitle = "Title";
        public string DialogTitle { 
            get => _dialogTitle;
            private set => SetProperty(ref _dialogTitle, value);
        }

        private string _dialogMessage = "Message";
        public string DialogMessage
        {
            get => _dialogMessage;
            private set => SetProperty(ref _dialogMessage, value);
        }

        private object? _dialogIcon;
        public object? DialogIcon
        {
            get => _dialogIcon;
            private set => SetProperty(ref _dialogIcon, value);
        }

        private DialogBoxButton? _leftButton;
        public DialogBoxButton? LeftButton
        {
            get => _leftButton;
            set => SetProperty(ref _leftButton, value);
        }

        private DialogBoxButton? _centralButton;
        public DialogBoxButton? CentralButton
        {
            get => _centralButton;
            set => SetProperty(ref _centralButton, value);
        }

        private DialogBoxButton? _rightButton;
        public DialogBoxButton? RightButton
        {
            get => _rightButton;
            set => SetProperty(ref _rightButton, value);
        }

        #endregion Properties

        #region Commands
        private DelegateCommand? _closeDialogCommand;
        public DelegateCommand CloseDialogCommand => _closeDialogCommand ??= new(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            },
            CanCloseDialog);

        private DelegateCommand? _leftButtonCommand;
        public DelegateCommand LeftButtonCommand => _leftButtonCommand ??= new DelegateCommand(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(LeftButton!.Result));
            }, 
            () => LeftButton != null).ObservesProperty(() => LeftButton);

        private DelegateCommand? _centralButtonCommand;
        public DelegateCommand CentralButtonCommand => _centralButtonCommand ??= new DelegateCommand(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(CentralButton!.Result));
            },
            () => CentralButton != null).ObservesProperty(() => CentralButton);

        private DelegateCommand? _rightButtonCommand;
        public DelegateCommand RightButtonCommand => _rightButtonCommand ??= new DelegateCommand(
            () =>
            {
                RequestClose?.Invoke(new DialogResult(RightButton!.Result));
            },
            () => RightButton != null).ObservesProperty(() => RightButton);
        #endregion Commands

        #region IDialogAware
        public event Action<IDialogResult>? RequestClose;

        public string Title { get; set; } = string.Empty;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.TryGetValue("DialogTitle", out string title))
                DialogTitle = title;
            if (parameters.TryGetValue("DialogMessage", out string message))
                DialogMessage = message;
            if (parameters.TryGetValue("DialogIcon", out string dialogIconName))
            {
                DialogIcon = LoadIconObject(dialogIconName);
            }
            if (parameters.TryGetValue("LeftButton", out DialogBoxButton leftButton))
            {
                LeftButton = leftButton;
            }
            if (parameters.TryGetValue("CentralButton", out DialogBoxButton centralButton))
            {
                CentralButton = centralButton;
            }
            if (parameters.TryGetValue("RightButton", out DialogBoxButton rightButton))
            {
                RightButton = rightButton;
            }
        }

        public object LoadIconObject(string iconName)
        {
            ResourceDictionary colorResources = new()
            {
                Source = new Uri("pack://application:,,,/Xcc.Application;Component/UI/Resources/Icons/CommonIcons.xaml", UriKind.RelativeOrAbsolute)
            };

            if (colorResources.Contains(iconName))
                return colorResources[iconName];
            else
                throw new Exception($"Required resource is missing. Resource key {iconName}");
        }
        #endregion IDialogAware
    }
}
