using System;
using System.Windows;
using System.Windows.Controls;
using Heracles.Indoor.ViewModels.Settings;

namespace Heracles.Indoor.Views.Settings;

public partial class DatabasePasswordView : ContentControl
{
    private DatabasePasswordViewModel? _viewModel;
    private bool _clearing;

    public DatabasePasswordView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs args)
    {
        _viewModel = DataContext as DatabasePasswordViewModel;
        if (_viewModel is not null)
            _viewModel.ClearPasswordControls += ClearControls;
        PasswordInput.Focus();
    }

    private void PasswordChanged(object sender, RoutedEventArgs args)
    {
        if (!_clearing && DataContext is DatabasePasswordViewModel viewModel && ConfirmationInput is not null)
            viewModel.SetPasswords(PasswordInput.Password, ConfirmationInput.Password);
    }

    private void ClearControls(object? sender, EventArgs args)
    {
        _clearing = true;
        try
        {
            PasswordInput.Clear();
            ConfirmationInput.Clear();
        }
        finally { _clearing = false; }
    }

    private void OnUnloaded(object sender, RoutedEventArgs args)
    {
        if (_viewModel is not null)
        {
            _viewModel.ClearSecrets();
            _viewModel.ClearPasswordControls -= ClearControls;
            _viewModel = null;
        }
        ClearControls(this, EventArgs.Empty);
    }
}
