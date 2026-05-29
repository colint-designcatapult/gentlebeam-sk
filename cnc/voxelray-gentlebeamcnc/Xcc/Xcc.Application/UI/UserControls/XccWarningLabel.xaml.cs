using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    /// <summary>
    /// Interaction logic for XccWarningLabel.xaml
    /// </summary>
    public partial class XccWarningLabel : UserControl
    {
        public XccWarningLabel()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(XccWarningLabel));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
