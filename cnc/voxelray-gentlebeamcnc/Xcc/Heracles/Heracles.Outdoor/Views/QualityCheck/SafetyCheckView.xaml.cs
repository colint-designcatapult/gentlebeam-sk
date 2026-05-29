using System.Windows.Controls;
using Xcc.Application.UI.Mvvm;

namespace Heracles.External.Views.QualityCheck
{
    /// <summary>
    /// Interaction logic for SafetyCheckView.xaml
    /// </summary>
    public partial class SafetyCheckView : UserControl
    {
        public SafetyCheckView()
        {
            InitializeComponent();
                        
            Loaded += (s, e) => 
            {
                if (this.IsVisible)
                {
                    (DataContext as ILoadAware)?.VisiblyLoaded();
                }
            };
            Unloaded += (s, e) => (DataContext as ILoadAware)?.Unloaded();
        }
    }
}
