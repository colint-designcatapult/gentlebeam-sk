using System;
using Prism.Commands;
using Prism.Services.Dialogs;
using Xcc.Application.UI.Mvvm;

namespace Heracles.Indoor.ViewModels.Settings;

public sealed class DatabasePasswordViewModel : DialogViewModelBase
{
    private string _password = string.Empty;
    private string _confirmation = string.Empty;
    private string _validationMessage = string.Empty;
    private bool _isExport;

    public DatabasePasswordViewModel()
    {
        ContinueCommand = new DelegateCommand(Submit);
        CancelCommand = new DelegateCommand(CancelDialog);
    }

    public bool IsExport { get => _isExport; private set => SetProperty(ref _isExport, value); }
    public string ValidationMessage { get => _validationMessage; private set => SetProperty(ref _validationMessage, value); }
    public DelegateCommand ContinueCommand { get; }
    public DelegateCommand CancelCommand { get; }
    public event EventHandler? ClearPasswordControls;

    protected override void SetDialogParameters(IDialogParameters parameters)
    {
        ClearSecrets();
        IsExport = parameters.GetValue<bool>("IsExport");
        Title = IsExport ? "Export database password" : "Import database password";
        ValidationMessage = string.Empty;
    }

    public void SetPasswords(string password, string confirmation)
    {
        _password = password;
        _confirmation = confirmation;
        ValidationMessage = string.Empty;
    }

    private void Submit()
    {
        if (_password.Contains('\0') ||
            (_password.Length != 0 && string.IsNullOrWhiteSpace(_password)))
        {
            ValidationMessage = "The password contains unsupported characters.";
            return;
        }
        if (IsExport && !string.Equals(_password, _confirmation, StringComparison.Ordinal))
        {
            ValidationMessage = "The passwords do not match.";
            return;
        }
        OnRequestClose(new PasswordResult(_password));
    }

    public override void OnRequestClose(IDialogResult dialogResult)
    {
        try
        {
            base.OnRequestClose(dialogResult);
        }
        finally
        {
            if (dialogResult is PasswordResult result) result.Clear();
            ClearSecrets();
        }
    }

    public void ClearSecrets()
    {
        _password = string.Empty;
        _confirmation = string.Empty;
        ClearPasswordControls?.Invoke(this, EventArgs.Empty);
    }

    // Prism's stock DialogParameters is append-only. Drop the entire result's parameter
    // reference immediately in the callback, rather than retaining a password on a closed dialog.
    public static string? ConsumePassword(IDialogResult dialogResult)
    {
        try
        {
            return dialogResult.Result == ButtonResult.OK
                ? dialogResult.Parameters.GetValue<string>("Password")
                : null;
        }
        finally
        {
            if (dialogResult is PasswordResult result) result.Clear();
        }
    }

    private sealed class PasswordResult(string password) : IDialogResult
    {
        public ButtonResult Result => ButtonResult.OK;
        public IDialogParameters Parameters { get; private set; } = new DialogParameters { { "Password", password } };
        public void Clear() => Parameters = new DialogParameters();
    }
}
