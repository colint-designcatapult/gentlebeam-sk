using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.CustomControls
{
    public class MultiSelectableDataGrid : DataGrid
    {
        bool IsSelectionChangedFromHandler = false;

        public static readonly DependencyProperty SelectedItemsListProperty = 
            DependencyProperty.Register(nameof(SelectedItemsList),
            typeof(IList), 
            typeof(MultiSelectableDataGrid), 
            new PropertyMetadata(
                null, 
                (d, e) =>
                {
                    if (d is not DataGrid dataGrid)
                        return;

                    if (e.NewValue is not IList list)
                        return;

                    dataGrid.SelectedItems.Clear();

                    foreach (var item in list)
                    {
                        dataGrid.SelectedItems.Add(item);   
                    }
                }));

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (IsSelectionChangedFromHandler) return;

            //if (SelectedItemsList == null)
            //    SelectedItemsList = new List<object>();

            //SelectedItemsList.Clear();

            //foreach (var item in this.SelectedItems)
            //    SelectedItemsList.Add(item);

            IsSelectionChangedFromHandler = true;
            
            var newSelectedItems = new List<object>();
            foreach (var item in this.SelectedItems)
                newSelectedItems.Add(item);

            SelectedItemsList = newSelectedItems;

            IsSelectionChangedFromHandler = false;
        }
    }
}
