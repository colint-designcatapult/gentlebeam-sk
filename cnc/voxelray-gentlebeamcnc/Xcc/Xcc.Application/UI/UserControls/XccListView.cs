using System.Windows.Controls;

namespace Xcc.Application.UI.UserControls
{
    /// <summary>
    /// <para>Extends the functionality of WPF ListView.</para>
    /// Added capabilities:<br/>
    /// Scrolling into view to SelectedItem.<br/>
    /// Focused item also becomes selected.
    /// </summary>
    public class XccListView : ListView
    {
        public XccListView() : base()
        {
            SelectionChanged += (s, e) =>
            {
                if (SelectedItem is null)
                    return;

                ScrollIntoView(SelectedItem);
            };

            ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        private void ItemContainerGenerator_StatusChanged(object? sender, System.EventArgs e)
        {
            if (ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (var item in Items)
                {
                    ListViewItem listViewItem = (ListViewItem)ItemContainerGenerator.ContainerFromItem(item);

                    if (listViewItem is not null)
                    {
                        listViewItem.GotFocus += (s, e) => listViewItem.IsSelected = true;
                    }
                }
            }
        }
    }
}
