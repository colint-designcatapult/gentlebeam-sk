using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Xcc.Controls;

public class XccKeyboardAppearance : INotifyPropertyChanged
{
    public static double InnerCornerRadius = 4;
    public static double DefaultCornerRadius = 5;

    public static XccKeyboardAppearance Instance { get; } = new();

    private XccKeyboardAppearance() { }

    /// <summary>
    /// Sets, if getting focus by editable fields will trigger virtual keyboard appearance
    /// </summary>
    public static bool UseXccKeyboard { set; get; } = false;


    public event EventHandler<Visibility>? KeyboardVisibilityChanged;

    private Visibility _keyboardVisibility = Visibility.Hidden;
    public Visibility KeyboardVisibility
    {
        get => _keyboardVisibility;
        set
        {
            if (value.Equals(_keyboardVisibility)) 
                return;

            _keyboardVisibility = value;

            KeyboardVisibilityChanged?.Invoke(this, value);
            OnPropertyChanged();
        }
    }

    private CornerRadius _keyboardCornerRadius = new(5,5,0,0);
    public CornerRadius KeyboardCornerRadius
    {
        get => _keyboardCornerRadius;
        set
        {
            if (value.Equals(_keyboardCornerRadius)) 
                return;

            _keyboardCornerRadius = value;
            OnPropertyChanged();
        }
    }

    private Dock _keyboardDock = Dock.Bottom;
    public Dock KeyboardDock
    {
        get => _keyboardDock;
        set
        {
            if (value == _keyboardDock) 
                return;

            _keyboardDock = value;
            OnPropertyChanged();
        }
    }

    private HorizontalAlignment _keyboardHorizontalAlignment = HorizontalAlignment.Center;
    public HorizontalAlignment KeyboardHorizontalAlignment
    {
        get => _keyboardHorizontalAlignment;
        set
        {
            if (value == _keyboardHorizontalAlignment)
                return;

            _keyboardHorizontalAlignment = value;
            OnPropertyChanged();
        }
    }

    private VerticalAlignment _keyboardVerticalAlignment = VerticalAlignment.Bottom;
    public VerticalAlignment KeyboardVerticalAlignment
    {
        get => _keyboardVerticalAlignment;
        set
        {
            if (value == _keyboardVerticalAlignment)
                return;

            _keyboardVerticalAlignment = value;
            OnPropertyChanged();
        }
    }

    public void ShowTop()
    {
        KeyboardVisibility = Visibility.Visible;
        KeyboardCornerRadius = new CornerRadius(0, 0, DefaultCornerRadius, DefaultCornerRadius);
        KeyboardVerticalAlignment = VerticalAlignment.Top;
        KeyboardHorizontalAlignment= HorizontalAlignment.Center;
    }


    public void ShowBottom()
    {
        KeyboardVisibility = Visibility.Visible;
        KeyboardCornerRadius = new CornerRadius(DefaultCornerRadius, DefaultCornerRadius, 0, 0);
        KeyboardVerticalAlignment = VerticalAlignment.Bottom;
        KeyboardHorizontalAlignment = HorizontalAlignment.Center;
    }

    public void ShowTopLeft()
    {
        KeyboardVisibility = Visibility.Visible;
        KeyboardCornerRadius = new CornerRadius(0, 0, DefaultCornerRadius, 0);
        KeyboardVerticalAlignment = VerticalAlignment.Top;
        KeyboardHorizontalAlignment = HorizontalAlignment.Left;
    }

    public void ShowBottomLeft()
    {
        KeyboardVisibility = Visibility.Visible;
        KeyboardCornerRadius = new CornerRadius(0, DefaultCornerRadius, 0, 0);
        KeyboardVerticalAlignment = VerticalAlignment.Bottom;
        KeyboardHorizontalAlignment = HorizontalAlignment.Left;
    }

    public void ShowTopRight()
    {
        KeyboardVisibility = Visibility.Visible;
        KeyboardCornerRadius = new CornerRadius(0, 0, 0, DefaultCornerRadius);
        KeyboardVerticalAlignment = VerticalAlignment.Top;
        KeyboardHorizontalAlignment = HorizontalAlignment.Right;
    }

    public void ShowBottomRight()
    {
        KeyboardVisibility = Visibility.Visible;
        KeyboardCornerRadius = new CornerRadius(DefaultCornerRadius, 0, 0, 0);
        KeyboardVerticalAlignment = VerticalAlignment.Bottom;
        KeyboardHorizontalAlignment = HorizontalAlignment.Right;
    }

    public void Hide()
    {
        KeyboardVisibility = (KeyboardVisibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible;
    }


    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion INotifyPropertyChanged
}