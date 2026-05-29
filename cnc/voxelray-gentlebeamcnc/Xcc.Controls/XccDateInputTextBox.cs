using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Controls;

public class XccDateInputTextBox : XccTextBox
{
    static XccDateInputTextBox()
    {
        TextBox.TextProperty.OverrideMetadata(typeof(XccDateInputTextBox),
            new FrameworkPropertyMetadata(OnTextPropertyChanged));
    }


    private static void OnTextPropertyChanged (DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not XccDateInputTextBox dateInputTextBox)
            return;

        if (e.NewValue is not string text)
            return;

        if (DateOnly.TryParseExact(text, CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern, out DateOnly date))
            dateInputTextBox.Date = date;
        else
            dateInputTextBox.Date = null;
    }


    public DateOnly? Date { get => (DateOnly?)GetValue(DateProperty); set => SetValue(DateProperty, value); }

    public static readonly DependencyProperty DateProperty =
        DependencyProperty.Register(
            nameof(Date),
            typeof(DateOnly?),
            typeof(XccTextBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnDatePropertyChanged));

    private static void OnDatePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (obj is not XccDateInputTextBox dateInputTextBox)
            return;

        if (e.NewValue is null)
        {
            dateInputTextBox.SetCurrentValue(TextProperty, null);
            return;
        }

        if (e.NewValue is not DateOnly date)
            return;

        dateInputTextBox.SetCurrentValue(TextProperty, date.ToShortDateString());
    }
}