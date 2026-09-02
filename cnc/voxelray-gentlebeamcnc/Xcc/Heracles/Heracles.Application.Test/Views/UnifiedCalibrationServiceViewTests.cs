using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Heracles.Ucsi.Models;
using Heracles.Ucsi.Services;
using Heracles.Ucsi.ViewModels;
using Heracles.Ucsi.Views;
using Moq;
using Prism.Mvvm;
using ScottPlot.Plottables;
using ScottPlot.WPF;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Application.UI.UserControls;

namespace Heracles.Application.Test.Views;

[TestFixture]
internal sealed class UnifiedCalibrationServiceViewTests
{
    [OneTimeSetUp]
    public void RegisterViewModelFactory()
    {
        ViewModelLocationProvider.Register<UnifiedCalibrationServiceView>(
            () => new { Graphs = new[] { new object() } });
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void PopulatedVisualTree_LoadsProjectResourcesWithoutHostDictionary()
    {
        var view = new UnifiedCalibrationServiceView();
        view.Measure(new System.Windows.Size(1400, 900));
        view.Arrange(new System.Windows.Rect(0, 0, 1400, 900));
        view.UpdateLayout();
        var refreshTimerField = typeof(UnifiedCalibrationServiceView).GetField(
            "_refreshTimer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var refreshTimer = (System.Windows.Threading.DispatcherTimer?)refreshTimerField?.GetValue(view);
        Assert.That(refreshTimer?.Interval.TotalSeconds, Is.EqualTo(1d / 30d).Within(0.000_001));

        GraphPaneView graphPane = FindVisualChild<GraphPaneView>(view)
            ?? throw new AssertionException("The populated graph pane was not created.");
        var followButton = (ToggleButton)graphPane.FindName("FollowButton");
        var plotControl = (WpfPlot)graphPane.FindName("PlotControl");

        Assert.That(followButton.IsChecked, Is.True);
        var wheelEvent = new MouseWheelEventArgs(Mouse.PrimaryDevice, Environment.TickCount, 120)
        {
            RoutedEvent = UIElement.MouseWheelEvent,
            Source = plotControl,
        };
        plotControl.RaiseEvent(wheelEvent);
        Assert.Multiple(() =>
        {
            Assert.That(followButton.IsChecked, Is.False);
            Assert.That(followButton.Content, Is.EqualTo("Follow Live"));
            Assert.That(wheelEvent.Handled, Is.True);
        });
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public async Task LiveGraph_FollowsThirtySecondsWithoutLoggerResettingNavigation()
    {
        var catalog = new TelemetryParameterCatalog();
        var telemetry = new Mock<ISystemTelemetry>();
        telemetry.SetupGet(value => value.KvFeedback).Returns(70);
        var history = new TelemetryHistoryBuffer();
        history.Append(new UcsiTelemetrySample(
            0,
            DateTimeOffset.UtcNow,
            TimeSpan.FromSeconds(45).Ticks,
            telemetry.Object,
            Array.Empty<FaultEntry>()));
        var coordinator = new Mock<ITelemetrySessionCoordinator>();
        coordinator.SetupGet(value => value.Mode).Returns(UcsiMode.Live);
        coordinator.SetupGet(value => value.LiveHistory).Returns(history);
        coordinator.SetupGet(value => value.CurrentElapsedTicks).Returns(TimeSpan.FromSeconds(45).Ticks);
        var graph = new GraphPaneView
        {
            DataContext = new GraphPaneViewModel(
                1,
                catalog,
                _ => { },
                "system.KvFeedback"),
        };
        graph.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

        await graph.RefreshAsync(coordinator.Object, history.Snapshot());

        var plotControl = (WpfPlot)graph.FindName("PlotControl");
        DataLogger[] loggers = plotControl.Plot.GetPlottables<DataLogger>().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(loggers, Is.Not.Empty);
            Assert.That(loggers, Has.All.Matches<DataLogger>(logger => !logger.ManageAxisLimits));
            Assert.That(plotControl.Plot.Axes.Bottom.Min, Is.EqualTo(15.5).Within(0.001));
            Assert.That(plotControl.Plot.Axes.Bottom.Max, Is.EqualTo(45.5).Within(0.001));
        });
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void EmissionTab_PrecedesHvpsAndContainsRequiredStateControls()
    {
        var view = new UnifiedCalibrationServiceView();
        view.Measure(new System.Windows.Size(1400, 900));
        view.Arrange(new System.Windows.Rect(0, 0, 1400, 900));
        view.UpdateLayout();

        System.Windows.Controls.TabControl tabs = FindVisualChild<System.Windows.Controls.TabControl>(view)
            ?? throw new AssertionException("The primary tab control was not created.");
        string?[] headers = tabs.Items
            .Cast<System.Windows.Controls.TabItem>()
            .Select(item => item.Header?.ToString())
            .ToArray();
        var stateText = (System.Windows.Controls.TextBlock)view.FindName("EmissionStateText");
        var sliders = new[]
        {
            (System.Windows.Controls.Slider)view.FindName("EmissionXSlider"),
            (System.Windows.Controls.Slider)view.FindName("EmissionYSlider"),
            (System.Windows.Controls.Slider)view.FindName("EmissionFocusSlider"),
            (System.Windows.Controls.Slider)view.FindName("EmissionTimerSlider"),
        };

        Assert.Multiple(() =>
        {
            Assert.That(headers.Take(3), Is.EqualTo(new[] { "System Status", "Emission", "HVPS" }));
            Assert.That(stateText.FontWeight, Is.EqualTo(FontWeights.Bold));
            Assert.That(view.FindName("EmissionRunStopButton"), Is.Not.Null);
            Assert.That(view.FindName("EmissionSequenceStatusText"), Is.Not.Null);
            Assert.That(view.FindName("StopCalibrationButton"), Is.Not.Null);
            Assert.That(sliders, Has.All.Matches<System.Windows.Controls.Slider>(
                slider => slider.Orientation == System.Windows.Controls.Orientation.Horizontal));
            Assert.That(view.FindName("EmissionUnavailableOverlay"), Is.Not.Null);
            Assert.That(view.FindName("HvpsUnavailableOverlay"), Is.Not.Null);
            Assert.That(view.FindName("CoilsUnavailableOverlay"), Is.Not.Null);
        });
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void Header_PlacesRadiationHazardIndicatorAtRightEdge()
    {
        var view = new UnifiedCalibrationServiceView();
        view.Measure(new System.Windows.Size(1400, 900));
        view.Arrange(new System.Windows.Rect(0, 0, 1400, 900));
        view.UpdateLayout();

        var indicator = (RadiationHazardControl)view.FindName("RadiationHazardIndicator");

        Assert.Multiple(() =>
        {
            Assert.That(System.Windows.Controls.Grid.GetColumn(indicator), Is.EqualTo(2));
            Assert.That(indicator.HorizontalAlignment, Is.EqualTo(HorizontalAlignment.Right));
            Assert.That(indicator.VerticalAlignment, Is.EqualTo(VerticalAlignment.Center));
            Assert.That(indicator.ActualWidth, Is.EqualTo(38));
            Assert.That(indicator.ActualHeight, Is.EqualTo(38));
        });
    }

    private static T? FindVisualChild<T>(DependencyObject parent)
        where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
                return match;
            if (FindVisualChild<T>(child) is T descendant)
                return descendant;
        }
        return null;
    }
}
