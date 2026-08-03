using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.ViewModels;

namespace Heracles.Ucsi.Views;

public partial class UnifiedCalibrationServiceView : System.Windows.Controls.UserControl
{
    private readonly DispatcherTimer _refreshTimer;
    private long _lastGraphSequence = -1;
    private bool _refreshing;
    private CancellationTokenSource? _refreshCancellation;

    public UnifiedCalibrationServiceView()
    {
        InitializeComponent();
        _refreshTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1d / 30d),
        };
        _refreshTimer.Tick += OnRefreshTick;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
        if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
            viewModel.Coordinator.Start();
        _refreshCancellation = new CancellationTokenSource();
        _refreshTimer.Start();
        _ = RefreshAsync();
    }

    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
        _refreshTimer.Stop();
        _refreshCancellation?.Cancel();
        _refreshCancellation?.Dispose();
        _refreshCancellation = null;
    }

    private void OnRefreshTick(object? sender, EventArgs eventArgs) => _ = RefreshAsync();

    private async Task RefreshAsync()
    {
        if (_refreshing || DataContext is not UnifiedCalibrationServiceViewModel viewModel)
            return;
        _refreshing = true;
        try
        {
            CancellationToken cancellationToken = _refreshCancellation?.Token ?? CancellationToken.None;
            await viewModel.TickAsync();
            IReadOnlyList<UcsiTelemetrySample> liveBatch = viewModel.Mode == UcsiMode.Live
                ? viewModel.Coordinator.LiveHistory.GetAfter(_lastGraphSequence)
                : Array.Empty<UcsiTelemetrySample>();
            if (liveBatch.Count > 0)
                _lastGraphSequence = liveBatch[^1].LiveSequence;

            foreach (GraphPaneView graph in FindVisualChildren<GraphPaneView>(this))
                await graph.RefreshAsync(viewModel.Coordinator, liveBatch, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _refreshing = false;
        }
    }

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent)
        where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
                yield return match;
            foreach (T descendant in FindVisualChildren<T>(child))
                yield return descendant;
        }
    }

    private void OnCommandTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.Return && sender is System.Windows.Controls.TextBox textBox)
        {
            BindingExpression binding = textBox.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
            binding?.UpdateSource();
            e.Handled = true;
        }
    }

    private void OnApplyMonitoredParametersClick(object sender, RoutedEventArgs e)
    {
        // Close the popup by toggling the ParameterButton's IsChecked state
        ParameterButton.IsChecked = false;
    }

    private void OnEyeButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string parameterId)
        {
            if (DataContext is UnifiedCalibrationServiceViewModel viewModel)
            {
                viewModel.AddToMonitoredParameters(parameterId);
            }
        }
    }
}
