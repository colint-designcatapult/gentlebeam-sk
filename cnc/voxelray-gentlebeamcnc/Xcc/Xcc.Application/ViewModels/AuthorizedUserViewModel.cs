using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Application.ViewModels
{
    public class AuthorizedUserViewModel(
        IAuthorizedUserStore authorizedUserStore, 
        IBearerTokenUserSessionManager userSessionManagement,
        IDialogService dialogService) : BindableBase
    {
        public IAuthorizedUserStore AuthorizedUserStore { get; } = authorizedUserStore;


        private DelegateCommand? _logOutCommand;
        public DelegateCommand LogOutCommand => _logOutCommand ??= new DelegateCommand(
            () =>
            {
                LogoutMenuIsOpen = false;

                userSessionManagement.CloseUserSession();
                AuthorizedUserStore.AuthorizedUser = null;

                dialogService.ShowDialog("LoginView");
            });

        private DelegateCommand? _lockSessionCommand;
        public DelegateCommand LockSessionCommand => _lockSessionCommand ??= new DelegateCommand(
            () =>
            {
                LogoutMenuIsOpen = false;

                userSessionManagement.LockUserSession();

                dialogService.ShowDialog("LoginView", r =>
                {
                    if (r.Result == ButtonResult.Cancel)
                    {
                        // TODO: now we perform logout on Login close button
                        // when session is expired or locked
                        userSessionManagement.CloseUserSession();
                        AuthorizedUserStore.AuthorizedUser = null;
                    }
                });

            });


        private bool _logoutMenuIsOpen;
        public bool LogoutMenuIsOpen
        {
            get => _logoutMenuIsOpen;
            set => SetProperty(ref _logoutMenuIsOpen, value);
        }
    }
}
