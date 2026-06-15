using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Xcc.Application.UI.Behaviors
{
    public static class CalendarBehaviors
    {
        public static readonly DependencyProperty ReleaseMouseAfterSelection =
            DependencyProperty.RegisterAttached(
                "ReleaseMouseAfterSelection", 
                typeof(bool), 
                typeof(Calendar),
                new FrameworkPropertyMetadata(
                    false,
                    (d, e) =>
                    {
                        if (d is Calendar calendar)
                        {
                            calendar.PreviewMouseUp += (s, e1) =>
                            {
                                if (Mouse.Captured is System.Windows.Controls.Primitives.CalendarItem)
                                {
                                    Mouse.Capture(null);
                                }
                            };
                        }
                    }));

        public static bool GetReleaseMouseAfterSelection(DependencyObject obj)
        {
            return (bool)obj.GetValue(ReleaseMouseAfterSelection);
        }

        public static void SetReleaseMouseAfterSelection(DependencyObject obj, bool value)
        {
            obj.SetValue(ReleaseMouseAfterSelection, value);
        }
    }
}
