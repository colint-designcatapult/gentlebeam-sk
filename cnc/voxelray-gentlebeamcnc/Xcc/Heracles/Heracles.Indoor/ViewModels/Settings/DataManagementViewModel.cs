using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Heracles.Indoor.Services;
using Heracles.Indoor.Views.Settings;
using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Xcc.Core.Enums;
using Xcc.Core.Services;

namespace Heracles.Indoor.ViewModels.Settings;

public sealed class DataManagementViewModel : BindableBase
{
    private const string KeyMask = "******-******-******-******-******-******-******-******";
    private const string DatabasePasswordMask = "********************************";
    private readonly DataManagementService _service;
    private readonly IDialogService _dialogs;
    private readonly IPopUpService _popups;
    private readonly Dispatcher _dispatcher;
    private CancellationTokenSource? _operationCancellation;
    private bool _active;
    private bool _isBusy;
    private bool _isRevealed;
    private string _recoveryKey = KeyMask;
    private string _databasePassword = DatabasePasswordMask;
    private string _statusMessage = string.Empty;

    public DataManagementViewModel(DataManagementService service, IDialogService dialogs, IPopUpService popups)
    {
        _service = service;
        _dialogs = dialogs;
        _popups = popups;
        _dispatcher = System.Windows.Application.Current.Dispatcher;
        ExportCommand = new DelegateCommand(async () => await TransferAsync(isExport: true), CanOperate);
        ImportCommand = new DelegateCommand(async () => await TransferAsync(isExport: false), CanOperate);
        CancelCommand = new DelegateCommand(() => _operationCancellation?.Cancel(),
            () => IsBusy && !_service.IsShuttingDown && _operationCancellation is not null);
    }

    public bool IsAvailable => _service.IsAvailable;
    public bool CanManage => _service.CanManage;
    public bool AdministratorRequired => IsAvailable && !CanManage;
    public bool IsUnavailable => !IsAvailable;
    public string RecoveryKey { get => _recoveryKey; private set => SetProperty(ref _recoveryKey, value); }
    public string DatabasePassword { get => _databasePassword; private set => SetProperty(ref _databasePassword, value); }
    public bool IsRevealed
    {
        get => _isRevealed;
        set
        {
            if (value == _isRevealed) return;
            if (!value) HideKey();
            else RevealKey();
        }
    }
    public string RecoveryKeyToggleAction => IsRevealed ? "Hide recovery key" : "Reveal recovery key";
    public bool CanToggleRecoveryKey => IsRevealed || CanOperate();
    public bool IsBusy { get => _isBusy; private set => SetProperty(ref _isBusy, value); }
    public string StatusMessage { get => _statusMessage; private set => SetProperty(ref _statusMessage, value); }
    public DelegateCommand ExportCommand { get; }
    public DelegateCommand ImportCommand { get; }
    public DelegateCommand CancelCommand { get; }

    public void Activate()
    {
        if (_active) return;
        _active = true;
        HideKey();
        _service.StateChanged += OnServiceStateChanged;
        RefreshState();
    }

    public void Deactivate()
    {
        _active = false;
        _service.StateChanged -= OnServiceStateChanged;
        HideKey();
        RefreshState();
    }

    private bool CanOperate() => _active && CanManage && !IsBusy && !_service.IsBusy;

    private void RevealKey()
    {
        if (!CanOperate())
        {
            RaisePropertyChanged(nameof(IsRevealed));
            return;
        }
        try
        {
            RecoveryKey = _service.RevealRecoveryKey();
            DatabasePassword = _service.RevealDatabasePassword();
            SetRevealed(true);
        }
        catch (Exception exception) { ShowFailure(exception); }
    }

    private void HideKey()
    {
        RecoveryKey = KeyMask;
        DatabasePassword = DatabasePasswordMask;
        SetRevealed(false);
    }

    private void SetRevealed(bool value)
    {
        SetProperty(ref _isRevealed, value, nameof(IsRevealed));
        RaisePropertyChanged(nameof(RecoveryKeyToggleAction));
        RaisePropertyChanged(nameof(CanToggleRecoveryKey));
    }

    private async Task TransferAsync(bool isExport)
    {
        if (!CanOperate()) return;
        HideKey();
        IsBusy = true;
        StatusMessage = string.Empty;
        RefreshState();
        string? password = null;
        try
        {
            if (!isExport && _popups.YesCancelDialog("Import database",
                    "Import replaces the entire database, including users and settings, and closes the application. Continue?",
                    "Continue", "Cancel", DialogBoxIconType.Warning) != DialogBoxResult.Yes)
                return;

            string path;
            if (isExport)
            {
                var dialog = new SaveFileDialog
                {
                    FileName = $"heracles-backup-{DateTime.Now:yyyyMMdd-HHmmss}.db",
                    Title = "Export database",
                    Filter = "Database (*.db)|*.db",
                    DefaultExt = ".db",
                    AddExtension = true,
                    OverwritePrompt = true,
                    CheckPathExists = true
                };
                if (dialog.ShowDialog() != true) return;
                path = dialog.FileName;
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Database (*.db)|*.db",
                    DefaultExt = ".db",
                    CheckFileExists = true,
                    Multiselect = false
                };
                if (dialog.ShowDialog() != true) return;
                path = dialog.FileName;
            }

            _dialogs.ShowDialog(nameof(DatabasePasswordView), new DialogParameters { { "IsExport", isExport } },
                result => password = DatabasePasswordViewModel.ConsumePassword(result));
            if (password is null) return;
            _operationCancellation = new CancellationTokenSource();
            StatusMessage = isExport ? "Exporting database..." : "Validating database...";
            RefreshState();
            Task operation = isExport
                ? _service.ExportAsync(path, password, _operationCancellation.Token)
                : _service.ImportAsync(path, password, _operationCancellation.Token);
            password = null;
            await operation;
            if (isExport)
            {
                StatusMessage = "Database exported.";
                _popups.ShowMessage("Export database", StatusMessage, ReportType.Info);
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Database operation cancelled. No database was replaced.";
        }
        catch (Exception exception)
        {
            if (!_service.IsShuttingDown) ShowFailure(exception);
        }
        finally
        {
            password = null;
            _operationCancellation?.Dispose();
            _operationCancellation = null;
            IsBusy = false;
            HideKey();
            RefreshState();
        }
    }

    private void ShowFailure(Exception exception)
    {
        HideKey();
        StatusMessage = exception switch
        {
            DataManagementException safe => safe.Message,
            UnauthorizedAccessException => "An administrator with an active, unlocked session is required.",
            _ => "The database operation could not be completed. Check the selected file and location, then try again."
        };
        _popups.ShowMessage("Data Management", StatusMessage, ReportType.Error);
    }

    private void OnServiceStateChanged(object? sender, EventArgs args)
    {
        if (_dispatcher.CheckAccess()) ApplyServiceChange();
        else _dispatcher.InvokeAsync(ApplyServiceChange);
    }

    private void ApplyServiceChange()
    {
        HideKey();
        if (_active) RefreshState();
    }

    private void RefreshState()
    {
        RaisePropertyChanged(nameof(IsAvailable));
        RaisePropertyChanged(nameof(CanManage));
        RaisePropertyChanged(nameof(AdministratorRequired));
        RaisePropertyChanged(nameof(IsUnavailable));
        RaisePropertyChanged(nameof(CanToggleRecoveryKey));
        ExportCommand.RaiseCanExecuteChanged();
        ImportCommand.RaiseCanExecuteChanged();
        CancelCommand.RaiseCanExecuteChanged();
    }
}
