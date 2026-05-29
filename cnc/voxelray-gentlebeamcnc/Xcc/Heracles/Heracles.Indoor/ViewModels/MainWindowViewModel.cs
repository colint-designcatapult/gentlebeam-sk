using Heracles.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Threading.Tasks;
using System.Windows;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.UI;
using Xcc.Application.UI.UserControls;
using Xcc.Infra.UserSessions;
using Xcc.Infra.UserSessions.BearerToken;

namespace Heracles.Indoor.ViewModels
{
    internal class MainWindowViewModel : BindableBase
    {
        public IAuthorizedUserStore AuthorizedUserStore { get; }
        public IDialogService DialogService { get; }
        public IRegionManager RegionManager { get; }
        public IBearerTokenUserSessionManager UserSessionManagement { get; }

        public MainWindowViewModel(
            IHeraclesMainSettings heraclesMainSettings, 
            IAuthorizedUserStore authorizedUserStore, 
            IDialogService dialogService,
            IRegionManager regionManager,
            IBearerTokenUserSessionManager userSessionManagement)
        {
            AuthorizedUserStore = authorizedUserStore;
            DialogService = dialogService;
            RegionManager = regionManager;
            UserSessionManagement = userSessionManagement;
            WinState = heraclesMainSettings.DoNotExpandFullscreen ? WindowState.Normal : WindowState.Maximized;

            WinStyle = WindowStyle.None;
            WinResizeMode = ResizeMode.NoResize;

            KeyboardAppearance.KeyboardVisibilityChanged += (_, visibility) => ShowVirtualKeyboard = visibility is Visibility.Visible;

            // TODO: this code duplicates in all apps - better put it to some model:
            UserSessionManagement.UserSessionChanged += (_, userSessionArgs) =>
            {
                Task.Run(() => 
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnUserSessionChanged(userSessionArgs);
                    });
                });
            };
        }

        #region UI Properties
        private string _title = "Heracles Indoor";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private WindowState _winState;
        public WindowState WinState
        {
            get => _winState;
            set => SetProperty(ref _winState, value);
        }

        private WindowStyle _winStyle;
        public WindowStyle WinStyle
        {
            get => _winStyle;
            set => SetProperty(ref _winStyle, value);
        }

        private ResizeMode _winResizeMode;
        public ResizeMode WinResizeMode
        {
            get => _winResizeMode;
            set => SetProperty(ref _winResizeMode, value);
        }

        // TODO: this property code duplicates in all apps - better put it to some model:
        private bool _showSessionLock = false;
        public bool ShowSessionLock
        {
            get => _showSessionLock;
            set => SetProperty(ref _showSessionLock, value);
        }
        #endregion UI Properties


        private DelegateCommand? _loginCommand;
        public DelegateCommand LoginCommand => _loginCommand ??= new DelegateCommand(
            () =>
            {
                DialogService.ShowDialog("LoginView");
            });

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
                    RegionManager.RequestNavigate(Regions.MainRegion, "MainTabsView");
                    break;
                case UserSessionEventType.Lock:
                case UserSessionEventType.Expiration:
                    ShowSessionLock = true;
                    break;
                case UserSessionEventType.Locked:
                    if (userSessionArgs.UserSession.IsExpired)
                    {
                        DialogService.ShowDialog("LoginView", r =>
                        {
                            if (r.Result == ButtonResult.Cancel)
                            {
                                // TODO: now we perform logout on Login close button
                                // when session is expired
                                UserSessionManagement.CloseUserSession();
                                AuthorizedUserStore.AuthorizedUser = null;
                            }
                        });
                    }
                    break;
            }
        }

        #endregion Private methods


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
