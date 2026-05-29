using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xcc.Application.UI.Behaviors
{
    public static class EditableComboBoxBehavior
    {
        public static readonly DependencyProperty AcceptsEnterKeyProperty =
            DependencyProperty.RegisterAttached("AcceptsEnterKey", typeof(bool), typeof(EditableComboBoxBehavior), 
                new PropertyMetadata(default(bool), OnAcceptsEnterKey));

        public static void SetAcceptsEnterKey(DependencyObject element, bool value)
        {
            element.SetValue(AcceptsEnterKeyProperty, value);
        }

        public static bool GetAcceptsEnterKey(DependencyObject element)
        {
            return (bool)element.GetValue(AcceptsEnterKeyProperty);
        }

        private static void OnAcceptsEnterKey(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var element = (UIElement)dependencyObject;

            if ((bool)eventArgs.NewValue)
            {
                element.PreviewKeyDown += KeyDownPreview;
                element.KeyUp += KeyDown;
            }
            else
            {
                element.PreviewKeyDown -= KeyDownPreview;
                element.KeyUp -= KeyDown;
            }
        }

        private static void KeyDownPreview(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
            }
        }

        private static void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
            }
            else
            {
                var comboBox = (ComboBox) sender;
                var text = comboBox.Text;
                comboBox.IsDropDownOpen = false;
            }
        }
    }
}
