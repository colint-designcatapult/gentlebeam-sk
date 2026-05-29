using Heracles.Indoor.ViewModels;

using System.Windows.Controls;

namespace Heracles.Indoor.Views;

/// <summary>
/// Interaction logic for ImagingView.xaml
/// </summary>
public partial class ImagingView : ContentControl
{
    private ImagingViewModel ViewModel { get; }

    public ImagingView()
    {
        InitializeComponent();
        ViewModel = (ImagingViewModel)DataContext;

        Loaded += (_, _) => ViewModel.LoadDeepColorApp(HostPanel);
    }
}