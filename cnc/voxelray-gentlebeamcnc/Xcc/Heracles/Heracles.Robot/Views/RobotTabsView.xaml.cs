using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace Heracles.Robot.Views
{
    /// <summary>
    /// Interaction logic for RobotTabsView.xaml
    /// </summary>
    public partial class RobotTabsView : ContentControl
    {
        public RobotTabsView()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)RobotLog.Items).CollectionChanged += RobotView_CollectionChanged;
        }

        private void RobotView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (VisualTreeHelper.GetChildrenCount(RobotLog) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(RobotLog, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }
    }
}
