using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xcc.Application.UI.UserControls
{
    /// <summary>
    /// <para>Extends the functionality of WPF DataGrid.</para>
    /// Added capabilities:<br/>
    /// Scrolling into view to SelectedItem.<br/>
    /// </summary>
    public class XccDataGrid : DataGrid
    {
        public XccDataGrid() : base()
        {
            SelectionChanged += (s, e) =>
            {
                if (SelectedItem is null)
                    return;

                ScrollIntoView(SelectedItem);
            };

            this.AddHandler(DataGridCell.GotFocusEvent, new RoutedEventHandler(OnCellGotFocus));
        }

        private void OnCellGotFocus(object sender, RoutedEventArgs e)
        {
            // Lookup for the source to be DataGridCell
            if (e.OriginalSource.GetType() == typeof(DataGridCell))
            {
                // Starts the Edit on the row;
                DataGrid grd = (DataGrid)sender;
                grd.BeginEdit(e);

                if (e.OriginalSource is DataGridCell dataGridCell)
                {
                    Control? control = GetFirstChildByType<Control>(dataGridCell);
                    control?.Focus();
                }
            }
        }

        private T? GetFirstChildByType<T>(DependencyObject prop) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(prop); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild((prop), i) as DependencyObject;
                if (child == null)
                    continue;

                if (child is T childAsT)
                    return childAsT;

                T? subChild = GetFirstChildByType<T>(child);

                if (subChild != null)
                    return subChild;
            }
            return null;
        }
    }
}
