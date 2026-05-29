using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Xcc.Styles.Behaviors;

public class ThreeStateCheckBoxBehavior : Behavior<CheckBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.Click += OnClick;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject.Click -= OnClick;
    }

    private void OnClick(object sender, RoutedEventArgs e)
    {
        var cb = AssociatedObject;

        if (cb.IsChecked == true)
            cb.IsChecked = null;
        else if (cb.IsChecked == null)
            cb.IsChecked = false;
        else
            cb.IsChecked = true;
    }
}