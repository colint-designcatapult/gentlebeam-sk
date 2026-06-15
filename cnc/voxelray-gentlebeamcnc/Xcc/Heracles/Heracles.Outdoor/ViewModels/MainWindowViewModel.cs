using Heracles.Core.Models;
using Heracles.External.Models;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Windows;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.UI;
using Xcc.Application.UI.UserControls;
using Xcc.Infra.UserSessions;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.External.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(
            IHeraclesExternalSettings heraclesExternalSettings,
            IAuthorizedUserStore authorizedUserStore,
            IDialogService dialogService,
            IRegionManager regionManager,
            IUIStateMachine uiStateMachine,
            IBearerTokenUserSessionManager userSessionManagement) 
        {
            WinState = heraclesExternalSettings.DoNotExpandFullscreen ? WindowState.Normal : WindowState.Maximized;

            KeyboardAppearance.KeyboardVisibilityChanged += (_, visibility) => ShowVirtualKeyboard = visibility is Visibility.Visible;
            AuthorizedUserStore = authorizedUserStore;
            DialogService = dialogService;
            RegionManager = regionManager;
            UIStateMachine = uiStateMachine;
            UserSessionManager = userSessionManagement;

            // TODO: this code duplicates in all apps - better put it to some model:
            UserSessionManager.UserSessionChanged += (_, userSessionArgs) =>
            {
                Task.Run(() =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnUserSessionChanged(userSessionArgs);
                    });
                });
            };
            UIStateMachine.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IUIStateMachine.State))
                {
                    OnUiStateChanged(UIStateMachine.State);
                }
            };
        }

        private WindowState _winState;
        public WindowState WinState
        {
            get { return _winState; }
            set { SetProperty(ref _winState, value); }
        }

        // TODO: this property code duplicates in all apps - better put it to some model:
        private bool _showSessionLock = false;
        public bool ShowSessionLock
        {
            get => _showSessionLock;
            set => SetProperty(ref _showSessionLock, value);
        }



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

        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public IDialogService DialogService { get; }
        public IRegionManager RegionManager { get; }
        public IUIStateMachine UIStateMachine { get; }
        public IBearerTokenUserSessionManager UserSessionManager { get; }
        #endregion Virtual keyboard properties


        #region Private methods
        // TODO: this code duplicates in all apps + main part of it is in Xcc AuthorizedUserViewModel - better put it to a service
        private void OnUserSessionChanged(UserSessionEventArgs userSessionArgs)
        {
            switch (userSessionArgs.EventType)
            {
                case UserSessionEventType.Open:
                case UserSessionEventType.Unlocked:
                    ShowSessionLock = false;
                    break;
                case UserSessionEventType.Close:
                    ShowSessionLock = false;
                    RegionManager.RequestNavigate(Regions.ExternalRegion, "ExternalTabsView");
                    break;
                case UserSessionEventType.Locked:
                    // session either was locked by user, or it was expired in stand-by mode, which is safe to lock
                    if (UIStateMachine.State == UIMacroState.StandBy || !userSessionArgs.UserSession.IsExpired)
                    {
                        OnSessionLocked(userSessionArgs.UserSession.IsExpired);
                    }
                    break;
                default:
                    break;
            }
        }

        private void OnSessionLocked(bool showLoginView = false)
        {
            ShowSessionLock = true;
            if (showLoginView)
            {
                // For any other event type, we already show this dialog from AuthorizedUserView
                DialogService.ShowDialog("LoginView", r =>
                {
                    if (r.Result == ButtonResult.Cancel)
                    {
                        // TODO: now we perform logout on Login close button
                        // when session is expired
                        UserSessionManager.CloseUserSession();
                        AuthorizedUserStore.AuthorizedUser = null;
                    }
                });
            }
        }

        private void OnUiStateChanged(UIMacroState state)
        {
            if (state == UIMacroState.StandBy &&
                !ShowSessionLock &&
                UserSessionManager.UserSession != BearerTokenUserSession.NoSession &&
                UserSessionManager.UserSession.IsExpired)
            {
                OnSessionLocked(showLoginView: true);
            }
        }

        #endregion Private methods
    }
}
