using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace Xcc.Styles.Behaviors;

public static class BehaviorHelper
{
    public static readonly DependencyProperty EnableThreeStateCheckBoxProperty =
        DependencyProperty.RegisterAttached(
            "EnableThreeStateCheckBox",
            typeof(bool),
            typeof(BehaviorHelper),
            new PropertyMetadata(false, OnEnableChanged));

    public static bool GetEnableThreeStateCheckBox(DependencyObject obj) =>
        (bool)obj.GetValue(EnableThreeStateCheckBoxProperty);

    public static void SetEnableThreeStateCheckBox(DependencyObject obj, bool value) =>
        obj.SetValue(EnableThreeStateCheckBoxProperty, value);

    private static void OnEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CheckBox cb && (bool)e.NewValue)
        {
            var behaviors = Interaction.GetBehaviors(cb);
            if (!behaviors.OfType<ThreeStateCheckBoxBehavior>().Any())
            {
                behaviors.Add(new ThreeStateCheckBoxBehavior());
            }
        }
    }
}