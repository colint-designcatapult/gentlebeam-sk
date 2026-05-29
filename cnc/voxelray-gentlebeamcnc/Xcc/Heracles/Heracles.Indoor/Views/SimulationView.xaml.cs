using System.Windows;
using System.Windows.Controls;

namespace Heracles.Indoor.Views;

/// <summary>
/// Interaction logic for SimulationView.xaml
/// </summary>
public partial class SimulationView : ContentControl
{
    public SimulationView()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ToolBarVisibilityProperty =
        DependencyProperty.Register(
            nameof(ToolBarVisibility),
            typeof(Visibility),
            typeof(SimulationView),
            new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public Visibility ToolBarVisibility
    {
        get => (Visibility)GetValue(ToolBarVisibilityProperty);
        set => SetValue(ToolBarVisibilityProperty, value);
    }


    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(SimulationView),
            new FrameworkPropertyMetadata(new CornerRadius(8,8,0,0), FrameworkPropertyMetadataOptions.AffectsRender));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
}