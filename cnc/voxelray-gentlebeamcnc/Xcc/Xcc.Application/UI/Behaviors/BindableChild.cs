using System;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Application.UI.Behaviors
{
    public static class BindableChild
    {
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.RegisterAttached(
                "Child",
                typeof(UIElement),
                typeof(BindableChild),
                new PropertyMetadata(null, OnChildChanged));

        public static void SetChild(DependencyObject element, UIElement value)
        {
            element.SetValue(ChildProperty, value);
        }

        public static UIElement GetChild(DependencyObject element)
        {
            return (UIElement)element.GetValue(ChildProperty);
        }

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Decorator decorator)
            {
                decorator.Child = e.NewValue as UIElement;
            }
            else
            {
                throw new InvalidOperationException("Child can only be applied to controls that derive from Decorator.");
            }
        }
    }
}