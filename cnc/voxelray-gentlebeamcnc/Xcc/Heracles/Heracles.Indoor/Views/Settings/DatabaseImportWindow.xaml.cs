using System;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;

namespace Heracles.Indoor.Views.Settings;

public partial class DatabaseImportWindow : Window
{
    private bool _finished;

    private DatabaseImportWindow()
    {
        InitializeComponent();
        Closing += PreventEarlyClose;
    }

    public static Task RunAsync(Func<Task> operation, Func<bool> shutdownStarted)
    {
        var window = new DatabaseImportWindow { Owner = System.Windows.Application.Current.MainWindow };
        Exception? failure = null;
        window.Loaded += async (_, _) =>
        {
            try
            {
                await operation();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                // Keep the shell disabled between the modal surface and the final close notice.
                if (shutdownStarted() && window.Owner is not null)
                    window.Owner.IsEnabled = false;
                window._finished = true;
                window.Close();
            }
        };
        window.ShowDialog();
        if (failure is not null)
            ExceptionDispatchInfo.Capture(failure).Throw();
        return Task.CompletedTask;
    }

    private void PreventEarlyClose(object? sender, CancelEventArgs args) => args.Cancel = !_finished;
}
