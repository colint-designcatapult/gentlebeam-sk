using System.Windows;

namespace Xcc.Application.UI.Behaviors;

public static class Watermark
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(Watermark),
            new FrameworkPropertyMetadata(string.Empty));

    public static void SetText(UIElement element, string value)
    {
        element.SetValue(TextProperty, value);
    }

    public static string GetText(UIElement element)
    {
        return (string)element.GetValue(TextProperty);
    }
}