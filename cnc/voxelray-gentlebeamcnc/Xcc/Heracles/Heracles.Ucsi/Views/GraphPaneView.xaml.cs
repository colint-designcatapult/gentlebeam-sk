using System.Windows;
using System.Windows.Input;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.ViewModels;
using ScottPlot;
using ScottPlot.Plottables;

namespace Heracles.Ucsi.Views;

public partial class GraphPaneView : System.Windows.Controls.UserControl
{
    private static readonly ScottPlot.Color[] SeriesColors =
    [
        ScottPlot.Color.FromHex("#48C7D9"),
        ScottPlot.Color.FromHex("#F0B44D"),
        ScottPlot.Color.FromHex("#E56B6F"),
        ScottPlot.Color.FromHex("#81C784"),
        ScottPlot.Color.FromHex("#B39DDB"),
        ScottPlot.Color.FromHex("#64B5F6"),
        ScottPlot.Color.FromHex("#FF8A65"),
        ScottPlot.Color.FromHex("#AED581"),
    ];

    private readonly Dictionary<string, SeriesState> _series = new(StringComparer.Ordinal);
    private readonly List<IYAxis> _additionalAxes = [];
    private GraphPaneViewModel? _viewModel;
    private UcsiMode? _renderedMode;
    private bool _configurationDirty = true;
    private long _lastLiveSequence = -1;
    private VerticalLine? _cursor;
    private const double LiveWindowSeconds = 30;
    private bool _followLive = true;


    public GraphPaneView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ConfigurePlotStyle();
        PlotControl.AddHandler(MouseWheelEvent, new MouseWheelEventHandler(OnPlotMouseWheel), true);
        PlotControl.PreviewMouseDown += OnPlotMouseDown;
        PlotControl.PreviewMouseMove += OnPlotMouseMove;
        PlotControl.PreviewKeyDown += OnPlotKeyDown;

    }

    public async Task RefreshAsync(
        ITelemetrySessionCoordinator coordinator,
        IReadOnlyList<UcsiTelemetrySample> liveBatch,
        CancellationToken cancellationToken = default)
    {
        if (_viewModel is null)
            return;

        UcsiMode mode = coordinator.Mode;
        if (_renderedMode != mode)
        {
            SetFollowLive(mode == UcsiMode.Live);
            FollowButton.IsEnabled = mode == UcsiMode.Live;
        }
        if (_configurationDirty || _renderedMode != mode)
        {
            await RebuildAsync(coordinator, mode, cancellationToken);
            _configurationDirty = false;
            _renderedMode = mode;
        }
        else if (mode == UcsiMode.Live)
        {
            AppendLive(liveBatch);
        }

        if (_cursor is not null)
            _cursor.X = TimeSpan.FromTicks(coordinator.CurrentElapsedTicks).TotalSeconds;
        if (mode == UcsiMode.Live && _followLive)
            ApplyLiveFollow(TimeSpan.FromTicks(coordinator.CurrentElapsedTicks).TotalSeconds);

        PlotControl.Refresh();
    }

    private void OnLoaded(object sender, RoutedEventArgs eventArgs)
    {
        AttachViewModel(DataContext as GraphPaneViewModel);
        DataContextChanged += OnDataContextChanged;
    }

    private void OnUnloaded(object sender, RoutedEventArgs eventArgs)
    {
        DataContextChanged -= OnDataContextChanged;
        AttachViewModel(null);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs eventArgs) =>
        AttachViewModel(eventArgs.NewValue as GraphPaneViewModel);

    private void AttachViewModel(GraphPaneViewModel? viewModel)
    {
        if (ReferenceEquals(_viewModel, viewModel))
            return;
        if (_viewModel is not null)
            _viewModel.SeriesSelectionChanged -= OnSeriesSelectionChanged;
        _viewModel = viewModel;
        if (_viewModel is not null)
            _viewModel.SeriesSelectionChanged += OnSeriesSelectionChanged;
        _configurationDirty = true;
    }

    private void OnSeriesSelectionChanged(object? sender, EventArgs eventArgs) =>
        _configurationDirty = true;
    private void OnFollowClick(object sender, RoutedEventArgs eventArgs)
    {
        SetFollowLive(FollowButton.IsChecked == true);
        if (_followLive)
            ApplyLiveFollow(GetLatestElapsedSeconds());
        PlotControl.Refresh();
    }

    private void OnFitAllClick(object sender, RoutedEventArgs eventArgs)
    {
        SetFollowLive(false);
        PlotControl.Plot.Axes.AutoScale();
        PlotControl.Refresh();
    }

    private void OnFitYClick(object sender, RoutedEventArgs eventArgs)
    {
        foreach (IYAxis axis in _series.Values.Select(state => state.YAxis).Distinct())
            PlotControl.Plot.Axes.AutoScaleY(axis);
        PlotControl.Refresh();
    }

    private void OnZoomInClick(object sender, RoutedEventArgs eventArgs)
    {
        SetFollowLive(false);
        PlotControl.Plot.Axes.ZoomIn(1.2, 1.2);
        PlotControl.Refresh();
    }

    private void OnZoomOutClick(object sender, RoutedEventArgs eventArgs)
    {
        SetFollowLive(false);
        PlotControl.Plot.Axes.ZoomOut(1.2, 1.2);
        PlotControl.Refresh();
    }

    private void OnPlotMouseWheel(object sender, MouseWheelEventArgs eventArgs)
    {
        PauseLiveFollow();
        eventArgs.Handled = true;
    }

    private void OnPlotMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs eventArgs)
    {
        if (eventArgs.ChangedButton is System.Windows.Input.MouseButton.Left or System.Windows.Input.MouseButton.Middle)
            PauseLiveFollow();
    }

    private void OnPlotMouseMove(object sender, System.Windows.Input.MouseEventArgs eventArgs)
    {
        if (eventArgs.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            PauseLiveFollow();
    }

    private void OnPlotKeyDown(object sender, System.Windows.Input.KeyEventArgs eventArgs)
    {
        if (eventArgs.Key is System.Windows.Input.Key.Left
            or System.Windows.Input.Key.Right
            or System.Windows.Input.Key.Up
            or System.Windows.Input.Key.Down
            or System.Windows.Input.Key.Add
            or System.Windows.Input.Key.Subtract
            or System.Windows.Input.Key.OemPlus
            or System.Windows.Input.Key.OemMinus)
        {
            PauseLiveFollow();
        }
    }

    private void PauseLiveFollow()
    {
        if (FollowButton.IsEnabled && _followLive)
            SetFollowLive(false);
    }

    private void SetFollowLive(bool enabled)
    {
        _followLive = enabled;
        FollowButton.IsChecked = enabled;
        FollowButton.Content = enabled ? $"Following {LiveWindowSeconds:F0} s" : "Follow Live";
    }

    private double GetLatestElapsedSeconds()
    {
        if (_lastLiveSequence < 0 || _series.Count == 0)
            return 0;
        return _series.Values
            .SelectMany(state => state.Segments)
            .Where(segment => segment.Data.Coordinates.Count > 0)
            .Select(segment => segment.Data.Coordinates[^1].X)
            .DefaultIfEmpty(0)
            .Max();
    }

    private void ApplyLiveFollow(double latestElapsedSeconds)
    {
        double right = Math.Max(LiveWindowSeconds, latestElapsedSeconds + 0.5);
        PlotControl.Plot.Axes.Bottom.Min = Math.Max(0, right - LiveWindowSeconds);
        PlotControl.Plot.Axes.Bottom.Max = right;
        foreach (IYAxis axis in _series.Values.Select(state => state.YAxis).Distinct())
            PlotControl.Plot.Axes.AutoScaleExpandY(axis);
    }


    private async Task RebuildAsync(
        ITelemetrySessionCoordinator coordinator,
        UcsiMode mode,
        CancellationToken cancellationToken)
    {
        _series.Clear();
        _lastLiveSequence = -1;
        _cursor = null;
        PlotControl.Plot.Clear();
        foreach (IYAxis axis in _additionalAxes)
            PlotControl.Plot.Remove(axis);
        _additionalAxes.Clear();
        ConfigurePlotStyle();
        if (_viewModel is null || _viewModel.SelectedParameterIds.Count == 0)
        {
            PlotControl.Plot.Title("Select one or more telemetry series");
            return;
        }

        var axes = new Dictionary<string, IYAxis>(StringComparer.Ordinal);
        int seriesIndex = 0;
        foreach (string parameterId in _viewModel.SelectedParameterIds)
        {
            TelemetryParameterDescriptor descriptor = FindDescriptor(parameterId);
            if (!axes.TryGetValue(descriptor.AxisKey, out IYAxis? yAxis))
            {
                if (axes.Count == 0)
                {
                    yAxis = PlotControl.Plot.Axes.Left;
                }
                else
                {
                    yAxis = PlotControl.Plot.Axes.AddLeftAxis();
                    _additionalAxes.Add(yAxis);
                }
                yAxis.Label.Text = string.IsNullOrEmpty(descriptor.Unit)
                    ? descriptor.DisplayName
                    : descriptor.Unit;
                axes.Add(descriptor.AxisKey, yAxis);
            }
            _series.Add(parameterId, new SeriesState(
                descriptor,
                yAxis,
                SeriesColors[seriesIndex++ % SeriesColors.Length]));
        }

        if (mode == UcsiMode.Live)
        {
            AppendLive(coordinator.LiveHistory.Snapshot());
        }
        else
        {
            IReadOnlyList<ReplayGraphSeries> replaySeries = await coordinator.ReadReplayGraphSeriesAsync(
                _viewModel.SelectedParameterIds,
                0,
                coordinator.TotalElapsedTicks,
                1_000_000,
                cancellationToken);
            foreach (ReplayGraphSeries replay in replaySeries)
            {
                SeriesState state = _series[replay.ParameterId];
                foreach (TelemetryGraphPoint point in replay.Points)
                    AppendPoint(state, TimeSpan.FromTicks(point.ElapsedTicks).TotalSeconds, point.Value);
            }
        }

        _cursor = PlotControl.Plot.Add.VerticalLine(TimeSpan.FromTicks(coordinator.CurrentElapsedTicks).TotalSeconds);
        _cursor.Color = ScottPlot.Color.FromHex("#E8EEF2");
        _cursor.LinePattern = LinePattern.Dashed;
        _cursor.LineWidth = 1;
        _cursor.EnableAutoscale = false;
        PlotControl.Plot.Axes.Bottom.Label.Text = "Elapsed time (s)";
        PlotControl.Plot.ShowLegend(Alignment.UpperRight);
        PlotControl.Plot.Axes.AutoScale();
    }

    private void AppendLive(IReadOnlyList<UcsiTelemetrySample> samples)
    {
        foreach (UcsiTelemetrySample sample in samples)
        {
            if (sample.LiveSequence <= _lastLiveSequence)
                continue;
            double x = TimeSpan.FromTicks(sample.LiveElapsedTicks).TotalSeconds;
            foreach (SeriesState state in _series.Values)
                AppendPoint(state, x, state.Descriptor.Project(sample, state.Categories));
            _lastLiveSequence = sample.LiveSequence;
        }
        foreach (SeriesState state in _series.Values)
            state.TrimTo(PlotControl.Plot, TelemetryHistoryBuffer.DefaultCapacity);
    }

    private void AppendPoint(SeriesState state, double x, double? value)
    {
        if (!value.HasValue || !double.IsFinite(value.Value))
        {
            state.HasGap = true;
            return;
        }

        if (state.HasGap || state.Segments.Count == 0)
        {
            DataLogger logger = PlotControl.Plot.Add.DataLogger();
            logger.ManageAxisLimits = false;
            logger.Axes = new Axes
            {
                XAxis = PlotControl.Plot.Axes.Bottom,
                YAxis = state.YAxis,
            };
            logger.Color = state.Color;
            logger.LineWidth = 1.5f;
            logger.LegendText = state.Segments.Count == 0
                ? string.IsNullOrEmpty(state.Descriptor.Unit)
                    ? state.Descriptor.DisplayName
                    : $"{state.Descriptor.DisplayName} ({state.Descriptor.Unit})"
                : string.Empty;
            state.Segments.Add(logger);
            state.HasGap = false;
        }

        state.Segments[^1].Add(x, value.Value);
        state.PointCount++;
    }

    private TelemetryParameterDescriptor FindDescriptor(string id)
    {
        if (_viewModel is null)
            throw new InvalidOperationException("Graph pane has no view model.");
        return _viewModel.ParameterOptions.First(option => option.Id == id).Descriptor;
    }

    private void ConfigurePlotStyle()
    {
        PlotControl.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#17232A");
        PlotControl.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#111A20");
        PlotControl.Plot.Axes.Color(ScottPlot.Color.FromHex("#BFCAD0"));
        PlotControl.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#35464F");
        PlotControl.Plot.Legend.BackgroundColor = ScottPlot.Color.FromHex("#D917232A");
        PlotControl.Plot.Legend.FontColor = ScottPlot.Color.FromHex("#E8EEF2");
    }

    private sealed class SeriesState(
        TelemetryParameterDescriptor descriptor,
        IYAxis yAxis,
        ScottPlot.Color color)
    {
        public TelemetryParameterDescriptor Descriptor { get; } = descriptor;
        public IYAxis YAxis { get; } = yAxis;
        public ScottPlot.Color Color { get; } = color;
        public Dictionary<string, double> Categories { get; } = new(StringComparer.Ordinal);
        public List<DataLogger> Segments { get; } = [];
        public int PointCount { get; set; }
        public bool HasGap { get; set; } = true;

        public void TrimTo(Plot plot, int maximumPoints)
        {
            while (PointCount > maximumPoints && Segments.Count > 0)
            {
                DataLogger oldest = Segments[0];
                List<Coordinates> points = oldest.Data.Coordinates;
                int remove = Math.Min(PointCount - maximumPoints, points.Count);
                points.RemoveRange(0, remove);
                PointCount -= remove;
                if (points.Count == 0 && Segments.Count > 1)
                {
                    Segments.RemoveAt(0);
                    plot.Remove(oldest);
                }
            }
        }
    }
}
