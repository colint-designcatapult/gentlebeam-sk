using System.Windows;
using Heracles.Application.UI.UserControls;
using Moq;
using Xcc.Core.Domain.GryphonBoard;
using Xcc.Core.Enums;

namespace Heracles.Application.Test.Views;

[TestFixture]
internal sealed class CalibrationStateLockoutOverlayTests
{
    [Test]
    [Apartment(ApartmentState.STA)]
    public void CalibrationModeWithoutCalibrationState_DoesNotLockOutContent()
    {
        CalibrationStateLockoutOverlay overlay = CreateOverlay(
            FirmwareMode.Calibration,
            GcbStateNew.Ready);

        Assert.That(overlay.Visibility, Is.EqualTo(Visibility.Collapsed));
    }

    [Test]
    [Apartment(ApartmentState.STA)]
    public void CalibrationStateOutsideCalibrationMode_LocksOutContent()
    {
        CalibrationStateLockoutOverlay overlay = CreateOverlay(
            FirmwareMode.Normal,
            GcbStateNew.Calibration);

        Assert.That(overlay.Visibility, Is.EqualTo(Visibility.Visible));
    }

    private static CalibrationStateLockoutOverlay CreateOverlay(
        FirmwareMode mode,
        GcbStateNew state)
    {
        var telemetry = new Mock<ISystemTelemetry>();
        System.Windows.Application application =
            System.Windows.Application.Current ?? new System.Windows.Application();
        string[] resourceUris =
        [
            "pack://application:,,,/Xcc.Application;Component/UI/Resources/ColorResources.xaml",
            "pack://application:,,,/Xcc.Styles;Component/Styles/FontSizeDefault.xaml",
        ];
        foreach (string resourceUri in resourceUris)
        {
            if (application.Resources.MergedDictionaries.Any(dictionary =>
                    dictionary.Source?.OriginalString.Equals(
                        resourceUri,
                        StringComparison.OrdinalIgnoreCase) == true))
            {
                continue;
            }

            application.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(resourceUri, UriKind.Absolute),
            });
        }

        telemetry.SetupGet(value => value.FirmwareMode).Returns(mode);
        telemetry.SetupGet(value => value.ControlBoardState).Returns(state);

        var overlay = new CalibrationStateLockoutOverlay
        {
            DataContext = new LockoutTestContext
            {
                GcbDataStore = new LockoutGcbDataStore
                {
                    SystemTelemetry = telemetry.Object,
                },
            },
        };
        overlay.Dispatcher.Invoke(
            static () => { },
            System.Windows.Threading.DispatcherPriority.DataBind);
        overlay.Measure(new Size(800, 600));
        overlay.Arrange(new Rect(0, 0, 800, 600));
        overlay.UpdateLayout();

        return overlay;
    }
}

public sealed class LockoutTestContext
{
    public required LockoutGcbDataStore GcbDataStore { get; init; }
}

public sealed class LockoutGcbDataStore
{
    public required ISystemTelemetry SystemTelemetry { get; init; }
}
