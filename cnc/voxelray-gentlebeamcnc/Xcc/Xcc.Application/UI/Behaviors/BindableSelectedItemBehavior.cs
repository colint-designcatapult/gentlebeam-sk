using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Xcc.Application.UI.Behaviors
{
    public class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        private static object? _lastClickedItem; 
        private static DateTime _lastClickTime = DateTime.MinValue;
        private static readonly TimeSpan DoubleClickThreshold = TimeSpan.FromMilliseconds(700);

        #region SelectedItem Property
        public object SelectedItem
        {
            get => (object)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                nameof(SelectedItem),
                typeof(object),
                typeof(BindableSelectedItemBehavior),
                new UIPropertyMetadata(null, OnSelectedItemChanged));

        public static readonly DependencyProperty ClickedAgainCommandProperty =
            DependencyProperty.RegisterAttached("ClickedAgainCommand", typeof(ICommand), typeof(BindableSelectedItemBehavior), new PropertyMetadata(null, OnClickedAgainCommandChanged));


        public static void SetClickedAgainCommand(UIElement element, ICommand value)
        {
            element.SetValue(ClickedAgainCommandProperty, value);
        }

        public static ICommand GetClickedAgainCommand(UIElement element)
        {
            return (ICommand)element.GetValue(ClickedAgainCommandProperty);
        }

        private static void OnClickedAgainCommandChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                if (e.NewValue != null)
                {
                    treeView.SelectedItemChanged += TreeView_SelectedItemChanged;
                    treeView.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
                }
                else
                {
                    treeView.SelectedItemChanged -= TreeView_SelectedItemChanged;
                    treeView.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
                }
            }
        }

        private static void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is DependencyObject d)
            {
                OnSelectedItemChanged(d, e);
            }
        }

        private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeView treeView)
            {
                var clickedItem = GetClickedItem(treeView, e.GetPosition(treeView));
                if (clickedItem != null)
                {
                    var currentTime = DateTime.Now;
                    if (clickedItem == _lastClickedItem)
                    {
                        var difference = currentTime - _lastClickTime;
                        if (difference > DoubleClickThreshold)
                        {
                            var command = GetClickedAgainCommand(treeView);
                            if (command != null && command.CanExecute(clickedItem))
                            {
                                command.Execute(clickedItem);
                            }
                        }
                    }
                    _lastClickTime = currentTime;
                }
            }
        }

        private static object? GetClickedItem(TreeView treeView, Point position)
        {
            var hitTestResult = VisualTreeHelper.HitTest(treeView, position);
            if (hitTestResult is not null)
            {
                if (hitTestResult.VisualHit is FrameworkElement frameworkElement)
                {
                    var contentPresenter = frameworkElement.TemplatedParent as ContentPresenter;

                    return contentPresenter?.Content;
                }
            }
            return null;
        }

        private static void OnSelectedItemChanged(DependencyObject sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (sender is TreeView treeView)
            {
                _lastClickedItem = treeView.SelectedItem;
            }
        }

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not BindableSelectedItemBehavior behavior)
                return;

            var treeView = behavior.AssociatedObject;

            if (treeView is null)
                return;

            SetSelected(treeView, e.NewValue);
        }

        private static bool SetSelected(ItemsControl parent, object child)
        {
            if (parent == null || child == null)
                return false;

            if (parent.ItemContainerGenerator.ContainerFromItem(child) is TreeViewItem childNode)
            {
                childNode.Focus();                
                return childNode.IsSelected = true;
            }

            if (parent.Items.Count > 0)
            {
                foreach (object childItem in parent.Items)
                {
                    if (parent.ItemContainerGenerator.ContainerFromItem(childItem) is ItemsControl childControl)
                    {
                        if (SetSelected(childControl, child))
                            return true;
                    }
                }
            }

            return false;
        }
        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }
}
