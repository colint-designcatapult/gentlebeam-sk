using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Users;
using Xcc.Application.AppLayer.Service;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Enums;
using Xcc.Core.Logging;
using Xcc.Infra.UserSessions;
using Xcc.Infra.UserSessions.BearerToken;

namespace Xcc.Application.ViewModels;

public class UserManagementViewModel(
    IAuthorizedUserStore authorizedUserStore,
    IUserRepository userRepository,
    ILogRepository logWriter,
    IDialogService dialogService,
    IActionAuditService actionAuditService,
    IBearerTokenUserSessionManager userSessionManager,
    IEventAggregator eventAggregator) 
    : BindableBase, INavigationAware
{
    public IAuthorizedUserStore AuthorizedUserStore { get; } = authorizedUserStore;


    #region Properties
    public ObservableCollection<IUser> Users { get; set; } = [];

    public bool IsAdministrator => AuthorizedUserStore.AuthorizedUser?.Role.Name == UserRole.BuiltInNames.Administrator;

    private IUser? _selectedUser;
    public IUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (SetProperty(ref _selectedUser, value))
            {
                UserToEdit = (_selectedUser != null && CanEditUser(_selectedUser)) ? new UserBindable(_selectedUser!) : null;
            }
        }
    }

    private UserBindable? _userToEdit;
    public UserBindable? UserToEdit
    {
        get => _userToEdit;
        set
        {
            if (SetProperty(ref _userToEdit, value))
            {
                if (_userToEdit is null)
                    return;

                SelectedUserRole = UserRoles.FirstOrDefault(x => _userToEdit.Role.Id == x.Id);

                RevealPassword = false;
                RevealConfirmPassword = false;

                RaisePropertyChanged(nameof(CanSave));
                _userToEdit.IsModifiedChanged += (_,_) => RaisePropertyChanged(nameof(CanSave));
                _userToEdit.IsValidChanged += (_, _) => RaisePropertyChanged(nameof(CanSave));
            }
        }
    }

    public ObservableCollection<UserRole> UserRoles { get; set; } = [];

    private UserRole? _selectedUserRole;
    public UserRole? SelectedUserRole
    {
        get => _selectedUserRole;
        set
        {
            if (SetProperty(ref _selectedUserRole, value))
            {
                if (UserToEdit is null || SelectedUserRole is null)
                    return;

                if(SelectedUserRole.Id == UserToEdit.Role.Id)
                    return;

                UserToEdit.Role = SelectedUserRole;
            }
        }
    }

    private bool _revealPassword;
    public bool RevealPassword
    {
        get => _revealPassword;
        set => SetProperty(ref _revealPassword, value);
    }

    private bool _revealConfirmPassword;
    public bool RevealConfirmPassword
    {
        get => _revealConfirmPassword;
        set => SetProperty(ref _revealConfirmPassword, value);
    }
    #endregion Properties


    #region Commands
    private DelegateCommand? _saveCommand;
    public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
        () =>
        {
            if (SelectedUser is null)
            {
                CreateUser();
            }
            else
            {
                SaveUser();
            }
        }).ObservesCanExecute(() => CanSave);

    private bool CanSave => UserToEdit is not null && UserToEdit.IsModified && UserToEdit.IsValid;

    private DelegateCommand? _newCommand;
    public DelegateCommand NewCommand => _newCommand ??= new DelegateCommand(
        () =>
        {
            SelectedUser = null;
            UserToEdit = new UserBindable();
        });

    private DelegateCommand? _deleteCommand;
    public DelegateCommand DeleteCommand => _deleteCommand ??= new DelegateCommand(
        () =>
        {
            if (dialogService.Confirmation(StringConstants.Common.ConfirmationDialogTitle, StringConstants.SystemSettings.UserManagement.DeleteUserConfirmationUiMessage))
            {
                UserTask = new ObservableTask(DeleteSelectedUserAsync(), StringConstants.SystemSettings.UserManagement.DeleteUserUiErrorMessage);

                RetryUserTaskCommand = new DelegateCommand(() =>
                {
                    UserTask = new ObservableTask(DeleteSelectedUserAsync(), StringConstants.SystemSettings.UserManagement.DeleteUserUiErrorMessage);
                });

                CancelUserTaskCommand = new DelegateCommand(() => UserTask = null);
            }
        }, 
        CanDeleteUser)
        .ObservesProperty(() => SelectedUser)
        .ObservesProperty(() => AuthorizedUserStore.AuthorizedUser);


    private DelegateCommand? _resetLockoutCommand;
    public DelegateCommand ResetLockoutCommand => _resetLockoutCommand ??= new DelegateCommand(
        ResetSelectedUserLockout, CanResetLockout)
        .ObservesProperty(() => SelectedUser)
        .ObservesProperty(() => AuthorizedUserStore.AuthorizedUser)
        .ObservesProperty(() => UserTask);
    private DelegateCommand? _cancelEditCommand;
    public DelegateCommand CancelEditCommand => _cancelEditCommand ??= new DelegateCommand(
        () =>
        {
            SelectedUser = null;
            UserToEdit = null;
        });

    private DelegateCommand? _loadPictureCommand;
    public DelegateCommand LoadPictureCommand => _loadPictureCommand ??= new DelegateCommand(
        () =>
        {
            if (UserToEdit is not null)
            {
                var openFileDialog = new System.Windows.Forms.OpenFileDialog
                {
                    Multiselect = false,
                    Filter = "Image Files(*.jpg;*.jpeg;*.png;*.bmp;)|*.jpg;*.jpeg;*.png;*.bmp"
                };

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    UserToEdit.Picture = openFileDialog.FileName;
                }
            }
            else
            {
                dialogService.ReportError(StringConstants.Common.ErrorTitle, StringConstants.SystemSettings.UserManagement.UserIsNotSelectedErrorMessage);
            }
        });
    #endregion Commands

    
    #region User task properties
    private ObservableTask? _userTask;
    public ObservableTask? UserTask
    {
        get => _userTask;
        private set
        {
            if (_userTask is not null)
                _userTask.PropertyChanged -= OnUserTaskChanged;
            if (SetProperty(ref _userTask, value))
                _resetLockoutCommand?.RaiseCanExecuteChanged();
            if (_userTask is not null)
                _userTask.PropertyChanged += OnUserTaskChanged;
        }
    }

    private DelegateCommand? _cancelUserTaskCommand;
    public DelegateCommand? CancelUserTaskCommand
    {
        get => _cancelUserTaskCommand;
        set => SetProperty(ref _cancelUserTaskCommand, value);
    }

    private DelegateCommand? _retryUserTaskCommand;
    public DelegateCommand? RetryUserTaskCommand
    {
        get => _retryUserTaskCommand;
        set => SetProperty(ref _retryUserTaskCommand, value);
    }
    #endregion  User task properties


    #region Private methods
    private async Task FetchUsersAsync(bool preserveDraft = false)
    {
        try
        {
            var users = await userRepository.FetchUsersAsync();
            var selectedId = SelectedUser?.Id;
            var draft = preserveDraft && UserToEdit?.IsModified == true ? UserToEdit : null;
            Users.Clear();
            Users.AddRange(users);
            SelectedUser = Users.FirstOrDefault(user => user.Id == selectedId);
            if (draft is not null && selectedId == SelectedUser?.Id)
                UserToEdit = draft;
        }
        catch(Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserManagement.FetchUsersErrorMessage}. {ex.Message}", 
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }


    private void SaveUser()
    {
        UserTask = new ObservableTask(SaveUserAsync(), StringConstants.SystemSettings.UserManagement.SaveUserUiErrorMessage);

        RetryUserTaskCommand = new DelegateCommand(() =>
        {
            UserTask = new ObservableTask(SaveUserAsync(), StringConstants.SystemSettings.UserManagement.SaveUserUiErrorMessage);
        });

        CancelUserTaskCommand = new DelegateCommand(() => UserTask = null);
    }

    private async Task SaveUserAsync()
    {
        try
        {
            if (UserToEdit is null)
                throw new Exception(StringConstants.SystemSettings.UserManagement.UserIsNotSelectedErrorMessage);

            await userRepository.SaveUserAsync(UserToEdit.ToUser(), actionAuditService.RegisterAction);
            await FetchUsersAsync();

            SelectedUser = null;
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserManagement.SaveUserErrorMessage}. {ex.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }



    private void CreateUser()
    {
        UserTask = new ObservableTask(CreateUserAsync(), StringConstants.SystemSettings.UserManagement.CreateUserUiErrorMessage);

        RetryUserTaskCommand = new DelegateCommand(() =>
        {
            UserTask = new ObservableTask(CreateUserAsync(), StringConstants.SystemSettings.UserManagement.CreateUserUiErrorMessage);
        });

        CancelUserTaskCommand = new DelegateCommand(() => UserTask = null);
    }

    private async Task CreateUserAsync()
    {
        try
        {
            if (UserToEdit is null)
                throw new Exception(StringConstants.SystemSettings.UserManagement.UserIsNotSelectedErrorMessage);

            await userRepository.CreateUserAsync(UserToEdit.ToUser(), actionAuditService.RegisterAction);
            await FetchUsersAsync();

            UserToEdit = null;
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserManagement.CreateUserErrorMessage}. {ex.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    private async Task DeleteSelectedUserAsync()
    {
        try
        {
            if (SelectedUser is null)
                throw new Exception(StringConstants.SystemSettings.UserManagement.UserIsNotSelectedErrorMessage);

            if(AuthorizedUserStore.AuthorizedUser is null)
                throw new InvalidOperationException(StringConstants.Common.Authorization.NoAuthorizedUserErrorMessage);

            if (SelectedUser.Id == AuthorizedUserStore.AuthorizedUser.Id)
            {
                dialogService.ReportError(StringConstants.Common.ErrorTitle, StringConstants.SystemSettings.UserManagement.DeleteAuthorizedUserUiMessage);
                return;
            }

            await userRepository.DeleteUserAsync(SelectedUser.Id, actionAuditService.RegisterAction);
            await FetchUsersAsync();
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserManagement.DeleteUserErrorMessage}. {ex.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    private bool _confirmingResetLockout;

    private bool HasResetLockoutAuthorization()
    {
        var actor = AuthorizedUserStore.AuthorizedUser;
        var session = userSessionManager.UserSession;
        return IsAdministrator && actor is { Id: > 0, IsLocked: false } &&
            !session.IsLocked && !session.IsExpired && !session.CancellationToken.IsCancellationRequested &&
            session.Username == actor.Username;
    }

    private bool CanResetLockout() =>
        !_confirmingResetLockout && UserTask?.IsNotCompleted != true &&
        SelectedUser is { Id: > 0, IsLocked: true } && HasResetLockoutAuthorization();

    private void ResetSelectedUserLockout()
    {
        if (!CanResetLockout())
            return;

        var targetId = SelectedUser!.Id;
        var targetUsername = SelectedUser.Username;
        var actorId = AuthorizedUserStore.AuthorizedUser!.Id;
        var session = userSessionManager.UserSession;
        _confirmingResetLockout = true;
        _resetLockoutCommand?.RaiseCanExecuteChanged();
        try
        {
            if (!dialogService.Confirmation(StringConstants.Common.ConfirmationDialogTitle,
                string.Format(StringConstants.SystemSettings.UserManagement.ResetLockoutConfirmationUiMessage, targetUsername, targetId)))
                return;

            var resetCompleted = false;
            async Task ResetAndRefreshAsync()
            {
                try
                {
                    if (!HasResetLockoutAuthorization() || AuthorizedUserStore.AuthorizedUser?.Id != actorId ||
                        !ReferenceEquals(userSessionManager.UserSession, session))
                        throw new UnauthorizedAccessException(StringConstants.SystemSettings.UserManagement.ResetLockoutAuthorizationErrorMessage);

                    if (!resetCompleted)
                    {
                        await userRepository.ResetUserLockoutAsync(targetId);
                        resetCompleted = true;
                    }
                    await FetchUsersAsync(preserveDraft: true);
                }
                catch (Exception ex)
                {
                    _ = logWriter.LogAsync(
                        $"{StringConstants.SystemSettings.UserManagement.ResetLockoutErrorMessage}. {ex.Message}",
                        LogRecordSeverity.Error, LogRecordType.System);
                    throw;
                }
            }

            RetryUserTaskCommand = new DelegateCommand(() =>
            {
                if (UserTask?.IsNotCompleted != true)
                    UserTask = new ObservableTask(ResetAndRefreshAsync(), StringConstants.SystemSettings.UserManagement.ResetLockoutUiErrorMessage);
            });
            CancelUserTaskCommand = new DelegateCommand(() =>
            {
                if (UserTask?.IsNotCompleted != true)
                    UserTask = null;
            });
            UserTask = new ObservableTask(ResetAndRefreshAsync(), StringConstants.SystemSettings.UserManagement.ResetLockoutUiErrorMessage);
        }
        finally
        {
            _confirmingResetLockout = false;
            _resetLockoutCommand?.RaiseCanExecuteChanged();
        }
    }

    private void OnUserTaskChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(ObservableTask.IsCompleted))
            _resetLockoutCommand?.RaiseCanExecuteChanged();
    }

    private void OnAuthorizedUserChanged(object? sender, IUser? user)
    {
        RaisePropertyChanged(nameof(IsAdministrator));
        _resetLockoutCommand?.RaiseCanExecuteChanged();
    }

    private void OnUserSessionChanged(object? sender, UserSessionEventArgs args) =>
        _resetLockoutCommand?.RaiseCanExecuteChanged();
    
    private async Task FetchUserRolesAsync()
    {
        try
        {
            var selectedUserRole = SelectedUserRole;

            UserRoles.Clear();
            UserRoles.AddRange(await userRepository.FetchAllUserRolesAsync());

            SelectedUserRole = UserRoles.FirstOrDefault(x => x.Id == selectedUserRole?.Id);
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserRoles.FetchUserRolesErrorMessage}. {ex.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    private void FetchUserRoles()
    {
        UserTask = new(FetchUserRolesAsync(), StringConstants.SystemSettings.UserRoles.FetchUserRolesUiErrorMessage);

        RetryUserTaskCommand = new DelegateCommand(() =>
        {
            UserTask = new ObservableTask(FetchUserRolesAsync(), StringConstants.SystemSettings.UserRoles.FetchUserRolesUiErrorMessage);
        });
    }

    private async Task FetchUsersAndRolesAsync()
    {
        await FetchUsersAsync();
        await FetchUserRolesAsync();
    }

    private bool CanDeleteUser() =>
       SelectedUser is not null &&
       AuthorizedUserStore.AuthorizedUser is not null &&
       SelectedUser.Id != AuthorizedUserStore.AuthorizedUser.Id;

    private bool CanEditUser(IUser selectedUser)
    {
        // Don't allow for the users to edit themselves
        return AuthorizedUserStore.AuthorizedUser is not null &&
            selectedUser.Id != AuthorizedUserStore.AuthorizedUser.Id;
    }
    #endregion Private methods


    #region INavigationAware
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        AuthorizedUserStore.AuthorizedUserChanged -= OnAuthorizedUserChanged;
        AuthorizedUserStore.AuthorizedUserChanged += OnAuthorizedUserChanged;
        userSessionManager.UserSessionChanged -= OnUserSessionChanged;
        userSessionManager.UserSessionChanged += OnUserSessionChanged;
        RaisePropertyChanged(nameof(IsAdministrator));
        _resetLockoutCommand?.RaiseCanExecuteChanged();

        UserTask = new(FetchUsersAndRolesAsync(), StringConstants.SystemSettings.UserManagement.FetchUsersUiErrorMessage);

        RetryUserTaskCommand = new DelegateCommand(() =>
        {
            UserTask = new ObservableTask(FetchUsersAndRolesAsync(), StringConstants.SystemSettings.UserManagement.FetchUsersUiErrorMessage);
        });

        eventAggregator.GetEvent<RoleChangedEvent>().Subscribe(_ =>
        {
            if (UserTask.IsNotCompleted)
                UserTask.ContinueWith = FetchUserRoles;
            else
                FetchUserRoles();
        });
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        AuthorizedUserStore.AuthorizedUserChanged -= OnAuthorizedUserChanged;
        userSessionManager.UserSessionChanged -= OnUserSessionChanged;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    #endregion INavigationAware
}