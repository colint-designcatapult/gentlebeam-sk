using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.Behaviors
{
    public class BindableMultipleSelectionBehaviour : Behavior<ListBox>
    {
        #region SelectedItem Property
        bool IsSelectionChangedFromHandler { get; set; } = false;

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(BindableMultipleSelectionBehaviour),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (obj, e) =>
                    {
                        if (obj is not ListBox listBox)
                            return;

                        listBox.SelectedItems.Clear();

                        if (e.NewValue is not IList list)
                            return;

                        foreach (var item in list)
                        {
                            listBox.SelectedItems.Add(item);
                        }
                    }));

        public static IList? GetSelectedItems(DependencyObject obj) => (IList?)obj.GetValue(SelectedItemsProperty);

        public static void SetSelectedItems(DependencyObject obj, IList? value) => obj.SetValue(SelectedItemsProperty, value);
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            this.AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsSelectionChangedFromHandler) 
                return;

            IsSelectionChangedFromHandler = true;

            if (this.AssociatedObject.SelectedItems.Count == 0)
            {
                SetSelectedItems(this.AssociatedObject, null);
            }
            else
            {
                var selectedItems = new List<object>();

                foreach (var item in this.AssociatedObject.SelectedItems)
                    selectedItems.Add(item);

                SetSelectedItems(this.AssociatedObject, selectedItems);
            }

            IsSelectionChangedFromHandler = false;
        }
    }
}
