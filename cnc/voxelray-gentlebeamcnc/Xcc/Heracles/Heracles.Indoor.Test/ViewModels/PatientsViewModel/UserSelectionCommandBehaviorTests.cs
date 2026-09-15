using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Xaml.Behaviors;
using Prism.Commands;
using Xcc.Application.UI.Behaviors;

namespace Heracles.Indoor.Test.ViewModels.PatientsViewModel;

[Apartment(ApartmentState.STA)]
internal sealed class UserSelectionCommandBehaviorTests
{
    [Test]
    public void MouseSelection_ReportsUserTransitionsAndAllowsRevisitButNotProgrammaticSelection()
    {
        var observed = new List<object>();
        var first = new ListBoxItem { Content = "First" };
        var second = new ListBoxItem { Content = "Second" };
        var list = new ListBox { Items = { first, second } };
        Interaction.GetBehaviors(list).Add(new UserSelectionCommandBehavior
        {
            Command = new DelegateCommand<object>(observed.Add)
        });
        var window = new Window { Content = list, Width = 200, Height = 200, ShowInTaskbar = false, ShowActivated = false };
        try
        {
            window.Show();
            window.UpdateLayout();
            list.SelectedItem = second;
            Assert.That(observed, Is.Empty);

            Click(first);
            Assert.That(list.SelectedItem, Is.SameAs(first));
            Assert.That(observed, Is.EqualTo(new[] { first }));
            Click(second);
            Click(first);
            Assert.That(observed, Is.EqualTo(new[] { first, second, first }));

            list.Dispatcher.Invoke(() => { }, DispatcherPriority.Background);
            list.SelectedItem = second;
            Assert.That(observed, Has.Count.EqualTo(3));
        }
        finally { window.Close(); }
    }

    [Test]
    public void KeyboardSelection_ReportsExactlyOneCommandForTheSelectedRecord()
    {
        var observed = new List<object>();
        var first = new ListBoxItem { Content = "First" };
        var second = new ListBoxItem { Content = "Second" };
        var list = new ListBox { Items = { first, second }, SelectedIndex = 0 };
        Interaction.GetBehaviors(list).Add(new UserSelectionCommandBehavior
        {
            Command = new DelegateCommand<object>(observed.Add)
        });
        var window = new Window { Content = list, Width = 200, Height = 200, ShowInTaskbar = false };
        try
        {
            window.Show();
            window.UpdateLayout();
            first.Focus();
            var source = PresentationSource.FromVisual(first)!;
            first.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Down)
            {
                RoutedEvent = Keyboard.PreviewKeyDownEvent
            });
            first.RaiseEvent(new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, Key.Down)
            {
                RoutedEvent = Keyboard.KeyDownEvent
            });

            Assert.That(list.SelectedItem, Is.SameAs(second));
            Assert.That(observed, Is.EqualTo(new[] { second }));
        }
        finally { window.Close(); }
    }

    private static void Click(ListBoxItem item)
    {
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = Mouse.PreviewMouseDownEvent
        });
        item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
        {
            RoutedEvent = Mouse.MouseDownEvent
        });
    }
}
