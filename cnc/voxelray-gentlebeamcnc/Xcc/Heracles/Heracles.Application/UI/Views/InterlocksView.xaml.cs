using System.Windows;
using System.Windows.Controls;

namespace Heracles.Application.UI.Views;

/// <summary>
/// Interaction logic for InterlocksView
/// </summary>
public partial class InterlocksView : ContentControl
{
    public InterlocksView()
    {
        InitializeComponent();
    }


    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(
            nameof(Orientation),
            typeof(Orientation),
            typeof(InterlocksView),
            new PropertyMetadata(Orientation.Horizontal));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }
}