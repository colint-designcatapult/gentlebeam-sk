using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Xcc.Application.Helpers;

namespace Xcc.Application.UI.UserControls;

/// <summary>
/// Interaction logic for XccKeyboard.xaml
/// </summary>
public partial class XccKeyboard : UserControl
{
    private readonly DispatcherTimer _updateKeyTimer;
    private Brush _buttonStrokeColor;
    private Brush _buttonStrokeDisabledColor;
    private readonly List<Button> _letterButtons = []; // Cached list of letter buttons
    private readonly List<TextBlock> _shiftKeys = [];
    private readonly List<TextBlock> _defaultKeys = [];


    public XccKeyboard()
    {
        InitializeComponent();
        LoadColorResources();

        Loaded += (_,_) =>
        {
            CacheLetterButtons();
            IsCapsLockToggled = Keyboard.IsKeyToggled(Key.CapsLock);
            IsAltToggled = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            IsCtrlToggled = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            IsShiftToggled = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        };
            
        _updateKeyTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateKeyTimer.Tick += (_,_) =>
        {
            IsCapsLockToggled = Keyboard.IsKeyToggled(Key.CapsLock);
            IsAltToggled = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
            IsCtrlToggled = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            IsShiftToggled = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        };
        _updateKeyTimer.Start();

        System.Windows.Application.Current.Exit += (_, _) =>
        {
            if (IsCtrlToggled)
            {
                IsCtrlToggled = false;
                SimulateInput.SendKeyUp(Key.RightCtrl);
                SimulateInput.SendKeyUp(Key.LeftCtrl);
            }

            if (IsAltToggled)
            {
                IsAltToggled = false;
                SimulateInput.SendKeyUp(Key.RightAlt);
                SimulateInput.SendKeyUp(Key.LeftAlt);
            }

            if (IsShiftToggled)
            {
                IsShiftToggled = false;
                SimulateInput.SendKeyUp(Key.RightShift);
                SimulateInput.SendKeyUp(Key.LeftShift);
            }
        };
    }


    [MemberNotNull(nameof(_buttonStrokeColor), nameof(_buttonStrokeDisabledColor))]
    private void LoadColorResources()
    {
        const string buttonStrokeColor = "ButtonStrokeColor";
        const string buttonStrokeDisabledColor = "ButtonStrokeDisabledColor";

        ResourceDictionary colorResources = new()
        {
            Source = new Uri("pack://application:,,,/Xcc.Application;Component/UI/Resources/ColorResources.xaml", UriKind.RelativeOrAbsolute)
        };

        _buttonStrokeColor = (Brush)(colorResources[buttonStrokeColor] ?? throw new Exception($"Required resource is missing. Resource key {_buttonStrokeColor}"));
        _buttonStrokeDisabledColor = (Brush)(colorResources[buttonStrokeDisabledColor] ?? throw new Exception($"Required resource is missing. Resource key {_buttonStrokeDisabledColor}"));
    }


    private void CacheLetterButtons()
    {
        _letterButtons.Clear();
        _shiftKeys.Clear();
        _defaultKeys.Clear();

        foreach (Button button in this.GetChildsOfType<Button>())
        {
            switch (button.Content)
            {
                case string text when LatinLetterRegex().IsMatch(text):
                    _letterButtons.Add(button);
                    break;

                case Grid grid:
                    foreach (TextBlock textBlock in grid.Children.OfType<TextBlock>())
                    {
                        var tag = textBlock.Tag?.ToString();

                        if (string.Equals(tag, "Shift", StringComparison.OrdinalIgnoreCase))
                            _shiftKeys.Add(textBlock);
                        else if (string.Equals(tag, "Default", StringComparison.OrdinalIgnoreCase))
                            _defaultKeys.Add(textBlock);
                    }
                    break;
            }
        }
    }

    public void UpdateLetters()
    {
        foreach (var button in _letterButtons)
        {
            button.Content = IsShiftToggled || IsCapsLockToggled ? button.Content?.ToString()?.ToUpperInvariant() : button.Content?.ToString()?.ToLowerInvariant();
        }
    }

    public void UpdateShiftable()
    {
        if (IsShiftToggled)
        {
            foreach (var textBox in _shiftKeys)
                textBox.Foreground = _buttonStrokeColor;
            foreach (var textBox in _defaultKeys)
                textBox.Foreground = _buttonStrokeDisabledColor;
        }
        else
        {
            foreach (var textBox in _shiftKeys)
                textBox.Foreground = _buttonStrokeDisabledColor;
            foreach (var textBox in _defaultKeys)
                textBox.Foreground = _buttonStrokeColor;
        }
    }

    private void UpdateCapsLockIndicator()
    {
        CapsLockIndicator.Fill = IsCapsLockToggled ? Brushes.Orange : Brushes.Gray;
        CapsLockIndicatorEffect.Opacity = IsCapsLockToggled ? 80 : 0;
    }

    private void UpdateShiftIndicator()
    {
        RShiftIndicator.Fill = IsShiftToggled ? Brushes.Orange : Brushes.Gray;
        RShiftIndicatorEffect.Opacity = IsShiftToggled ? 80 : 0;

        LShiftIndicator.Fill = IsShiftToggled ? Brushes.Orange : Brushes.Gray;
        LShiftIndicatorEffect.Opacity = IsShiftToggled ? 80 : 0;
    }

    private void UpdateAltIndicator()
    {
        RAltIndicator.Fill = IsAltToggled ? Brushes.Orange : Brushes.Gray;
        RAltIndicatorEffect.Opacity = IsAltToggled ? 80 : 0;

        LAltIndicator.Fill = IsAltToggled ? Brushes.Orange : Brushes.Gray;
        LAltIndicatorEffect.Opacity = IsAltToggled ? 80 : 0;
    }

    private void UpdateCtrlIndicator()
    {
        RCtrlIndicator.Fill = IsCtrlToggled ? Brushes.Orange : Brushes.Gray;
        RCtrlIndicatorEffect.Opacity = IsCtrlToggled ? 80 : 0;

        LCtrlIndicator.Fill = IsCtrlToggled ? Brushes.Orange : Brushes.Gray;
        LCtrlIndicatorEffect.Opacity = IsCtrlToggled ? 80 : 0;
    }


    private bool IsSingleShiftToggled { get; set; }

    private void KeyButtonMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button { Tag: Key key })
        {
            SimulateInput.SendKeyDown(key);
        }

        if (IsSingleShiftToggled)
        {
            IsSingleShiftToggled = false;
            SimulateInput.SendKeyUp(Key.LeftShift);
            SimulateInput.SendKeyUp(Key.RightShift);
        }

        if (IsCtrlToggled)
        {
            IsCtrlToggled = false;
            SimulateInput.SendKeyUp(Key.RightCtrl);
            SimulateInput.SendKeyUp(Key.LeftCtrl);
        }

        if (IsAltToggled)
        {
            IsAltToggled = false;
            SimulateInput.SendKeyUp(Key.RightAlt);
            SimulateInput.SendKeyUp(Key.LeftAlt);
        }

    }

    private void KeyButtonMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Button { Tag: Key key })
        {
            SimulateInput.SendKeyUp(key);
        }
    }

    private void LShiftButtonClick(object sender, RoutedEventArgs e)
    {
        if (IsShiftToggled)
        {
            SimulateInput.SendKeyUp(Key.LeftShift);
            SimulateInput.SendKeyUp(Key.RightShift);
        }
        else
        {
            SimulateInput.SendKeyDown(Key.LeftShift);
            SimulateInput.SendKeyDown(Key.RightShift);
        }
    }

    private void RShiftButtonClick(object sender, RoutedEventArgs e)
    {
        if (IsShiftToggled)
        {
            SimulateInput.SendKeyUp(Key.LeftShift);
            SimulateInput.SendKeyUp(Key.RightShift);
        }
        else
        {
            IsSingleShiftToggled = true;
            SimulateInput.SendKeyDown(Key.LeftShift);
            SimulateInput.SendKeyDown(Key.RightShift);
        }
    }

    private void CtrlButtonClick(object sender, RoutedEventArgs e)
    {
        if (IsCtrlToggled)
        {
            SimulateInput.SendKeyUp(Key.LeftCtrl);
            SimulateInput.SendKeyUp(Key.RightCtrl);
        }
        else
        {
            SimulateInput.SendKeyDown(Key.LeftCtrl);
            SimulateInput.SendKeyDown(Key.RightCtrl);
        }
    }

    private void AltButtonClick(object sender, RoutedEventArgs e)
    {
        if (IsAltToggled)
        {
            SimulateInput.SendKeyUp(Key.LeftAlt);
            SimulateInput.SendKeyUp(Key.RightAlt);
        }
        else
        {
            SimulateInput.SendKeyDown(Key.LeftAlt);
            SimulateInput.SendKeyDown(Key.RightAlt);
        }
    }

    private void SelectClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.A);
    }

    private void CutClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.A);
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.X);
    }

    private void CopyClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.A);
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.C);
    }

    private void ClearClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.A);
        SimulateInput.SendKeyPress(Key.Delete);//
    }

    private void PasteClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.V);
    }

    private void UndoClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.Z);
    }

    private void RedoClick(object sender, RoutedEventArgs e)
    {
        SimulateInput.SendKeyPress(Key.LeftCtrl, Key.Y);
    }

    private void DockTopButtonClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.ShowTop();
    }

    private void DockBottomButtonClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.ShowBottom();
    }

    private void DockTopLeftButtonClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.ShowTopLeft();
    }

    private void DockTopRightButtonClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.ShowTopRight();
    }

    private void DockTopBottomLeftButtonClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.ShowBottomLeft();
    }

    private void DockTopBottomRightButtonClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.ShowBottomRight();
    }

    private void HideKeyboardClick(object sender, RoutedEventArgs e)
    {
        XccKeyboardAppearance.Instance.KeyboardVisibility = Visibility.Hidden;
    }


    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(
            nameof(CornerRadius),
            typeof(CornerRadius),
            typeof(XccKeyboard),
            new PropertyMetadata(new CornerRadius(8)));


    public new Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public new static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(XccKeyboard));


    public bool IsShiftToggled
    {
        get => (bool)GetValue(IsShiftToggledProperty);
        set => SetValue(IsShiftToggledProperty, value);
    }

    public static readonly DependencyProperty IsShiftToggledProperty =
        DependencyProperty.Register(
            nameof(IsShiftToggled),
            typeof(bool),
            typeof(XccKeyboard),
            new PropertyMetadata(false, OnShiftToggledChanged));

    private static void OnShiftToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is XccKeyboard xccKeyboard)
        {
            xccKeyboard.UpdateShiftIndicator();
            xccKeyboard.UpdateLetters();
            xccKeyboard.UpdateShiftable();
        }
    }


    public bool IsCapsLockToggled
    {
        get => (bool)GetValue(IsCapsLockToggledProperty);
        set => SetValue(IsCapsLockToggledProperty, value);
    }

    public static readonly DependencyProperty IsCapsLockToggledProperty =
        DependencyProperty.Register(
            nameof(IsCapsLockToggled),
            typeof(bool),
            typeof(XccKeyboard),
            new PropertyMetadata(false, OnCapsLockToggledChanged));

    private static void OnCapsLockToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is XccKeyboard xccKeyboard)
        {
            xccKeyboard.UpdateCapsLockIndicator();
            xccKeyboard.UpdateLetters();
        }
    }


    public bool IsAltToggled
    {
        get => (bool)GetValue(IsAltToggledProperty);
        set => SetValue(IsAltToggledProperty, value);
    }

    public static readonly DependencyProperty IsAltToggledProperty =
        DependencyProperty.Register(
            nameof(IsAltToggled),
            typeof(bool),
            typeof(XccKeyboard),
            new PropertyMetadata(false, OnAltToggledChanged));

    private static void OnAltToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is XccKeyboard xccKeyboard)
        {
            xccKeyboard.UpdateAltIndicator();
        }
    }


    public bool IsCtrlToggled
    {
        get => (bool)GetValue(IsCtrlToggledProperty);
        set => SetValue(IsCtrlToggledProperty, value);
    }

    public static readonly DependencyProperty IsCtrlToggledProperty =
        DependencyProperty.Register(
            nameof(IsCtrlToggled),
            typeof(bool),
            typeof(XccKeyboard),
            new PropertyMetadata(false, OnCtrlToggledChanged));

    private static void OnCtrlToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is XccKeyboard xccKeyboard)
        {
            xccKeyboard.UpdateCtrlIndicator();
        }
    }


    public Visibility DockButtonsVisibility
    {
        get => (Visibility)GetValue(DockButtonsVisibilityProperty);
        set => SetValue(DockButtonsVisibilityProperty, value);
    }

    public static readonly DependencyProperty DockButtonsVisibilityProperty =
        DependencyProperty.Register(
            nameof(DockButtonsVisibility),
            typeof(Visibility),
            typeof(XccKeyboard));


    [GeneratedRegex("^[a-zA-Z]$")]
    private static partial Regex LatinLetterRegex();
}