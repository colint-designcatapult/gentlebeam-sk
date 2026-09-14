using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;

using System;
using System.Linq;
using System.Threading.Tasks;

using Xcc.Application.AppLayer.Model;
using Xcc.Application.AppLayer.Users;
using Xcc.Application.Common;
using Xcc.Application.Helpers;
using Xcc.Core.Constants;
using Xcc.Core.Domain.DataManagement.Common.Users;
using Xcc.Core.Domain.DataManagement.Common.Users.DataAccess;
using Xcc.Core.Enums;
using Xcc.Core.Logging;

namespace Xcc.Application.ViewModels;

/// <summary>
/// ViewModel for the first-run administrator setup dialog.
/// Allows the user to create the initial Administrator account when no users exist.
/// </summary>
public class FirstRunAdminSetupViewModel(
    IUserRepository userRepository,
    ILogRepository logWriter)
    : BindableBase, IDialogAware
{
    public string Title => "First-Run Administrator Setup";

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

                RevealPassword = false;
                RevealConfirmPassword = false;

                RaisePropertyChanged(nameof(CanSave));
                _userToEdit.IsModifiedChanged += (_, _) => RaisePropertyChanged(nameof(CanSave));
                _userToEdit.IsValidChanged += (_, _) => RaisePropertyChanged(nameof(CanSave));
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

    private ObservableTask? _setupTask;
    public ObservableTask? SetupTask
    {
        get => _setupTask;
        private set => SetProperty(ref _setupTask, value);
    }

    private DelegateCommand? _cancelSetupTaskCommand;
    public DelegateCommand? CancelSetupTaskCommand
    {
        get => _cancelSetupTaskCommand;
        set => SetProperty(ref _cancelSetupTaskCommand, value);
    }

    public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(CreateAdministrator)
        .ObservesCanExecute(() => CanSave);

    public DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(() =>
    {
        RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
    });

    private bool CanSave => UserToEdit is not null && UserToEdit.IsModified && UserToEdit.IsValid;

    private DelegateCommand? _saveCommand;
    private DelegateCommand? _cancelCommand;

    public event Action<IDialogResult>? RequestClose;

    public FirstRunAdminSetupViewModel() : this(null!, null!)
    {
        // Default constructor for designer support
    }

    public void OnDialogOpened(IDialogParameters parameters)
    {
        // Initialize a new user for admin creation
        UserToEdit = new UserBindable();
    }

    public bool CanCloseDialog() => true;

    public void OnDialogClosed()
    {
    }

    private void CreateAdministrator()
    {
        SetupTask = new ObservableTask(CreateAdministratorAsync(), StringConstants.SystemSettings.UserManagement.CreateUserUiErrorMessage);

        CancelSetupTaskCommand = new DelegateCommand(() => SetupTask = null);
    }

    private async Task CreateAdministratorAsync()
    {
        try
        {
            if (UserToEdit is null)
                throw new Exception("User data is not initialized");

            var administratorRole = (await userRepository.FetchAllUserRolesAsync())
                .FirstOrDefault(role => role.Name == "Administrator");
            if (administratorRole is null)
                throw new InvalidOperationException("The Administrator role is not available.");

            UserToEdit.Role = administratorRole;

            // Create the user and its Administrator role mapping
            var user = UserToEdit.ToUser();
            await userRepository.CreateUserAsync(user);

            await logWriter.LogAsync(
                $"First-run administrator account created: {user.Username}",
                LogRecordSeverity.Info,
                LogRecordType.System);

            // Close the dialog with success
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }
        catch (Exception ex)
        {
            await logWriter.LogAsync(
                $"First-run administrator setup error: {ex.Message}",
                LogRecordSeverity.Error,
                LogRecordType.Error);
            throw;
        }
    }
}
