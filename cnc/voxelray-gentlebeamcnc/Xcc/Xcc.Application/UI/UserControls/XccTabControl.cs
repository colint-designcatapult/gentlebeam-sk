using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xcc.Application.UI.UserControls
{
    public class XccTabItem : TabItem
    {
        /// <summary>
        /// Specifies that the control should prevent tab from changing.
        /// </summary>
        public bool PreventTabChange { get => (bool)GetValue(PreventTabChangeProperty); set => SetValue(PreventTabChangeProperty, value); }

        public static readonly DependencyProperty
            PreventTabChangeProperty =
            DependencyProperty.Register(
                nameof(PreventTabChange),
                typeof(bool),
                typeof(XccTabItem));

        public void SetPreventTabChangeCurrent(bool value)
        {
            SetCurrentValue(PreventTabChangeProperty, value);
        }   
    }

    /// <summary>
    /// <para>Extends the functionality of WPF TabControl.</para>
    /// Added capabilities:<br/>
    /// Preventing currently selected tab from changing.
    /// </summary>
    public class XccTabControl : TabControl
    {
        public XccTabControl()
        {
            Loaded += (_,_) =>
            {
                // There's misbehaviour in case of regular tabs, where IsVisible isn't set to the first one,
                // so we only do this if tab selection is explicitely required
                if (SelectFirstVisibleItem)
                {
                    if (SelectedItem is null or TabItem { IsVisible: false })
                        SetCurrentValue(SelectedItemProperty, GetFirstVisibleItem());
                }
            };
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            // There's misbehaviour in case of regular tabs, where IsVisible isn't set to the first one,
            // so we only do this if tab selection is explicitely required
            if (SelectFirstVisibleItem)
            {
                if (element is TabItem tabItem)
                {
                    tabItem.IsVisibleChanged -= Container_IsVisibleChanged;
                    tabItem.IsVisibleChanged += Container_IsVisibleChanged;
                }
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            // There's misbehaviour in case of regular tabs, where IsVisible isn't set to the first one,
            // so we only do this if tab selection is explicitely required
            if (SelectFirstVisibleItem)
            {
                if (element is TabItem tabItem)
                    tabItem.IsVisibleChanged -= Container_IsVisibleChanged;
            }
        }

        private void Container_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TabItem { IsVisible: false, IsSelected: true } || SelectedItem is null && IsLoaded)
            {
                SetCurrentValue(SelectedItemProperty, GetFirstVisibleItem());
            }
        }

        private TabItem? GetFirstVisibleItem()
        {
            foreach (var item in Items)
            {
                if (item is TabItem { IsVisible: true } container)
                {
                    return container;
                }
            }
            return null;
        }

        /// <summary>
        /// Needed to prevent infinite changing of tab selection.
        /// </summary>
        bool _isChanging = false;

        /// <summary>
        /// Stores the tab the user originally wanted to go to.
        /// </summary>
        object _desiredTabItem = null!;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_isChanging)
            {
                Mouse.Capture(null); // to prevent getting stuck in a state of infinitely changing the selected tab 
                _isChanging = false;
                if (SelectedItem is XccTabItem tabItem)
                {
                    if (tabItem.IsVisible)
                    {
                        // Apply this preveintion to visible tabs only
                        TabChangePreventedCommand?.Execute(Tuple.Create(SelectedItem, _desiredTabItem));
                    }
                    else
                    {
                        // We don't want triggering this error/prevention in hidden tabs,
                        // so we just silently suppress this tab change prevention and switch to the desired one
                        tabItem.SetPreventTabChangeCurrent(false);
                        SelectedIndex = Items.IndexOf(_desiredTabItem);
                        tabItem.SetPreventTabChangeCurrent(true);
                    }
                }
                return; // we need to stop recursion here in any case
            }

            if ((e.RemovedItems.Count > 0 && e.RemovedItems[0] is XccTabItem item && item.PreventTabChange) || PreventTabChange)
            {
                _isChanging = true;

                foreach (var removedItem in e.RemovedItems)
                {
                    _desiredTabItem = SelectedItem;
                    SelectedIndex = Items.IndexOf(removedItem); // process the first item and exit the loop
                    break;
                }

                _isChanging = false;
                return;
            }
            
            base.OnSelectionChanged(e);
        }

        /// <summary>
        /// Specifies that the control should prevent selected tab from changing.
        /// </summary>
        public bool PreventTabChange { get => (bool)GetValue(PreventTabChangeProperty); set => SetValue(PreventTabChangeProperty, value); }

        public static readonly DependencyProperty
            PreventTabChangeProperty =
            DependencyProperty.Register(
                nameof(PreventTabChange),
                typeof(bool),
                typeof(XccTabControl));

        /// <summary>
        /// Command executes when the tab changing is prevented.
        /// </summary>
        public ICommand TabChangePreventedCommand { get => (ICommand)GetValue(TabChangePreventedCommandProperty); set => SetValue(TabChangePreventedCommandProperty, value); }

        public static readonly DependencyProperty
            TabChangePreventedCommandProperty =
            DependencyProperty.Register(
                nameof(TabChangePreventedCommand),
                typeof(ICommand),
                typeof(XccTabControl));


        /// <summary>
        /// Corner radius of the border containing the tab item content.
        /// </summary>
        public CornerRadius TabCornerRadius { get => (CornerRadius)GetValue(TabCornerRadiusProperty); set => SetValue(TabCornerRadiusProperty, value); }

        public static readonly DependencyProperty
            TabCornerRadiusProperty =
                DependencyProperty.Register(
                    nameof(TabCornerRadius),
                    typeof(CornerRadius),
                    typeof(XccTabControl));


        /// <summary>
        /// If there's need to select first visible item when current item changes its visibility
        /// </summary>
        public bool SelectFirstVisibleItem { get => (bool)GetValue(SelectFirstVisibleItemProperty); set => SetValue(SelectFirstVisibleItemProperty, value); }

        public static readonly DependencyProperty
            SelectFirstVisibleItemProperty =
            DependencyProperty.Register(
                nameof(SelectFirstVisibleItem),
                typeof(bool),
                typeof(XccTabControl));
    }
}

