using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xcc.Application.UI.UserControls
{
    public partial class SpinnerControl : UserControl
    {
        public SpinnerControl()
        {
            InitializeComponent();
        }

        #region Dependency properties
        public Brush Color { get => (Brush)GetValue(ColorProperty); set => SetValue(ColorProperty, value);}

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), 
                typeof(Brush), 
                typeof(SpinnerControl), 
                new PropertyMetadata(Brushes.Cyan));

        public int MillisecondsToSpin { get => (int)GetValue(MillisecondsToSpinProperty); set => SetValue(MillisecondsToSpinProperty, value); }

        public static readonly DependencyProperty MillisecondsToSpinProperty =
            DependencyProperty.Register(
                nameof(MillisecondsToSpin), 
                typeof(int), 
                typeof(SpinnerControl), 
                new PropertyMetadata(1300));

        #endregion Dependency properties
    }
}
