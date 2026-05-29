using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Heracles.Application.UI.Views
{
    /// <summary>
    /// Interaction logic for LoginView
    /// </summary>
    public partial class LoginView : ContentControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void Border_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is Border border)
            {
                var rect = new RectangleGeometry
                {
                    Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight),
                    RadiusX = border.CornerRadius.TopLeft,
                    RadiusY = border.CornerRadius.TopLeft
                };
                border.Clip = rect;
            }
        }
    }
}
