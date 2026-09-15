using System;
using System.Windows;

namespace Heracles.Indoor.Views.Settings;

public partial class DatabaseRecoveryWindow : Window
{
    private readonly bool _provisioning;
    private string? _enteredKey;

    private DatabaseRecoveryWindow(bool provisioning, string? key, string? error)
    {
        InitializeComponent();
        _provisioning = provisioning;
        Heading.Text = provisioning ? "Save your database recovery key" : "Unlock the existing database";
        Instructions.Text = provisioning
            ? "Keep this key in a secure location outside this computer. It is required to recover your database if automatic unlock becomes unavailable. Anyone with this key and a copy of the database can access its data."
            : "Automatic unlock is unavailable. Enter the recovery key saved when this database was created. Cancelling leaves the database unchanged and closes the application.";
        RecoveryKeyBox.IsReadOnly = provisioning;
        RecoveryKeyBox.Text = key ?? string.Empty;
        ValidationMessage.Text = error ?? string.Empty;
        Acknowledgement.Visibility = provisioning ? Visibility.Visible : Visibility.Collapsed;
        ContinueButton.Content = provisioning ? "Continue" : "Unlock";
        ContinueButton.IsEnabled = !provisioning;
        Closed += (_, _) => RecoveryKeyBox.Clear();
    }

    public static bool ConfirmNewKey(string key) => Show(provisioning: true, key, null).Accepted;
    public static string? RequestKey(string? error) => Show(provisioning: false, null, error).Key;

    private static (bool Accepted, string? Key) Show(bool provisioning, string? key, string? error)
    {
        var application = System.Windows.Application.Current;
        var shutdownMode = application.ShutdownMode;
        var mainWindow = application.MainWindow;
        DatabaseRecoveryWindow? window = null;
        application.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        try
        {
            window = new DatabaseRecoveryWindow(provisioning, key, error);
            bool accepted = window.ShowDialog() == true;
            return (accepted, accepted ? window._enteredKey : null);
        }
        finally
        {
            key = null;
            if (window is not null)
            {
                window.RecoveryKeyBox.Clear();
                window._enteredKey = null;
            }
            application.MainWindow = mainWindow;
            application.ShutdownMode = shutdownMode;
        }
    }

    private void AcknowledgementChanged(object sender, RoutedEventArgs e)
    {
        if (ContinueButton is not null)
            ContinueButton.IsEnabled = !_provisioning || Acknowledgement.IsChecked == true;
    }

    private void ContinueClicked(object sender, RoutedEventArgs e)
    {
        if (_provisioning && Acknowledgement.IsChecked != true) return;
        if (!_provisioning) _enteredKey = RecoveryKeyBox.Text;
        DialogResult = true;
    }

    private void CancelClicked(object sender, RoutedEventArgs e) => DialogResult = false;
}
