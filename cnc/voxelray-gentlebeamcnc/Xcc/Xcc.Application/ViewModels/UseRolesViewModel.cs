using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;
using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Users;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Enums;
using Xcc.Core.Infra.DataManagement.Common.DataAccess;
using Xcc.Core.Logging;

namespace Xcc.Application.ViewModels;
public class UserRolesViewModel(
    IAuthorizedUserStore authorizedUserStore,
    IRoleCommands roleCommands,
    IPermissionCommands permissionCommands,
    IUserRepository userRepository,
    ILogRepository logWriter,
    IDialogService dialogService,
    IEventAggregator eventAggregator) : DirtyFlaggedBindableBase, INavigationAware
{
    private IAuthorizedUserStore AuthorizedUserStore { get; } = authorizedUserStore;

    #region Properties

    public IEnumerable<string> ExistingUserRoleNames => UserRoles.Select(ur => ur.Name);

    public ObservableCollection<UserRole> UserRoles { get; set; } = [];

    private UserRole? _selectedUserRole;
    public UserRole? SelectedUserRole
    {
        get => _selectedUserRole;
        set
        {
            if (SetProperty(ref _selectedUserRole, value))
            {
                if (_selectedUserRole is null || _selectedUserRole.BuiltIn)
                    ClearUserRoleToEdit();
                else
                {
                    UserRoleToEdit = new UserRole(_selectedUserRole);
                    UserRoleNameToEdit = _selectedUserRole.Name;
                }
            }
        }
    }

    public string? SelectedUserRoleName => SelectedUserRole?.Name;

    private UserRole? _userRoleToEdit;
    public UserRole? UserRoleToEdit
    {
        get => _userRoleToEdit;
        set => SetProperty(ref _userRoleToEdit, value);
    }

    private string? _userRoleNameToEdit;
    [Required(ErrorMessage = StringConstants.Common.Validation.FieldRequiredError)]
    [NotExistsAlready(nameof(ExistingUserRoleNames), nameof(SelectedUserRoleName), ErrorMessage = StringConstants.Common.Validation.NameExistsAlready)]
    public string? UserRoleNameToEdit
    {
        get => _userRoleNameToEdit;
        set
        {
            if (SetProperty(ref _userRoleNameToEdit, value))
            {
                Validate(value);

                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private ObservableTask? _userRoleTask;
    public ObservableTask? UserRoleTask
    {
        get => _userRoleTask;
        private set => SetProperty(ref _userRoleTask, value);
    }

    #endregion Properties



    #region Commands

    private DelegateCommand? _cancelUserRoleTaskCommand;
    public DelegateCommand? CancelUserRoleTaskCommand
    {
        get => _cancelUserRoleTaskCommand;
        set => SetProperty(ref _cancelUserRoleTaskCommand, value);
    }

    private DelegateCommand? _retryUserTaskCommand;
    public DelegateCommand? RetryUserRolesTaskCommand
    {
        get => _retryUserTaskCommand;
        set => SetProperty(ref _retryUserTaskCommand, value);
    }

    private DelegateCommand? _saveCommand;
    public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(
        () =>
        {
            UserRoleToEdit!.Name = UserRoleNameToEdit!;

            if (SelectedUserRole is null)
            {
                // new user, call Create
                UserRoleTask = new ObservableTask(CreateUserRoleAsync(), StringConstants.SystemSettings.UserRoles.CreateUserRoleUiErrorMessage);

                RetryUserRolesTaskCommand = new DelegateCommand(() =>
                {
                    UserRoleTask = new ObservableTask(CreateUserRoleAsync(), StringConstants.SystemSettings.UserRoles.CreateUserRoleUiErrorMessage);
                });

                CancelUserRoleTaskCommand = new DelegateCommand(() => UserRoleTask = null);
            }
            else
            {
                // existing user, call Update
                UserRoleTask = new ObservableTask(SaveUserRoleAsync(), StringConstants.SystemSettings.UserRoles.SaveUserRoleUiErrorMessage);

                RetryUserRolesTaskCommand = new DelegateCommand(() =>
                {
                    UserRoleTask = new ObservableTask(SaveUserRoleAsync(), StringConstants.SystemSettings.UserRoles.SaveUserRoleUiErrorMessage);
                });

                CancelUserRoleTaskCommand = new DelegateCommand(() => UserRoleTask = null);
            }
        },
        canExecuteMethod: CanSave);
    
    private bool CanSave()
    {
        return UserRoleToEdit != null &&
               !string.IsNullOrEmpty(UserRoleNameToEdit) && 
               IsValid;
    }

    private DelegateCommand? _newCommand;
    public DelegateCommand NewCommand => _newCommand ??= new DelegateCommand(
        () =>
        {
            SelectedUserRole = null;
            //UserRoleToEditName = "New User Role";
            UserRoleToEdit = new UserRole("");
        });

    private DelegateCommand? _deleteCommand;
    public DelegateCommand DeleteCommand => _deleteCommand ??= new DelegateCommand(
        () =>
        {
            if (dialogService.Confirmation(StringConstants.Common.ConfirmationDialogTitle, StringConstants.SystemSettings.UserRoles.DeleteRoleConfirmationUiMessage) == false) 
                return;

            UserRoleTask = new ObservableTask(DeleteSelectedRoleAsync(), StringConstants.SystemSettings.UserRoles.DeleteUserRoleUiErrorMessage);

            RetryUserRolesTaskCommand = new DelegateCommand(() =>
            {
                UserRoleTask = new ObservableTask(DeleteSelectedRoleAsync(), StringConstants.SystemSettings.UserRoles.DeleteUserRoleUiErrorMessage);
            });

            CancelUserRoleTaskCommand = new DelegateCommand(() => UserRoleTask = null);
        }, 
        CanDeleteUserRole)
        .ObservesProperty(() => SelectedUserRole)
        .ObservesProperty(() => AuthorizedUserStore.AuthorizedUser);

    private bool CanDeleteUserRole() => SelectedUserRole is not null &&
                                        !SelectedUserRole.BuiltIn;

    private DelegateCommand? _cancelEditCommand;
    public DelegateCommand CancelEditCommand => _cancelEditCommand ??= new DelegateCommand(
        () =>
        {
            SelectedUserRole = null;
            ClearUserRoleToEdit();
        });
    #endregion Commands



    #region Private methods
    private async Task FetchUserRolesAsync()
    {
        try
        {
            UserRoles.Clear();
            UserRoles.AddRange(await userRepository.FetchAllUserRolesAsync());
        }
        catch(Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserRoles.FetchUserRolesErrorMessage} {ex.Message}",  
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    private async Task SaveUserRoleAsync()
    {
        try
        {
            if (SelectedUserRole is null || UserRoleToEdit is null)
                throw new Exception(StringConstants.SystemSettings.UserRoles.UserRoleIsNotSelectedErrorMessage);
            
            var storedRole = await roleCommands.UpdateAsync(SelectedUserRole, UserRoleToEdit);

            foreach (var permission in UserRoleToEdit.Permissions)
            {
                if (permission.Value)
                {
                    if (BaseEntry.IsBlankEntry(permission))
                    {
                        await permissionCommands.CreateAsync(new PermissionRecord
                            { RoleId = storedRole.Id, Type = permission.Type });
                    }
                }
                else
                {
                    if (BaseEntry.IsBlankEntry(permission) == false)
                    {
                        await permissionCommands.DeleteAsync(permission.Id);
                    }
                }
            }

            await FetchUserRolesAsync();
            eventAggregator.GetEvent<RoleChangedEvent>().Publish(storedRole);

            SelectedUserRole = null;
        }
        catch(Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserRoles.SaveUserRoleErrorMessage} {ex.Message}", 
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }
    
    private async Task CreateUserRoleAsync()
    {
        try
        {
            if (UserRoleToEdit is null)
                throw new Exception(StringConstants.SystemSettings.UserRoles.UserRoleIsNotSelectedErrorMessage);
            
            var storedRole = await roleCommands.CreateAsync(UserRoleToEdit);

            var creationTasks = new List<Task>();
            foreach (var permission in UserRoleToEdit.Permissions)
            {
                if (permission.Value)
                {
                    creationTasks.Add(permissionCommands.CreateAsync(new PermissionRecord { RoleId = storedRole.Id, Type = permission.Type }));
                }
            }

            await Task.WhenAll(creationTasks);

            await FetchUserRolesAsync();
            eventAggregator.GetEvent<RoleChangedEvent>().Publish(storedRole);
            
            ClearUserRoleToEdit();
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserRoles.CreateUserRoleErrorMessage} {ex.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }

    private async Task DeleteSelectedRoleAsync()
    {
        try
        {
            if (SelectedUserRole is null)
                throw new Exception(StringConstants.SystemSettings.UserRoles.UserRoleIsNotSelectedErrorMessage);

            if (AuthorizedUserStore.AuthorizedUser is null)
                throw new InvalidOperationException(StringConstants.Common.Authorization.NoAuthorizedUserErrorMessage);

            if (AuthorizedUserStore.AuthorizedUser.Role.Id == SelectedUserRole.Id)
            {
                dialogService.ReportError(StringConstants.Common.ErrorTitle, StringConstants.SystemSettings.UserRoles.DeleteAuthorizedUserRoleUiMessage);
                return;
            }

            foreach (var permission in SelectedUserRole.Permissions)
            {
                if (BaseEntry.IsBlankEntry(permission) == false)
                {
                    await permissionCommands.DeleteAsync(permission.Id);
                }
            }

            await roleCommands.DeleteAsync(SelectedUserRole.Id);
            eventAggregator.GetEvent<RoleChangedEvent>().Publish(SelectedUserRole);

            await FetchUserRolesAsync();
        }
        catch (Exception ex)
        {
            _ = logWriter.LogAsync(
                $"{StringConstants.SystemSettings.UserRoles.DeleteUserRoleErrorMessage} {ex.Message}",
                LogRecordSeverity.Error, LogRecordType.System);
            throw;
        }
    }
    
    private void ClearUserRoleToEdit()
    {
        UserRoleToEdit = null;
        UserRoleNameToEdit = string.Empty;
    }
    #endregion Private methods

    #region INavigationAware
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        UserRoleTask = new(FetchUserRolesAsync(), StringConstants.SystemSettings.UserRoles.FetchUserRolesUiErrorMessage);

        RetryUserRolesTaskCommand = new DelegateCommand(() =>
        {
            UserRoleTask = new ObservableTask(FetchUserRolesAsync(), StringConstants.SystemSettings.UserRoles.FetchUserRolesUiErrorMessage);
        });

        CancelUserRoleTaskCommand = null;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext) { }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    #endregion INavigationAware
}