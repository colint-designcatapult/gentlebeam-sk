using System.Windows;
using System.Windows.Controls;

namespace GcbTelemetryPresenter.UI.Behaviors;

public static class ScrollIntoViewBehavior
{
    public static readonly DependencyProperty EnableScrollIntoViewProperty =
        DependencyProperty.RegisterAttached(
            "EnableScrollIntoView",
            typeof(bool),
            typeof(ScrollIntoViewBehavior),
            new PropertyMetadata(false, OnEnableScrollIntoViewChanged));

    public static bool GetEnableScrollIntoView(DependencyObject obj) =>
        (bool)obj.GetValue(EnableScrollIntoViewProperty);

    public static void SetEnableScrollIntoView(DependencyObject obj, bool value) =>
        obj.SetValue(EnableScrollIntoViewProperty, value);

    private static void OnEnableScrollIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListView listView && e.NewValue is bool enabled && enabled)
        {
            listView.SelectionChanged += (s, args) =>
            {
                if (listView.SelectedItem != null)
                {
                    listView.ScrollIntoView(listView.SelectedItem);
                }
            };
        }
    }
}