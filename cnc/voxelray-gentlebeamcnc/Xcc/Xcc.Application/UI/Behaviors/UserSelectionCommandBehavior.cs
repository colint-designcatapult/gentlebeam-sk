using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Xcc.Application.UI.Behaviors;

// Selection bindings also change during data refresh and navigation. Execute only for
// selections made by a mouse or keyboard input event in this control.
public sealed class UserSelectionCommandBehavior : Behavior<Selector>
{
    public bool UseSelectedIndex { get; set; }

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(UserSelectionCommandBehavior));

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    private bool _userInput;

    protected override void OnAttached()
    {
        AssociatedObject.PreviewMouseDown += OnInput;
        AssociatedObject.PreviewKeyDown += OnInput;
        AssociatedObject.SelectionChanged += OnSelectionChanged;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.PreviewMouseDown -= OnInput;
        AssociatedObject.PreviewKeyDown -= OnInput;
        AssociatedObject.SelectionChanged -= OnSelectionChanged;
    }

    private void OnInput(object sender, InputEventArgs args)
    {
        _userInput = true;
        AssociatedObject.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => _userInput = false));
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
        if (!_userInput || args.OriginalSource != AssociatedObject)
            return;
        _userInput = false;
        object? selected = UseSelectedIndex ? AssociatedObject.SelectedIndex : AssociatedObject.SelectedItem;
        if (Command?.CanExecute(selected) == true)
            Command.Execute(selected);
    }
}
