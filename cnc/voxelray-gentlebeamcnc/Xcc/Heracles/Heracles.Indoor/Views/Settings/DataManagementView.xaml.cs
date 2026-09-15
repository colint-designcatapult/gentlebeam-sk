using System.Windows;
using System.Windows.Controls;
using Heracles.Indoor.ViewModels.Settings;

namespace Heracles.Indoor.Views.Settings;

public partial class DataManagementView : ContentControl
{
    public DataManagementView()
    {
        InitializeComponent();
        Loaded += (_, _) => UpdateVisibility();
        IsVisibleChanged += (_, _) => UpdateVisibility();
        Unloaded += (_, _) => (DataContext as DataManagementViewModel)?.Deactivate();
        DataContextChanged += (_, args) =>
        {
            (args.OldValue as DataManagementViewModel)?.Deactivate();
            UpdateVisibility();
        };
    }

    private void UpdateVisibility()
    {
        if (DataContext is not DataManagementViewModel viewModel) return;
        if (IsLoaded && IsVisible) viewModel.Activate();
        else viewModel.Deactivate();
    }
}
