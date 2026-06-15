using System.Windows;
using Prism.Commands;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.UI.Mvvm;
using Xcc.Core.Constants;
using Xcc.Core.Logging;
using Xcc.Core.Models;
using Xcc.Application.UI.UserControls;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Application.ViewModels
{
    public class LoginViewModel : DialogViewModelBase
    {
        public LoginViewModel(
            IAuthorizationService authorizationService, 
            IAuthorizedUserStore authorizedUserStore,
            IBearerTokenUserSessionManager bearerTokenUserSessionManager,
            ICoreSettings coreSettings, 
            ILogRepository logWriter)
        {
            AuthorizationService = authorizationService;
            LogWriter = logWriter;
            AuthorizedUserStore = authorizedUserStore;
            Username = coreSettings.StartupLoginUsername;

            // Select either currently authorized user (for locked sessions) or the one from the settings
            var session = bearerTokenUserSessionManager.UserSession;
            if (session != BearerTokenUserSession.NoSession) {
                Username = session.Username;
                if (session.IsExpired) 
                {
                    ErrorMessage = StringConstants.Common.Authorization.SessionExpirationError;
                }
            }

            KeyboardAppearance.KeyboardVisibilityChanged += (_, visibility) => ShowVirtualKeyboard = visibility is Visibility.Visible;
        }

        #region Injected Dependencies
        private IAuthorizationService AuthorizationService { get; }
        private ILogRepository LogWriter { get; }
        private IAuthorizedUserStore AuthorizedUserStore { get; } 
        #endregion Injected Dependencies


        #region Properties
        private string _username = string.Empty;
        public string Username
        {
            get => _username;
            set
            {
                SetProperty(ref _username, value);
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(_username) == false)
                {
                    SignInCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _password = string.Empty;
        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                ErrorMessage = string.Empty;

                if (string.IsNullOrWhiteSpace(_password) == false)
                {
                    SignInCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _revealPassword;
        public bool RevealPassword
        {
            get => _revealPassword;
            set => SetProperty(ref _revealPassword, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage { get => _errorMessage; set => SetProperty(ref _errorMessage, value); }

        private DelegateCommand? _signInCommand;
        public DelegateCommand SignInCommand => _signInCommand ??= new DelegateCommand(
            async () =>
            {
                try
                {
                    ErrorMessage = string.Empty;
                    AuthorizedUserStore.AuthorizedUser = await AuthorizationService.LoginAsync(Username, Password);
                }
                catch (AuthorizationServiceException ex)
                {
                    ErrorMessage = ex.Message;

                    _ = LogWriter.LogAsync(
                        $"{ex.Message}: {ex.Details}",
                        Core.Enums.LogRecordSeverity.Error,
                        Core.Enums.LogRecordType.System);
                    return;
                }

                CloseDialog();
                ShowVirtualKeyboard = false;
            });

        private DelegateCommand? _cancelCommand;
        public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(CancelDialog);

        public override string Title { get; set; } = StringConstants.Common.Authorization.LoginDialogTitle;
        #endregion Properties


        #region Virtual keyboard properties
        public XccKeyboardAppearance KeyboardAppearance { get; } = XccKeyboardAppearance.Instance;


        private bool _showVirtualKeyboard;
        public bool ShowVirtualKeyboard
        {
            get => _showVirtualKeyboard;
            set
            {
                if (SetProperty(ref _showVirtualKeyboard, value))
                {
                    KeyboardAppearance.KeyboardVisibility = value ? Visibility.Visible : Visibility.Hidden;
                }
            }
        }
        #endregion Virtual keyboard properties
    }
}
