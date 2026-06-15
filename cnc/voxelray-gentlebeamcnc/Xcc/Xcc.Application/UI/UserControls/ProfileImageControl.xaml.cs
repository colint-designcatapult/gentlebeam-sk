using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    /// <summary>
    /// Interaction logic for ProfileImageControl.xaml
    /// </summary>
    public partial class ProfileImageControl : UserControl
    {
        public ProfileImageControl()
        {
            InitializeComponent();
        }

        #region Dependency properties
        public static readonly DependencyProperty PathToImageProperty =
            DependencyProperty.Register(
                nameof(PathToImage),
                typeof(string),
                typeof(ProfileImageControl));

        public string PathToImage
        {
            get => (string)GetValue(PathToImageProperty);
            set => SetValue(PathToImageProperty, value);
        }
        #endregion
    }
}
