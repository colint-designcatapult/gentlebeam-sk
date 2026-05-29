using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Xcc.Controls;

public class XccTextBox : TextBox
{
    public XccTextBox()
    {
        SizeChanged += OnSizeChanged;
        GotFocus += OnGotFocus;
        LostFocus += OnLostFocus;

        SetClipboardState();
    }


    static XccTextBox()
    {
        TextProperty.OverrideMetadata(
            typeof(XccTextBox),
            new FrameworkPropertyMetadata(
                TextProperty.DefaultMetadata.DefaultValue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                TextProperty.DefaultMetadata.PropertyChangedCallback,
                TextProperty.DefaultMetadata.CoerceValueCallback,
                isAnimationProhibited: true,
                UpdateSourceTrigger.PropertyChanged)
        );
    }

    /// <summary>
    /// Prevents TextBox's size expanding by text in case of TextBox located in the star-sized content host.
    /// </summary>
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;

        if (textBox.CanUndo && e.NewSize.Width > e.PreviousSize.Width)
        {
            textBox.Width = e.PreviousSize.Width;
        }

        if (textBox.CanUndo && e.NewSize.Height > e.PreviousSize.Height)
        {
            textBox.Height = e.PreviousSize.Height;
        }
    }

    private void OnGotFocus(object sender, RoutedEventArgs e)
    {
        IsGotFocus= true;

        if (XccKeyboardAppearance.UseXccKeyboard)
        {
            XccKeyboardAppearance.Instance.KeyboardVisibility = Visibility.Visible;

            var window = System.Windows.Application.Current.MainWindow;

            if (window is not null)
            {
                var position = this.TranslatePoint(new Point(), window);
                var height2 = this.ActualHeight / 2;

                if (position.Y + height2 < window.ActualHeight / 2)
                    XccKeyboardAppearance.Instance.ShowBottom();
                else
                    XccKeyboardAppearance.Instance.ShowTop();
            }
        }
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (XccKeyboardAppearance.UseXccKeyboard)
        {
            XccKeyboardAppearance.Instance.Hide();
        }
    }


    private ContextMenu? _contextMenu;

    private void SetClipboardState()
    {
        if (IsClipboardDisabled)
        {
            _contextMenu = ContextMenu;
            ContextMenu = null;
            CommandManager.AddPreviewExecutedHandler(this, PreviewExecuted);
        }
        else
        {
            if (_contextMenu is not null)
            {
                ContextMenu = _contextMenu;
                _contextMenu = null;
            }
            CommandManager.RemoveExecutedHandler(this, PreviewExecuted);
        }
    }

    private void PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Copy ||
            e.Command == ApplicationCommands.Cut ||
            e.Command == ApplicationCommands.Paste)
        {
            e.Handled = true;
        }
    }

    /// <summary>
    /// Cause explicit update bindings of the control.
    /// </summary>
    /// <param name="sender"></param>
    private void UpdateBindings(object sender)
    {
        if (sender is TextBox textBox)
        {
            BindingExpression? binding = textBox.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
        }
    }


    public static readonly DependencyProperty IsClipboardDisabledProperty =
        DependencyProperty.Register(
            nameof(IsClipboardDisabled),
            typeof(bool),
            typeof(XccTextBox),
            new PropertyMetadata(
                false,
                (d, e) =>
                {
                    if (d is XccTextBox control)
                    {
                        control.SetClipboardState();
                    }
                }));

    public bool IsClipboardDisabled
    {
        get => (bool)GetValue(IsClipboardDisabledProperty);
        set => SetValue(IsClipboardDisabledProperty, value);
    }


    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(
            nameof(Watermark),
            typeof(string),
            typeof(XccTextBox),
            new PropertyMetadata(string.Empty));

    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }


    public static readonly DependencyProperty TagPositionProperty =
        DependencyProperty.Register(
            nameof(TagPosition),
            typeof(Dock),
            typeof(XccTextBox),
            new PropertyMetadata(Dock.Top));

    public Dock TagPosition
    {
        get => (Dock)GetValue(TagPositionProperty);
        set => SetValue(TagPositionProperty, value);
    }


    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(object),
            typeof(XccTextBox));

    public object Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IsGotFocusProperty =
        DependencyProperty.Register(
            nameof(IsGotFocus),
            typeof(bool),
            typeof(XccTextBox));

    public bool IsGotFocus
    {
        get => (bool)GetValue(IsGotFocusProperty);
        set => SetValue(IsGotFocusProperty, value);
    }
}