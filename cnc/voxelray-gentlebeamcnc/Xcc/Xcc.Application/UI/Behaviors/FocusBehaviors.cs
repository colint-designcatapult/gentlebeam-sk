using System.Windows;
using System.Windows.Input;

namespace Xcc.Application.UI.Behaviors
{
    public class FocusBehaviors
    {
        public static readonly DependencyProperty FocusOnLoad = DependencyProperty.RegisterAttached(
            "FocusOnLoad",
            typeof(bool),
            typeof(FocusManager),
            new PropertyMetadata(
                false, 
                new PropertyChangedCallback(OnValueChanged)));

        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not FrameworkElement control)
                return;

            if ((bool)e.NewValue == false)
                return;

            control.Loaded += (s, e) =>
            {
                if (control.IsVisible)
                    control.Focus();
            };
        }

        public static bool GetFocusOnLoad(DependencyObject d) => (bool)d.GetValue(FocusOnLoad);

        public static void SetFocusOnLoad(DependencyObject d, bool value) => d.SetValue(FocusOnLoad, value);
    }
}
