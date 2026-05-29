using System.Windows;
using System.Windows.Controls;

namespace Heracles.Indoor.Views
{
    /// <summary>
    /// Interaction logic for PrescriptionView.xaml
    /// </summary>
    public partial class PrescriptionView : UserControl
    {
        public PrescriptionView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ToolBarVisibilityProperty =
            DependencyProperty.Register(
                nameof(ToolBarVisibility),
                typeof(Visibility),
                typeof(PrescriptionView),
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
                typeof(PrescriptionView),
                new FrameworkPropertyMetadata(new CornerRadius(8, 8, 0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
    }
}
